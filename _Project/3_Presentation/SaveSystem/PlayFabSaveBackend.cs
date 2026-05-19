// Assets/Scripts/Presentation/SaveSystem/PlayFabSaveBackend.cs
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class PlayFabSaveBackend : MonoBehaviour, ICloudSaveBackend
{
    public async Task<bool> SaveProfileAsync(PlayerProfileSave profile)
    {
        var tcs = new TaskCompletionSource<bool>();
        string jsonPayload = JsonUtility.ToJson(profile);


        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { "PlayerProfile", jsonPayload } }
        };


        PlayFabClientAPI.UpdateUserData(request,
            result => tcs.SetResult(true),
            error =>
            {
                Debug.LogError("Failed to save to PlayFab: " + error.ErrorMessage);
                tcs.SetResult(false);
            });


        return await tcs.Task;
    }


    public async Task<PlayerProfileSave> LoadProfileAsync()
    {
        var tcs = new TaskCompletionSource<PlayerProfileSave>();


        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null && result.Data.ContainsKey("PlayerProfile"))
                {
                    string json = result.Data["PlayerProfile"].Value;
                    tcs.SetResult(JsonUtility.FromJson<PlayerProfileSave>(json));
                }
                else
                {
                    tcs.SetResult(new PlayerProfileSave()); // New player, return default
                }
            },
            error => tcs.SetResult(new PlayerProfileSave())); // Fallback


        return await tcs.Task;
    }

    UniTask<PlayerProfileSave> ICloudSaveBackend.LoadProfileAsync()
    {
        throw new System.NotImplementedException();
    }

    UniTask<bool> ICloudSaveBackend.SaveProfileAsync(PlayerProfileSave profile)
    {
        throw new System.NotImplementedException();
    }
}
