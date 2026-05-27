// Assets/_Project/3_Presentation/Scene_MainMenu/Scripts/MainMenuEntryPoint.cs
using System.Threading;
using UnityEngine;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceProviders; // Required for SceneInstance
using UnityEngine.SceneManagement;
using Billiards.Bootstrapper;
using Billiards.CoreDomain.Services;

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
        private readonly IAppReviewService _appReviewService;
        private readonly AppReviewConfig _appReviewConfig;

        public MainMenuEntryPoint(
            UIAddressablesLoader uiLoader, 
            MainMenuRouter router, 
            IPlayerDataService playerDataService,
            Billiards.CoreDomain.Monetization.IStoreDataSource storeDataSource,
            IStoreOrchestrator storeOrchestrator,
            IAppReviewService appReviewService,
            AppReviewConfig appReviewConfig)
        {
            _uiLoader = uiLoader;
            _router = router;
            _playerDataService = playerDataService;
            _storeDataSource = storeDataSource;
            _storeOrchestrator = storeOrchestrator;
            _appReviewService = appReviewService;
            _appReviewConfig = appReviewConfig;
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

            CheckAppReviewPromptAsync().Forget();
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

        private async UniTaskVoid CheckAppReviewPromptAsync()
        {
            try
            {
                Debug.Log("[MainMenuEntryPoint] CheckAppReviewPromptAsync invoked. Awaiting AppReviewService initialization...");
                
                // CRITICAL FIX: The bootstrapper fires InitializeAsync().Forget().
                // We MUST await it to guarantee the persisted LaunchCount is loaded
                // before we evaluate IsEligibleForReview.
                await _appReviewService.InitializeAsync();

                Debug.Log($"[MainMenuEntryPoint] Checking AppReview eligibility. LaunchCount={_appReviewService.LaunchCount}, HasReviewed={_appReviewService.HasReviewed}, IsEligible={_appReviewService.IsEligibleForReview}");
                if (!_appReviewService.IsEligibleForReview)
                {
                    Debug.Log("[MainMenuEntryPoint] User is not eligible for review prompt. Returning.");
                    return;
                }

                // Resolve the notification queue early so we can check its state.
                var queue = ResolveNotificationQueue();
                if (queue == null)
                {
                    Debug.LogError("[MainMenuEntryPoint] Could not resolve INotificationQueue! UI_Popup scene may not be loaded.");
                    return;
                }
                Debug.Log("[MainMenuEntryPoint] Resolved INotificationQueue.");

                // Wait for the login StatusOverlay dismiss to fully complete.
                // On Android the dismiss animation + NotifyDisplayFinished cycle
                // can still be in-flight when this code runs — a fixed delay was
                // a race condition. Poll until the queue is fully idle (no active
                // notification AND no pending items).
                int waitFrames = 0;
                const int maxWaitFrames = 300; // ~5 seconds at 60fps
                while (!queue.IsIdle && waitFrames < maxWaitFrames)
                {
                    await UniTask.Yield();
                    waitFrames++;
                }
                if (waitFrames > 0)
                {
                    Debug.Log($"[MainMenuEntryPoint] Waited {waitFrames} frames for notification queue to become idle.");
                }

                // Additional grace period for the UI to settle after login → main menu.
                await UniTask.Delay(2000);

                // IMPORTANT: UNITY_EDITOR must come FIRST. When the Editor has Android
                // as the active build target, BOTH UNITY_EDITOR and UNITY_ANDROID are
                // defined — the first matching #if branch wins.
                bool useNative = false;
#if UNITY_EDITOR
                // In the Editor, always use the custom notification popup for testing.
                useNative = false;
#elif UNITY_ANDROID
                useNative = _appReviewConfig.UseNativeAndroidReviewPopUp;
#elif UNITY_IOS
                useNative = _appReviewConfig.UseNativeIosReviewPopUp;
#endif
                bool useNativeDirectly = useNative && !_appReviewConfig.ShowCustomPopupFirst;
                Debug.Log($"[MainMenuEntryPoint] Eligibility checked positive. useNative={useNative}, useNativeDirectly={useNativeDirectly}");

                if (useNativeDirectly)
                {
                    Debug.Log("[MainMenuEntryPoint] Prompting native store review directly.");
                    bool success = await _appReviewService.RequestNativeReviewAsync();
                    Debug.Log($"[MainMenuEntryPoint] RequestNativeReviewAsync status: {success}");
                    // Reset the counter so the prompt recurs after another N launches.
                    await _appReviewService.PostponeReviewAsync();
                }
                else
                {
                    Debug.Log("[MainMenuEntryPoint] Custom popup first or native review disabled. Prompting custom rating notification layout.");
                    Debug.Log("[MainMenuEntryPoint] Enqueuing rating notification...");
                    queue.Enqueue(new NotificationData
                    {
                        Type = enNotificationType.SystemWarning,
                        Classification = NotificationClassification.Info,
                        Layout = NotificationLayout.Rating,
                        SlideIn = NotificationSlideDirection.Top,
                        SlideOut = NotificationSlideDirection.Top,
                        Title = "Enjoying the Game?",
                        Message = "Please take a moment to rate us! Your feedback helps us improve.",
                        DisplayDurationSeconds = 60,
                        OnRatingResolved = (stars) =>
                        {
                            // Wrap in UniTaskVoid to prevent silent async void crashes.
                            HandleRatingResolved(stars).Forget();
                        }
                    });
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MainMenuEntryPoint] CheckAppReviewPromptAsync FAILED: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async UniTaskVoid HandleRatingResolved(int stars)
        {
            Debug.Log($"[MainMenuEntryPoint] Custom rating notification resolved with stars: {stars}");
            try
            {
                if (stars <= 0)
                {
                    // 0 stars = "Maybe Later" tapped (or timeout).
                    Debug.Log("[MainMenuEntryPoint] Rating postponed. Resetting LaunchCountsBeforeShowingPopup counter.");
                    await _appReviewService.PostponeReviewAsync();
                    return;
                }

                bool isPositiveRating = stars >= _appReviewConfig.LowRatingThreshold;

                if (isPositiveRating)
                {
                    // ── Native Google Play In-App Review dialog ───────────────────────────
                    // This is the ONLY mechanism that keeps the user inside the app.
                    // It renders as an Android system dialog drawn on top of the Unity
                    // surface (the popup in the reference image). No URI scheme —
                    // market://, https://, or otherwise — achieves this; Application.OpenURL()
                    // always fires an Android Intent that switches the foreground application.
                    //
                    // IMPORTANT: Google enforces strict conditions before showing the dialog:
                    //   • The app must be installed from the Play Store (not sideloaded)
                    //   • The package name must be published on at least one track
                    //     (Internal Testing, Alpha, Beta, or Production)
                    //   • The signed-in Google account must be an authorised tester on that track
                    //
                    // During development (ADB-installed APK) the API returns NoError silently
                    // and shows nothing. The dialog WILL appear correctly in production.
                    Debug.Log($"[MainMenuEntryPoint] Positive rating ({stars}★). Requesting native Google Play review dialog...");
                    await _appReviewService.RequestNativeReviewAsync();

                    // Permanently mark as reviewed — this player is never prompted again.
                    await _appReviewService.MarkAsReviewedAsync();
                }
                else
                {
                    // ── Low-rated path (< LowRatingThreshold stars) ───────────────────────
                    // Do NOT open the store. Show an instant in-app feedback message so
                    // the player feels heard. We reset the counter so we can re-engage
                    // them after they've had a better experience.
                    Debug.Log($"[MainMenuEntryPoint] Negative rating ({stars}★ < threshold {_appReviewConfig.LowRatingThreshold}). Showing in-app feedback message...");

                    var queue = ResolveNotificationQueue();
                    if (queue != null)
                    {
                        string feedbackMsg = !string.IsNullOrEmpty(_appReviewConfig.LowRatingFeedbackMessage)
                            ? _appReviewConfig.LowRatingFeedbackMessage
                            : "We're sorry to hear that! Please contact our support team so we can improve.";

                        queue.Enqueue(new NotificationData
                        {
                            Type = enNotificationType.SystemWarning,
                            Classification = NotificationClassification.Info,
                            Layout = NotificationLayout.Standard,
                            // Immediate = no slide animation (same as "Login Successful" banner)
                            SlideIn = NotificationSlideDirection.Immediate,
                            SlideOut = NotificationSlideDirection.Top,
                            Title = "Thank You for Your Feedback",
                            Message = feedbackMsg,
                            DisplayDurationSeconds = 8,
                        });
                    }

                    // Reset the counter — this player may rate higher after a better session.
                    await _appReviewService.PostponeReviewAsync();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MainMenuEntryPoint] Error handling rating callback: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private INotificationQueue ResolveNotificationQueue()
        {
            var scopes = Object.FindObjectsByType<VContainer.Unity.LifetimeScope>(FindObjectsSortMode.None);
            foreach (var scope in scopes)
            {
                if (scope.Container == null) continue;
                try
                {
                    var queue = (INotificationQueue)scope.Container.Resolve(typeof(INotificationQueue));
                    if (queue != null) return queue;
                }
                catch { /* Ignore */ }
            }
            return null;
        }
    }
}
