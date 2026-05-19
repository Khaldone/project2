// Assets/_Project/2_Infrastructure/Authentication/CbsAuthServiceWrapper.cs
using Billiards.CoreDomain.Services;
using CBS;
using CBS.Models;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Billiards.Infrastructure.Authentication
{
    public class CbsAuthServiceWrapper : IAuthenticationService_New
    {
        private readonly PlayerSession _session;

        public CbsAuthServiceWrapper(PlayerSession session)
        {
            _session = session;
        }

        public async UniTask<bool> AuthenticateAsync()
        {
            var tcs = new UniTaskCompletionSource<bool>();
            var authModule = CBSModule.Get<CBSAuthModule>();

            // Use CBS's native Device Login (mirrors old PlayFabCustomID login)
            authModule.LoginWithDevice(result =>
            {
                if (result.IsSuccess)
                {
                    var profileModule = CBSModule.Get<CBSProfileModule>();
                    
                    var profile = new PlayerProfile
                    {
                        UserId = result.ProfileID,
                        DisplayName = string.IsNullOrEmpty(profileModule.DisplayName) ? "New Challenger" : profileModule.DisplayName,
                        Level = 1, // Fallbacks
                        CountryCode = "US"
                    };

                    _session.CurrentProfile = profile;
                    Debug.Log($"[CBS Auth] Successfully authenticated and loaded profile for: {profile.DisplayName}");
                    tcs.TrySetResult(true);
                }
                else
                {
                    Debug.LogError($"[CBS Auth] Authentication Failed: {result.Error?.Message}");
                    tcs.TrySetResult(false);
                }
            });

            return await tcs.Task;
        }

        public async UniTask<bool> AuthenticateWithNativeTokenAsync(string platformName, string token)
        {
            var tcs = new UniTaskCompletionSource<bool>();
            var authModule = CBSModule.Get<CBSAuthModule>();

            switch (platformName)
            {
                case "Google":
                    // CBS method name has a typo: "LoginWithGoolge"
                    authModule.LoginWithGoolge(token, result =>
                    {
                        if (result.IsSuccess)
                        {
                            HydrateSession(result);
                            tcs.TrySetResult(true);
                        }
                        else
                        {
                            Debug.LogError($"[CBS Auth] Google login failed: {result.Error?.Message}");
                            tcs.TrySetResult(false);
                        }
                    });
                    break;

                case "Apple":
                    authModule.LoginWithApple(token, result =>
                    {
                        if (result.IsSuccess)
                        {
                            HydrateSession(result);
                            tcs.TrySetResult(true);
                        }
                        else
                        {
                            Debug.LogError($"[CBS Auth] Apple login failed: {result.Error?.Message}");
                            tcs.TrySetResult(false);
                        }
                    });
                    break;

                default:
                    // Developer Sandbox / Editor mock falls through here
                    if (platformName == "Developer Sandbox")
                    {
                        Debug.Log($"[CBS Auth] Editor Mock token '{token}' accepted. Faking login...");
                        
                        var profile = new PlayerProfile
                        {
                            UserId = "Editor_Mock_ID",
                            DisplayName = "Editor Tester",
                            Level = 99,
                            CountryCode = "US"
                        };
                        _session.CurrentProfile = profile;
                        tcs.TrySetResult(true);
                    }
                    else
                    {
                        Debug.LogError($"[CBS Auth] Unsupported platform: {platformName}");
                        tcs.TrySetResult(false);
                    }
                    break;
            }

            return await tcs.Task;
        }
        
        /// <summary>
        /// Extracts profile data from a successful CBS login result and populates the PlayerSession.
        /// </summary>
        private void HydrateSession(CBSLoginResult result)
        {
            var profileModule = CBSModule.Get<CBSProfileModule>();

            var profile = new PlayerProfile
            {
                UserId = result.ProfileID,
                DisplayName = string.IsNullOrEmpty(profileModule.DisplayName) ? "New Challenger" : profileModule.DisplayName,
                Level = 1,
                CountryCode = "US"
            };

            _session.CurrentProfile = profile;
            Debug.Log($"[CBS Auth] Successfully authenticated and loaded profile for: {profile.DisplayName}");
        }

        public async UniTask<bool> AutoLoginAsync()
        {
            var tcs = new UniTaskCompletionSource<bool>();
            var authModule = CBSModule.Get<CBSAuthModule>();

            authModule.AutoLogin(result =>
            {
                if (result.IsSuccess)
                {
                    HydrateSession(result);
                    tcs.TrySetResult(true);
                }
                else
                {
                    Debug.Log($"[CBS Auth] AutoLogin failed: {result.Error?.Message}");
                    tcs.TrySetResult(false);
                }
            });

            return await tcs.Task;
        }
    }
}
