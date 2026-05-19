//// Assets/_Project/1_CoreDomain/Bootstrap/AppBootstrapper.cs
//using Cysharp.Threading.Tasks;
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using UnityEngine;
//using VContainer.Unity;


//public class AppBootstrapper_New : IAsyncStartable
//{
//    // --- DEPENDENCIES ---
//    // Injected automatically by VContainer from the HubLifetimeScope
//    private readonly IAssetDeliveryService _assetDelivery;
//    private readonly IPushNotificationService _pushService;
//    private readonly ILocalSaveService _saveService;
//    private readonly ISettingsOrchestrator _settingsOrchestrator;
//    //private readonly IPlayFabAuthService _authService;
//    private readonly IUIRouter _uiRouter;
//    private readonly IMessageBroker _messageBroker;


//    public AppBootstrapper_New(
//        IAssetDeliveryService assetDelivery,
//        IPushNotificationService pushService,
//        ILocalSaveService saveService,
//        ISettingsOrchestrator settingsOrchestrator,
//        //IPlayFabAuthService authService,
//        IUIRouter uiRouter,
//        IMessageBroker messageBroker)
//    {
//        _assetDelivery = assetDelivery;
//        _pushService = pushService;
//        _saveService = saveService;
//        _settingsOrchestrator = settingsOrchestrator;
//        //_authService = authService;
//        _uiRouter = uiRouter;
//        _messageBroker = messageBroker;
//    }


//    // VContainer automatically calls this the moment the Hub scene finishes loading
//    public async Task StartAsync()
//    {
//        Debug.Log("[BOOT] Sequence Initiated...");


//        try
//        {
//            // STEP 1: Initialize the Asset Pipeline (Addressables)
//            // We must do this first so we can load loading screens or error popups if needed.
//            Debug.Log("[BOOT] 1. Initializing Asset Delivery...");
//            await _assetDelivery.InitializeAsync();


//            // STEP 2: Initialize Hardware & OS Services
//            // Checks for Google Play Services on Android and requests notification permissions on iOS
//            Debug.Log("[BOOT] 2. Initializing Hardware Services...");
//            //bool isFirebaseReady = await _pushService.InitializeAsync();


//            // STEP 3: Load Local Device Data
//            // Decrypts AES local save to get volume levels, language prefs, and cached login tokens
//            Debug.Log("[BOOT] 3. Hydrating Local Settings...");
//            //await _settingsOrchestrator.LoadLocalSettingsAsync();


//            // STEP 4: Attempt Server Authentication
//            // Try to log in silently using the device ID or a cached Google/Apple token
//            Debug.Log("[BOOT] 4. Authenticating with PlayFab...");
//        }
//        //bool isLoggedIn = await _authService.TrySilentLoginAsync();


//        //    if (isLoggedIn)
//        //    {
//        //        // STEP 5A: Silent Login Succeeded!
//        //        Debug.Log("[BOOT] Auth Success. Finalizing server sync...");


//        //        // Tell PlayFab our device token so they can send us push notifications
//        //        if (isFirebaseReady)
//        //        {
//        //            await _authService.RegisterPushNotificationTokenAsync(_pushService.CurrentDeviceToken);
//        //        }


//        //        // Broadcast that the app is fully online (Triggers Audio to start playing music, etc.)
//        //        _messageBroker.Publish(new ApplicationBootCompleteMessage(Success: true));


//        //        // Route directly to the 3D Main Menu
//        //        Debug.Log("[BOOT] Routing to Main Menu.");
//        //        _uiRouter.LoadScene("01_Spoke_MainMenu");
//        //    }
//        //    else
//        //    {
//        //        // STEP 5B: Silent Login Failed (First time playing, or token expired)
//        //        Debug.Log("[BOOT] Auth Failed/Not Found. Routing to Login UI.");

//        //        // Broadcast boot completion so background systems spin up, but don't load the arena
//        //        _messageBroker.Publish(new ApplicationBootCompleteMessage(Success: false));


//        //        // We DO NOT load the 3D Main Menu. We load the 2D opaque Login Canvas to block the screen.
//        //        _uiRouter.LoadOpaqueCanvas("UI_Login");
//        //    }
//        //}
//        catch (Exception ex)
//        {
//            // STEP 6: Catastrophic Boot Failure
//            // If the CDN is down or the player has no internet, we catch it here.
//            Debug.LogError($"[BOOT] CATASTROPHIC FAILURE: {ex.Message}");

//            // Tell the UI Router to load a localized error popup with a "Retry" button
//            //_uiRouter.ShowCriticalErrorModal("Connection Failed", "Please check your internet and try again.", onRetry: () =>
//            //{
//            //    // Restart the boot sequence
//            //    _ = StartAsync();
//            //});
//        }
//    }

//    public Awaitable StartAsync(CancellationToken cancellation = default)
//    {
//        throw new NotImplementedException();
//    }

//    UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
//    {
//        throw new NotImplementedException();
//    }
//}
