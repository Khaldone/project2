// Assets/_Project/3_Presentation/Scene_MainMenu/Scripts/MainMenuEntryPoint.cs
using System.Threading;
using UnityEngine;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceProviders; // Required for SceneInstance
using UnityEngine.SceneManagement;
using Billiards.Bootstrapper;
namespace Billiards.Presentation.MainMenu
{
    public class MainMenuEntryPoint : IAsyncStartable
    {
        private readonly UIAddressablesLoader _uiLoader;
        private readonly MainMenuRouter _router;
        private readonly IPlayerDataService _playerDataService;
        private bool _isInitialized = false;

        // NEW: Create "receipts" to hold the loaded scene instances
        private SceneInstance _homeSceneInstance;
        private SceneInstance _shopSceneInstance;
        private SceneInstance _cueShopSceneInstance;
        private SceneInstance _coinShopSceneInstance;
        private SceneInstance _matchMakingSceneInstance;
        private SceneInstance _playerProfileSceneInstance;
        private SceneInstance _citySelectionSceneInstance;
        private SceneInstance _trophyRoadSceneInstance;

        private readonly Billiards.CoreDomain.Monetization.IStoreDataSource _storeDataSource;
        private readonly IStoreOrchestrator _storeOrchestrator;

        public MainMenuEntryPoint(
            UIAddressablesLoader uiLoader, 
            MainMenuRouter router, 
            IPlayerDataService playerDataService,
            Billiards.CoreDomain.Monetization.IStoreDataSource storeDataSource,
            IStoreOrchestrator storeOrchestrator)
        {
            _uiLoader = uiLoader;
            _router = router;
            _playerDataService = playerDataService;
            _storeDataSource = storeDataSource;
            _storeOrchestrator = storeOrchestrator;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            if (_isInitialized) return;
            _isInitialized = true;

            // Pre-fetch achievements in the background while UI scenes load
            _ = _playerDataService.GetAchievementsAsync();
            
            // Note: We don't need to force refresh StoreDataSource here because CBS automatically calls ItemModule.FetchAll upon successful login.

            // Initialize Unity IAP in the background (fetches RM IDs from CBS under the hood)
            _ = _storeOrchestrator.InitializeStoreAsync();

            Debug.Log("[MainMenu] Starting SEQUENTIAL preload of UI Scenes...");

            try
            {
                // UPDATE: Catch the returned SceneInstances and save them
                _homeSceneInstance = await _uiLoader.LoadUISceneAdditiveAsync("Home_Menu");
                _shopSceneInstance = await _uiLoader.LoadUISceneAdditiveAsync("IAP_Scene");
                _cueShopSceneInstance = await _uiLoader.LoadUISceneAdditiveAsync("CueShop_Scene");
                _coinShopSceneInstance = await _uiLoader.LoadUISceneAdditiveAsync("CoinShop_Scene");
                _matchMakingSceneInstance = await _uiLoader.LoadUISceneAdditiveAsync("Matchmaking_Scene");
                _playerProfileSceneInstance = await _uiLoader.LoadUISceneAdditiveAsync("UI_PlayerProfile");
                _citySelectionSceneInstance = await _uiLoader.LoadUISceneAdditiveAsync("UI_CitySelection");
                _trophyRoadSceneInstance = await _uiLoader.LoadUISceneAdditiveAsync("UI_TrophyRoad");

                Debug.Log("[MainMenu] All UI Scenes loaded and safely parented.");

                if (_router.MenuCount > 0)
                {
                    _router.SetInitialMenu<HomeMenu>();
                }
                else
                {
                    Debug.LogError("[EntryPoint] Router dictionary is empty! Registration failed.");
                }

                // THE HANDOFF: Everything is loaded. Destroy the Login scene!
                // Scenario A: We came from the Login Scene
                var loginScene = SceneManager.GetSceneByName("Scene_Login");
                if (loginScene.IsValid() && loginScene.isLoaded)
                {
                    Debug.Log("[MainMenu] Login scene detected. Tearing it down...");
                    await SceneManager.UnloadSceneAsync("Scene_Login");
                }
                // Scenario B: We came from the Game Arena
                var arenaScene = SceneManager.GetSceneByName("02_Spoke_GameArena");
                if (arenaScene.IsValid() && arenaScene.isLoaded)
                {
                    Debug.Log("[MainMenu] Game Arena scene detected. Tearing it down...");
                    await SceneManager.UnloadSceneAsync("02_Spoke_GameArena");
                }

            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MainMenu] Preload failed: {e.Message}");
            }
        }

        // UPDATE: The Transition Method now uses the saved receipts
        public async UniTask TransitionToGameArenaAsync()
        {
            Debug.Log("[MainMenu] Tearing down menus to load Game Arena...");
            
            // Clear cached achievements so next visit pulls fresh data
            _playerDataService.ClearAchievementsCache();

            // 1. Hand the receipts back to Addressables to unload the UI scenes
            await _uiLoader.UnloadUISceneAsync(_homeSceneInstance);
            await _uiLoader.UnloadUISceneAsync(_shopSceneInstance);
            await _uiLoader.UnloadUISceneAsync(_cueShopSceneInstance);
            await _uiLoader.UnloadUISceneAsync(_coinShopSceneInstance);
            await _uiLoader.UnloadUISceneAsync(_matchMakingSceneInstance);
            await _uiLoader.UnloadUISceneAsync(_playerProfileSceneInstance);
            await _uiLoader.UnloadUISceneAsync(_citySelectionSceneInstance);
            await _uiLoader.UnloadUISceneAsync(_trophyRoadSceneInstance);

            // 2. Load the Game Arena additively (preserves UI_Popup and root scope)
            await SceneManager.LoadSceneAsync("02_Spoke_GameArena", LoadSceneMode.Additive);

            // 3. Set the Arena as the active scene (so new GameObjects spawn there)
            var arenaScene = SceneManager.GetSceneByName("02_Spoke_GameArena");
            if (arenaScene.IsValid())
            {
                SceneManager.SetActiveScene(arenaScene);
            }

            // 4. Unload the MainMenu bootstrap scene
            var menuBootstrap = SceneManager.GetSceneByName("Scene_MainMenu_Bootstrap");
            if (menuBootstrap.IsValid() && menuBootstrap.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(menuBootstrap);
            }
        }
    }
}