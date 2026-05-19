// Assets/_Project/2_Infrastructure/Backend/PlayFabAuthService.cs
using Billiards.CoreDomain.Services;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Billiards.Infrastructure.Authentication
{
    public class PlayFabAuthService : IAuthenticationService_New
    {
        private readonly PlayerSession _session;

        // VContainer automatically injects the Session from the Project Root!
        public PlayFabAuthService(PlayerSession session)
        {
            _session = session;
        }

        public async UniTask<bool> AuthenticateAsync()
        {
            try
            {
                // 1. Step One: Log in to PlayFab
                var loginResult = await PerformPlayFabLoginAsync();

                // 2. We are in! Create the initial struct with the User ID
                var profile = new PlayerProfile
                {
                    UserId = loginResult.PlayFabId
                };

                // 3. Step Two: Fetch the deeper Player Profile data
                var profileResult = await FetchPlayFabProfileAsync();

                // 4. Map the PlayFab data into your custom struct
                // (We use safe fallbacks in case the player is brand new and has no name yet)
                profile.DisplayName = string.IsNullOrEmpty(profileResult.PlayerProfile.DisplayName)
                                      ? "New Challenger"
                                      : profileResult.PlayerProfile.DisplayName;

                // Example mapping (You can pull these from PlayFab's Player Data or Statistics later)
                profile.Level = 1;
                profile.CountryCode = "US";

                // 5. THE CRITICAL MOMENT: Save the fully populated struct into the Global Vault
                _session.CurrentProfile = profile;

                Debug.Log($"[PlayFab] Successfully authenticated and loaded profile for: {profile.DisplayName}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PlayFab] Authentication Failed: {ex.Message}");
                return false; // This tells your LoginEntryPoint to show the Error popup
            }
        }

        // --- Helper Methods to convert PlayFab Callbacks into UniTasks ---

        private UniTask<LoginResult> PerformPlayFabLoginAsync()
        {
            var completionSource = new UniTaskCompletionSource<LoginResult>();

            // Example using Device ID. Change to Email/Password or Google Auth as needed.
            var request = new LoginWithCustomIDRequest
            {
                CustomId = SystemInfo.deviceUniqueIdentifier,
                CreateAccount = true
            };

            PlayFabClientAPI.LoginWithCustomID(request,
                result => completionSource.TrySetResult(result),
                error => completionSource.TrySetException(new System.Exception(error.ErrorMessage))
            );

            return completionSource.Task;
        }

        private UniTask<GetPlayerProfileResult> FetchPlayFabProfileAsync()
        {
            var completionSource = new UniTaskCompletionSource<GetPlayerProfileResult>();

            var request = new GetPlayerProfileRequest
            {
                // Tell PlayFab what specific pieces of the profile we want it to return
                ProfileConstraints = new PlayerProfileViewConstraints
                {
                    ShowDisplayName = true,
                    ShowLocations = true
                }
            };

            PlayFabClientAPI.GetPlayerProfile(request,
                result => completionSource.TrySetResult(result),
                error => completionSource.TrySetException(new System.Exception(error.ErrorMessage))
            );

            return completionSource.Task;
        }

        public UniTask<bool> AuthenticateWithNativeTokenAsync(string platformName, string token)
        {
            throw new System.NotImplementedException();
        }

        public UniTask<bool> AutoLoginAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}