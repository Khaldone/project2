//// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/AppBootstrapper.cs
//using System;
//using System.Threading.Tasks;
//using VContainer.Unity;
//using UnityEngine;
//using System.Threading;
//using Cysharp.Threading.Tasks;


//public class AppBootstrapper : IAsyncStartable
//{
//    private readonly IUIRouter _router;
//    //private readonly IPlayFabAuthService _auth;
//    private readonly IFTUETracker _ftueTracker;
//    private readonly ISettingsOrchestrator _settings;


//    //public AppBootstrapper(IUIRouter router, IPlayFabAuthService auth, IFTUETracker ftueTracker, ISettingsOrchestrator settings)
//    //{
//    //    _router = router;
//    //    _auth = auth;
//    //    _ftueTracker = ftueTracker;
//    //    _settings = settings;
//    //}
//    public AppBootstrapper(IUIRouter router, IFTUETracker ftueTracker, ISettingsOrchestrator settings)
//    {
//        _router = router;
//        _ftueTracker = ftueTracker;
//        _settings = settings;
//    }

//    //public async UniTask StartAsync()
//    public async Task StartAsync()
//    {
//        Debug.Log("AAA Pipeline: Boot sequence initiated.");


//        // SEQUENCE 1: Initialize Core Services (No UI instantiated yet)
//        await _settings.InitializeAsync();


//        // SEQUENCE 2: Authentication
//        //bool isLoggedIn = await _auth.TrySilentLoginAsync();

//        //if (!isLoggedIn)
//        //{
//        //    // INSTANTIATION: Spawns the UI_Login Canvas prefab over the black screen
//        //    await _router.OpenMenuAsync("UI_Login");

//        //    // Halt the boot sequence here until the LoginPresenter fires an event saying they succeeded
//        //    return;
//        //}


//        await ContinueBootSequenceAsync();
//    }


//    // Called either immediately (if silent login works), or after the UI_Login succeeds
//    public async Task ContinueBootSequenceAsync()
//    {
//        // Clean up the login screen if it exists
//        //_router.CloseMenu("UI_Login");


//        // SEQUENCE 3: Check First Time User Experience (FTUE)
//        await _ftueTracker.FetchFTUEStateAsync();


//        // INSTANTIATION: Spawn the global notification canvas so it's always ready
//        await _router.OpenMenuAsync("UI_GlobalNotifications");


//        if (!_ftueTracker.HasCompletedTutorial)
//        {
//            Debug.Log("AAA Pipeline: Routing to FTUE Tutorial.");

//            // INSTANTIATION: Loads the 3D Arena Scene, then spawns the Tutorial Overlay prefab
//            //await _router.LoadBaseSpokeAsync("02_Spoke_GameArena");
//            await _router.OpenMenuAsync("UI_TutorialOverlay");
//        }
//        else
//        {
//            Debug.Log("AAA Pipeline: Routing to Main Menu.");

//            // INSTANTIATION: Loads the 3D Main Menu Scene, spawns the MainMenu UI Canvas,
//            // and asynchronously streams in the 3D Pool Table model.
//            //await _router.LoadBaseSpokeAsync("01_Spoke_MainMenu");
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
