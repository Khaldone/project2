// Assets/_Project/4_Bootstrapper/GameEntryPoint.cs
using VContainer.Unity;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Scripting;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Billiards.Presentation
{
    [Preserve]
    public class GameEntryPoint : IStartable
    {
        public void Start()
        {
            // Force script-only stack traces for unhandled exceptions so
            // logcat shows the C# call site, not just the engine frames.
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);

            // Lock to 60 FPS on mobile. Without this, Android defaults to 30.
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0; // Must be 0 for targetFrameRate to take effect

            // Force mobile-optimized quality settings at runtime.
            // This overrides the Editor's Quality Level which may be set to "Ultra".
#if !UNITY_EDITOR
            if (Application.isMobilePlatform)
            {
                // Use the lowest quality level as a base
                QualitySettings.SetQualityLevel(0, true);

                // Shadows are pointless for a 2D top-down game
                QualitySettings.shadows = ShadowQuality.Disable;

                // MSAA: Off. The #1 mobile GPU killer. Not needed for 2D sprite rendering.
                QualitySettings.antiAliasing = 0;

                // Reduce max pixel lights (our game is 2D, we barely need 1)
                QualitySettings.pixelLightCount = 1;

                // Disable skin weights (no 3D skinned meshes in our 2D game)
                QualitySettings.skinWeights = SkinWeights.OneBone;

                Debug.Log("[Bootstrapper] Mobile quality overrides applied.");
            }
#endif

            LoadLoginSceneAsync().Forget();
        }

        private async UniTaskVoid LoadLoginSceneAsync()
        {
            try
            {
                Debug.Log("[Bootstrapper] Engine Wired. Starting Async Load...");

                // Load the Login scene (replaces Scene_Bootstrap)
                await SceneManager.LoadSceneAsync("Scene_Login").ToUniTask();

                // Load the persistent global popup overlay additively (Addressables)
                await UnityEngine.AddressableAssets.Addressables
                    .LoadSceneAsync("UI_Popup", LoadSceneMode.Additive)
                    .ToUniTask();

                Debug.Log("[Bootstrapper] UI_Popup + Login Load Complete.");
            }
            catch (System.Exception ex)
            {
                // On device, this will now show up in logcat instead of being silently swallowed
                Debug.LogError($"[Bootstrapper] CRITICAL: Failed to load Scene_Login: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}