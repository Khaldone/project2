// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Auth/EditorMockAuthWrapper.cs
#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Billiards.Infrastructure.Authentication.NativeAuth
{
    public class EditorMockAuthWrapper : INativeAuthService
    {
        public string PlatformName => "Developer Sandbox";
        public string ButtonIconKey => "icon_dev";

        public async UniTask<string> AuthenticateAsync()
        {
            Debug.Log("AAA Pipeline: Editor Login Simulated. Bypassing native auth.");
            await UniTask.Delay(500); // Simulate network wait
            return "editor_mock_token_999";
        }

        public UniTask<string> TrySilentAuthenticateAsync()
        {
            // In the editor, we don't want to auto-login silently so we can test the UI.
            return UniTask.FromResult<string>(null);
        }
    }
}
#endif