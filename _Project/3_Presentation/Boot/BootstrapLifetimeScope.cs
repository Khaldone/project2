//// Assets/Scripts/Presentation/Boot/BootstrapLifetimeScope.cs
//using VContainer;
//using VContainer.Unity;
//using UnityEngine;


//public class BootstrapLifetimeScope : LifetimeScope
//{
//    [SerializeField] private PlayFabAuthService _playfabAuth;
//    //[SerializeField] private FusionNetworkService _fusionNetwork; // Your INetworkService wrapper
//    //[SerializeField] private UnitySceneLoader _sceneLoader;       // Your ISceneLoader wrapper
//    [Header("Unity Implementation Wrappers")]
//    [SerializeField] private UnityIAPWrapper _iapWrapper;
//    [SerializeField] private UnityAdsServiceWrapper _adsWrapper;
//    [SerializeField] private PlayFabSaveBackend _saveBackend;

//    protected override void Configure(IContainerBuilder builder)
//    {
//        // 1. Bind the Unity dependencies
//        builder.RegisterComponent<IAuthenticationService>(_playfabAuth);
//        //builder.RegisterComponent<INetworkService>(_fusionNetwork);
//        //builder.RegisterComponent<ISceneLoader>(_sceneLoader);


//        // 2. Bind the Bootstrapper.
//        builder.Register<GameBootstrapper>(Lifetime.Singleton);


//        // 3. The Entry Point: Tell VContainer to run this specific class the moment the app opens
//        //builder.RegisterEntryPoint<BootEntryPoint>();

//        // 1. Register the "Humble" Unity components
//        builder.RegisterComponent<IStorePlatform>(_iapWrapper);
//        builder.RegisterComponent<IAdsService>(_adsWrapper);
//        builder.RegisterComponent<ICloudSaveBackend>(_saveBackend);


//        // 2. Register Feature Modules (Pure C#)
//        new MonetizationModule().Register(builder);
//        //new ProgressionModule().Register(builder); // Handles SaveSystem/XP
//        //new AudioModule().Register(builder);


//        // 3. The AAA "Publication" Step
//        // This callback populates the ServiceLocator once VContainer is ready.
//        builder.RegisterBuildCallback(resolver =>
//        {
//            ServiceLocator.Clear(); // Safety for hot-reloads


//            // Publish the contracts to the global locator
//            ServiceLocator.Register(resolver.Resolve<IStoreOrchestrator>());
//            ServiceLocator.Register(resolver.Resolve<IAdsService>());
//            ServiceLocator.Register(resolver.Resolve<IPlayerDataService>());
//            //ServiceLocator.Register(resolver.Resolve<IAudioService>());
//            //ServiceLocator.Register(resolver.Resolve<IAnalyticsService>());

//            Debug.Log("AAA Service Registry: All services published to Locator.");
//        });
//    }
//}


//// A simple bridge between VContainer's lifecycle and our pure C# Bootstrapper
////public class BootEntryPoint : IAsyncStartable
////{
////    private readonly GameBootstrapper _bootstrapper;


////    public BootEntryPoint(GameBootstrapper bootstrapper)
////    {
////        _bootstrapper = bootstrapper;
////    }


////    public async Task StartAsync(System.Threading.CancellationToken cancellation)
////    {
////        await _bootstrapper.StartGameAsync();
////    }
////}
