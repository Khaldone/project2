// Assets/_Project/2_Infrastructure/Backend/ImgurAvatarService.cs
using Billiards.CoreDomain.Services;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Threading;
using UnityEngine;

namespace Billiards.Infrastructure.Backend
{
    public class ImgurAvatarService : IAvatarService
    {
        private const string ImgurClientId = "f33a99e5ef19d4a";
        private const string ImgurClientSecret = "ff44ce65ad52ed5b29a672631a90a56b66aae122";

        // Simple struct for JSON parsing
        [Serializable]
        private class ImgurResponse
        {
            public ImgurData data;
            public bool success;
        }

        [Serializable]
        private class ImgurData
        {
            public string link;
            public string deletehash;
        }

        public async UniTask<string> UploadAvatarAsync(byte[] imageData, CancellationToken token)
        {
            // 1. Upload to Imgur natively via UnityWebRequest to avoid HttpClient / User-Agent blocks
            Debug.Log("[ImgurAvatarService] Uploading avatar to Imgur...");

            WWWForm form = new WWWForm();
            form.AddBinaryData("image", imageData, "avatar.png", "image/png");
            form.AddField("type", "file");

            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Post("https://api.imgur.com/3/image", form))
            {
                // Imgur requires the Authorization header for anonymous uploads
                www.SetRequestHeader("Authorization", "Client-ID " + ImgurClientId);
                
                await www.SendWebRequest().ToUniTask(cancellationToken: token);

                if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    string errorMsg = $"[ImgurAvatarService] Upload failed: {www.error}. Response: {www.downloadHandler.text}";
                    Debug.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                var imgurResponse = JsonUtility.FromJson<ImgurResponse>(www.downloadHandler.text);
                if (imgurResponse == null || !imgurResponse.success)
                {
                    throw new Exception("Imgur upload failed or returned invalid JSON.");
                }

                string link = imgurResponse.data.link;
                string deletehash = imgurResponse.data.deletehash;
                Debug.Log($"[ImgurAvatarService] Uploaded to Imgur successfully: {link} (DeleteHash: {deletehash})");

                // 2. Save to PlayFab (AvatarUrl + DeleteHash in UserData)
                var request = new UpdateAvatarUrlRequest { ImageUrl = link };
                var tcs = new UniTaskCompletionSource<bool>();

                PlayFabClientAPI.UpdateAvatarUrl(request, 
                    result => 
                    {
                        // Also save the delete hash to UserData
                        var dataRequest = new UpdateUserDataRequest
                        {
                            Data = new System.Collections.Generic.Dictionary<string, string> { { "AvatarDeleteHash", deletehash } }
                        };
                        PlayFabClientAPI.UpdateUserData(dataRequest, 
                            dataResult => 
                            {
                                Debug.Log("[ImgurAvatarService] Avatar and DeleteHash Updated Successfully in PlayFab.");
                                tcs.TrySetResult(true);
                            },
                            dataError => 
                            {
                                Debug.LogError("[ImgurAvatarService] Failed to save delete hash: " + dataError.GenerateErrorReport());
                                tcs.TrySetResult(true); // Still return true since URL updated
                            });
                    }, 
                    error => 
                    {
                        Debug.LogError("[ImgurAvatarService] Failed to update avatar in PlayFab: " + error.GenerateErrorReport());
                        tcs.TrySetException(new Exception(error.GenerateErrorReport()));
                    });

                await tcs.Task.AttachExternalCancellation(token);

                return link;
            }
        }

        public async UniTask<string> GetPlayerAvatarUrlAsync(CancellationToken token)
        {
            var tcs = new UniTaskCompletionSource<string>();

            PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest
            {
                ProfileConstraints = new PlayerProfileViewConstraints
                {
                    ShowAvatarUrl = true
                }
            },
            result =>
            {
                string avatarUrl = result.PlayerProfile?.AvatarUrl;
                tcs.TrySetResult(avatarUrl);
            },
            error =>
            {
                Debug.LogError("[ImgurAvatarService] Failed to fetch player profile: " + error.GenerateErrorReport());
                tcs.TrySetResult(null); // Return null rather than crashing
            });

            return await tcs.Task.AttachExternalCancellation(token);
        }

        public async UniTask<string> GetAvatarDeleteHashAsync(CancellationToken token)
        {
            var tcs = new UniTaskCompletionSource<string>();

            PlayFabClientAPI.GetUserData(new GetUserDataRequest
            {
                Keys = new System.Collections.Generic.List<string> { "AvatarDeleteHash" }
            },
            result =>
            {
                if (result.Data != null && result.Data.TryGetValue("AvatarDeleteHash", out var record))
                {
                    tcs.TrySetResult(record.Value);
                }
                else
                {
                    tcs.TrySetResult(null);
                }
            },
            error =>
            {
                Debug.LogError("[ImgurAvatarService] Failed to fetch delete hash: " + error.GenerateErrorReport());
                tcs.TrySetResult(null);
            });

            return await tcs.Task.AttachExternalCancellation(token);
        }

        public async UniTask<bool> DeleteAvatarAsync(string deleteHash, CancellationToken token)
        {
            if (string.IsNullOrEmpty(deleteHash)) return false;

            Debug.Log($"[ImgurAvatarService] Deleting avatar with hash {deleteHash} from Imgur...");

            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Delete($"https://api.imgur.com/3/image/{deleteHash}"))
            {
                www.SetRequestHeader("Authorization", "Client-ID " + ImgurClientId);
                
                await www.SendWebRequest().ToUniTask(cancellationToken: token);

                if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[ImgurAvatarService] Failed to delete from Imgur: {www.error}. Response: {www.downloadHandler?.text}");
                    return false;
                }

                Debug.Log("[ImgurAvatarService] Successfully deleted avatar from Imgur.");
                
                // Clear the delete hash from PlayFab
                var dataRequest = new UpdateUserDataRequest
                {
                    KeysToRemove = new System.Collections.Generic.List<string> { "AvatarDeleteHash" }
                };
                PlayFabClientAPI.UpdateUserData(dataRequest, null, null);

                return true;
            }
        }
    }
}
