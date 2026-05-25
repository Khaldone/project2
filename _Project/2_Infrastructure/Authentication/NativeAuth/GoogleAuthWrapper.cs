// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Auth/GoogleAuthWrapper.cs
#if UNITY_ANDROID
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

namespace Billiards.Infrastructure.Authentication.NativeAuth
{
    public class GoogleAuthWrapper : INativeAuthService
    {
        public string PlatformName => "Google";
        public string ButtonIconKey => "icon_google";

        // OPEN_ID is required so Google's OAuth code-exchange response includes an
        // id_token. PlayFab's LoginWithGoogleAccount rejects the login otherwise
        // (GoogleOAuthNoIdTokenIncludedInResponse). EMAIL/PROFILE are conventional
        // companions and let PlayFab populate the player's display info.
        private static readonly List<AuthScope> RequiredScopes = new List<AuthScope>
        {
            AuthScope.OPEN_ID,
            AuthScope.EMAIL,
            AuthScope.PROFILE,
        };

        private static bool _initialized = false;

        private static void InitializePlatform()
        {
            if (_initialized) return;
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();
            _initialized = true;
            Debug.Log("[GoogleAuthWrapper] PlayGamesPlatform Activated and Debug Logs Enabled.");
        }

        public async UniTask<string> AuthenticateAsync()
        {
            InitializePlatform();
            Debug.Log("AAA Pipeline: Triggering Google Play Games prompt...");

            var tcs = new UniTaskCompletionSource<string>();

            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                Debug.Log($"[GoogleAuthWrapper] Authenticate callback fired. Status: {status}");

                if (status == SignInStatus.Success)
                {
                    PlayGamesPlatform.Instance.RequestServerSideAccess(false, RequiredScopes, response =>
                    {
                        var grantedScopes = response.GetGrantedScopes();
                        var grantedNames = grantedScopes.Count > 0
                            ? string.Join(",", grantedScopes)
                            : "<none>";
                        Debug.Log($"[GoogleAuthWrapper] Granted scopes: {grantedNames}");

                        if (!grantedScopes.Contains(AuthScope.OPEN_ID))
                        {
                            Debug.LogError("[GoogleAuthWrapper] OPEN_ID was NOT granted. The user declined the consent screen, or OPEN_ID isn't enabled on the Web OAuth client. PlayFab will reject this with GoogleOAuthNoIdTokenIncludedInResponse.");
                        }

                        var code = response.GetAuthCode();
                        if (string.IsNullOrEmpty(code))
                        {
                            // Empty code = OAuth Web Client ID misconfigured in GPGS Setup,
                            // or Linked Web App missing in Google Play Console, or already-consumed code.
                            Debug.LogError("[GoogleAuthWrapper] RequestServerSideAccess returned an EMPTY code. Check (1) GPGS Setup window has the Web OAuth Client ID, (2) Google Play Console has a Linked Web App pointing at that OAuth client, (3) the SHA-1 of the signing keystore is registered on that OAuth client.");
                            tcs.TrySetException(new System.Exception("Google serverAuthCode is empty — OAuth/web-client misconfiguration."));
                            return;
                        }

                        Debug.Log($"[GoogleAuthWrapper] Google serverAuthCode obtained (length={code.Length}).");
                        tcs.TrySetResult(code);
                    });
                }
                else
                {
                    Debug.LogError($"[GoogleAuthWrapper] Google Play Games authentication failed with status: {status}");
                    tcs.TrySetException(new System.Exception($"Google Play Games authentication failed with status: {status}"));
                }
            });

            return await tcs.Task;
        }

        public async UniTask<string> TrySilentAuthenticateAsync()
        {
            InitializePlatform();
            Debug.Log("[GoogleAuthWrapper] Attempting silent authentication...");
            var tcs = new UniTaskCompletionSource<string>();

            // GPGS v11+ Authenticate() is silent-first by design.
            // If the user has Google Play Games on the device and has previously
            // signed in, this returns Success immediately with zero UI.
            // If they have never signed in, it returns Canceled — no prompt shown.
            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                Debug.Log($"[GoogleAuthWrapper] Silent auth status: {status}");

                if (status == SignInStatus.Success)
                {
                    // User is signed into Google Play Games at the OS level.
                    // Now grab the server auth code so CBS/PlayFab can verify it.
                    PlayGamesPlatform.Instance.RequestServerSideAccess(false, RequiredScopes, response =>
                    {
                        var code = response.GetAuthCode();
                        if (!string.IsNullOrEmpty(code))
                        {
                            Debug.Log($"[GoogleAuthWrapper] Silent auth obtained serverAuthCode (length={code.Length}).");
                            tcs.TrySetResult(code);
                        }
                        else
                        {
                            Debug.LogWarning("[GoogleAuthWrapper] Silent auth succeeded but serverAuthCode was empty.");
                            tcs.TrySetResult(null);
                        }
                    });
                }
                else
                {
                    // Not signed into Google Play Games — fall back to manual buttons.
                    Debug.Log("[GoogleAuthWrapper] Silent auth returned non-success. User will see login buttons.");
                    tcs.TrySetResult(null);
                }
            });

            return await tcs.Task;
        }
    }
}
#endif