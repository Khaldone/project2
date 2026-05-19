////Assets/_Project /Scenes /00_Hub_Bootstrap /Scripts/BootstrapLifetimeScope.cs
//using UnityEngine;
//using VContainer;
//using VContainer.Unity;
//using Billiards.CoreDomain.Progression;
//using Billiards.Infrastructure.Progression;
//using Billiards.Infrastructure.Authentication.NativeAuth;

//public class BootstrapLifetimeScope : LifetimeScope
//{
//    [SerializeField] private FusionNetworkManager _fusionNetworkManager;
//    [SerializeField] private PhotonChatWrapper _photonChatWrapper;
//    [SerializeField] private UnityAudioService _audioService; // Dragged in via Inspector
//    [SerializeField] private CanvasFadeTransition _globalTransition;
//    [SerializeField] private UnityBotProfileProvider _botProfileProvider;
//    [SerializeField] private UnityIAPWrapper _unityIapWrapper;
//    [SerializeField] private UnityAddressablesWrapper _addressablesWrapper;
//    [SerializeField] private CBSBattlePassWrapper _cbsWrapper;
//    [SerializeField] private FirebasePushWrapper _firebasePushWrapper;
//    //[SerializeField] private MixpanelAnalyticsWrapper _mixpanelWrapper;

//    [Header("Scene MonoBehaviours")]
//    [SerializeField] private UnityDeviceInfoService _deviceInfo;
//    [SerializeField] private AddressablesDeliveryWrapper _assetDelivery;
//    [SerializeField] private UnityAudioWrapper _audioWrapper;
//    protected override void Configure(IContainerBuilder builder)
//    {
//        // Register the Fusion Wrapper as BOTH interfaces
//        builder.RegisterComponent(_fusionNetworkManager)
//               .As<INetMatchmakingService>()
//               .As<INetworkSpawner>();

//        // 1. Bind the Unity SDK Wrapper
//        builder.RegisterComponent(_photonChatWrapper).As<IChatBackend>();


//        // 2. Bind the Pure C# Orchestrator
//        builder.Register<ChatOrchestrator>(Lifetime.Singleton).As<IChatOrchestrator>();


//        // 3. Expose it to the Global Locator
//        builder.RegisterBuildCallback(resolver =>
//        {
//            ServiceLocator.Register(resolver.Resolve<IChatOrchestrator>());
//        });

//        // VContainer registers this specific instance.
//        // It is now globally available to any Spoke scene that asks for it.
//        builder.RegisterComponent(_audioService).As<IAudioService>();

//        // ... register other systems like PlayFab, Photon, etc.

//        // Register the transition service globally
//        builder.RegisterComponent(_globalTransition).As<ITransitionService>();

//        // ... rest of your Hub registrations


//        // Register the pure C# Message Broker as a true Singleton
//        builder.Register<MessageBroker>(Lifetime.Singleton).As<IMessageBroker>();

//        // ... register your Audio, Analytics, and Network services

//        builder.RegisterComponent(_botProfileProvider).As<IBotProfileProvider>();
//        builder.Register<MatchmakingOrchestrator>(Lifetime.Singleton);
//        // ...

//        builder.RegisterComponent(_addressablesWrapper).As<IAssetDeliveryService>();

//        builder.RegisterEntryPoint<GlobalAnalyticsTracker>();

//        builder.RegisterComponent(_cbsWrapper).As<IBattlePassOrchestrator>();

//        builder.RegisterComponent(_firebasePushWrapper).As<IPushNotificationService>();
        
        
//        builder.RegisterEntryPoint<PushTokenSyncer>();

//#if UNITY_EDITOR
//        //builder.Register<EditorMockAuthWrapper>(Lifetime.Singleton).As<INativeAuthService>();
//#elif UNITY_IOS
//        builder.Register<AppleAuthWrapper>(Lifetime.Singleton).As<INativeAuthService>();
//#elif UNITY_ANDROID
//        builder.Register<GoogleAuthWrapper>(Lifetime.Singleton).As<INativeAuthService>();
//#else
//        // Fallback for WebGL, PC, etc.
//        builder.Register<EditorMockAuthWrapper>(Lifetime.Singleton).As<INativeAuthService>();
//#endif

//        builder.Register<UnityDeviceInfoService>(Lifetime.Singleton);
//        // Or this maybe:
//        // builder.Register<UnityDeviceInfoService>(Lifetime.Singleton).As<IDeviceInfoService>();

//        // 1. REGISTER SCENE MONOBEHAVIOURS (The hardware/engine wrappers)
//        builder.RegisterComponent(_deviceInfo).As<IDeviceInfoService>();
//        builder.RegisterComponent(_assetDelivery).As<IAssetDeliveryService>();
//        builder.RegisterComponent(_audioWrapper).As<IAudioService>();


//        // 2. REGISTER PURE C# SERVICES (No GameObjects needed! They live in RAM)
//        builder.Register<IMessageBroker, MessageBroker>(Lifetime.Singleton);
        
//        //builder.Register<ILocalSaveService, AESLocalSaveService>(Lifetime.Singleton);
//        builder.Register<IUIRouter, UIRouter>(Lifetime.Singleton);

//        // PlayFab Wrappers
//        //builder.Register<IPlayFabAuthService, PlayFabAuthWrapper>(Lifetime.Singleton);
//        builder.Register<ISettingsOrchestrator, SettingsOrchestrator>(Lifetime.Singleton);


//        // 3. REGISTER THE BOOTSTRAPPER
//        // VContainer automatically creates this pure C# class and runs StartAsync()
//        //builder.RegisterEntryPoint<AppBootstrapper>();


//        // Bind the interface to the wrapper
//        //builder.RegisterComponent(_mixpanelWrapper).As<IAnalyticsService>();

//        // Register our stealth orchestrator (explained in Step 4)
//        //builder.RegisterEntryPoint<AnalyticsOrchestrator>();
//    }
//}
