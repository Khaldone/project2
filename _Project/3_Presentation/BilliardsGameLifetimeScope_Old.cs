//// Assets/Scripts/Presentation/BilliardsGameLifetimeScope.cs
//using UnityEngine;
//using VContainer;
//using VContainer.Unity;


//public class BilliardsGameLifetimeScope : LifetimeScope
//{
//    [Header("Network Dependencies")]
//    [Tooltip("Drag the GameObject holding the FusionMatchBroadcaster here.")]
//    [SerializeField] private FusionMatchBroadcaster _fusionMatchBroadcaster;


//    [Header("Input Dependencies")]
//    [SerializeField] private MobileTouchInput _mobileTouchInput;

//    [SerializeField] PlayFabMatchSubmitter playFabMatchSubmitter;
//    [SerializeField] UnityAdsServiceWrapper unityAdsServiceWrapper;
//    [SerializeField] PlayFabSaveBackend playFabSaveBackend;
//    [SerializeField] UnityIAPWrapper unityIAPWrapper;
//    [SerializeField] SentryTelemetryWrapper sentryTelemetryWrapper;
//    protected override void Configure(IContainerBuilder builder)
//    {
//        // --------------------------------------------------------
//        // 1. Bind the Presentation Layer (The Unity/Fusion Objects)
//        // --------------------------------------------------------

//        // We tell VContainer: "Whenever a pure C# class asks for an IMatchBroadcaster,
//        // give it this exact Fusion component from the scene."
//        builder.RegisterComponent<IMatchBroadcaster>(_fusionMatchBroadcaster);

//        // Similarly, bind the input we built earlier
//        builder.RegisterComponent<IPlayerInput>(_mobileTouchInput);




//        // --------------------------------------------------------
//        // 2. Bind the Core Domain Layer (The Pure C# Brains)
//        // --------------------------------------------------------

//        // Register the ledger that tracks the turn
//        builder.Register<TurnContext>(Lifetime.Scoped);

//        // Register the Rules Engine
//        builder.Register<RulesEngine>(Lifetime.Singleton);


//        // Register the Coordinator.
//        // MAGIC HAPPENS HERE: VContainer looks at the MatchCoordinator's constructor,
//        // sees it needs an IMatchBroadcaster, a RulesEngine, and a TurnContext,
//        // and automatically injects all three of the bindings we defined above.
//        builder.Register<MatchCoordinator>(Lifetime.Singleton);


//        // Bind the PlayFab integration
//        builder.RegisterComponent<IMatchResultSubmitter>(playFabMatchSubmitter);


//        // Bind the Core Brain
//        builder.Register<PostMatchProcessor>(Lifetime.Scoped);

//        // Bind the Unity component to the interface
//        builder.RegisterComponent<IAdsService>(unityAdsServiceWrapper);


//        // Run a callback after VContainer builds to populate our static Locator
//        builder.RegisterBuildCallback(resolver =>
//        {
//            ServiceLocator.Register(resolver.Resolve<IAdsService>());
//            // Register Analytics, Audio, etc.
//        });

//        // 1. Bind the Humble PlayFab wrapper
//        builder.RegisterComponent<ICloudSaveBackend>(playFabSaveBackend);


//        // 2. Bind the Core Data Service
//        builder.Register<PlayerDataService>(Lifetime.Singleton).AsImplementedInterfaces();


//        // 3. Expose it to the Service Locator for the UI to use
//        builder.RegisterBuildCallback(resolver =>
//        {
//            var dataService = resolver.Resolve<IPlayerDataService>();
//            ServiceLocator.Register<IPlayerDataService>(dataService);
//        });

//        // 1. Bind the Humble Unity IAP wrapper
//        //builder.RegisterComponent<IStorePlatform>(unityIAPWrapper);


//        // 2. Bind the Core Orchestrator
//        builder.Register<StoreOrchestrator>(Lifetime.Singleton).AsImplementedInterfaces();


//        // 3. Expose it to the global Service Locator for the UI to access
//        builder.RegisterBuildCallback(resolver =>
//        {
//            var storeOrchestrator = resolver.Resolve<IStoreOrchestrator>();
//            ServiceLocator.Register<IStoreOrchestrator>(storeOrchestrator);
//        });

//        builder.RegisterComponent<ITelemetryService>(sentryTelemetryWrapper);


//        builder.RegisterBuildCallback(resolver =>
//        {
//            // Publish to the global locator for UI scripts to use easily
//            ServiceLocator.Register(resolver.Resolve<ITelemetryService>());
//        });


//    }
//}
