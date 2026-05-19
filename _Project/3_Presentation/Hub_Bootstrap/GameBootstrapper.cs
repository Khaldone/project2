// Assets/Scripts/Presentation/Hub_Bootstrap/GameBootstrapper.cs
using System.IO;
using UnityEngine;
using VContainer.Unity;
using Billiards.CoreDomain.Services;
public class GameBootstrapper : IStartable
{
    private readonly ISceneLoaderHubSpokes _sceneLoader;
    // Inject other services like ICloudSaveBackend for initial login

    private readonly IAssetDeliveryService _assetDelivery;
    public GameBootstrapper(ISceneLoaderHubSpokes sceneLoader)
    {
        _sceneLoader = sceneLoader;
    }

    public async void Start()
    {
        Debug.Log("AAA Pipeline: Hub Services Initialized. Authenticating...");
        // await _saveBackend.InitializeAsync();
        Debug.Log("AAA Pipeline: Booting...");

        // 1. Fetch the catalog (which tells the game where the CDN is)
        //await _assetDelivery.InitializeAsync();

        // 2. Load the Spoke Scene
        await _sceneLoader.LoadSpokeSceneAsync("01_MainMenu");

        // --- THE EDITOR FAST-FORWARD ---
#if UNITY_EDITOR
        string targetSpokePath = UnityEditor.EditorPrefs.GetString("AutoLoad_TargetSpokeScene", "");

        if (!string.IsNullOrEmpty(targetSpokePath))
        {
            // Wipe the memory so it doesn't get stuck in a loop
            UnityEditor.EditorPrefs.DeleteKey("AutoLoad_TargetSpokeScene");


            // Extract the scene name (e.g., "02_GameArena") from the file path
            string sceneName = Path.GetFileNameWithoutExtension(targetSpokePath);

            Debug.Log($"AAA Pipeline: Editor Auto-Load Intercepted. Fast-forwarding to {sceneName}...");
            await _sceneLoader.LoadSpokeSceneAsync(sceneName);
            return;
        }
#endif
        // --- NORMAL PRODUCTION BOOT ---

        // If not in the Editor, or if they started from the Hub normally,
        // proceed to the default Lobby.
        await _sceneLoader.LoadSpokeSceneAsync("01_MainMenu");
    }
}
