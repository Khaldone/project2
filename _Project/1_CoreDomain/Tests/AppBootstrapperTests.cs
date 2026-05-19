//// Assets/_Project/1_CoreDomain/Tests/AppBootstrapperTests.cs
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using NUnit.Framework;
//using NSubstitute;
//using Cysharp.Threading.Tasks;


//[TestFixture]
//public class AppBootstrapperTests
//{
//    private IAssetDeliveryService _subAssetDelivery;
//    private IPushNotificationService _subPushService;
//    private ILocalSaveService _subSaveService;
//    private ISettingsOrchestrator _subSettings;
//    private IPlayFabAuthService _subAuth;
//    private IUIRouter _subRouter;
//    private IMessageBroker _subBroker;

//    private AppBootstrapper _bootstrapper;


//    [SetUp]
//    public void Setup()
//    {
//        // ARRANGE: Instantiate all 7 substitutes
//        _subAssetDelivery = Substitute.For<IAssetDeliveryService>();
//        _subPushService = Substitute.For<IPushNotificationService>();
//        _subSaveService = Substitute.For<ILocalSaveService>();
//        _subSettings = Substitute.For<ISettingsOrchestrator>();
//        _subAuth = Substitute.For<IPlayFabAuthService>();
//        _subRouter = Substitute.For<IUIRouter>();
//        _subBroker = Substitute.For<IMessageBroker>();


//        // Inject them into our bootstrapper
//        _bootstrapper = new AppBootstrapper(
//            _subAssetDelivery, _subPushService, _subSaveService,
//            _subSettings, _subAuth, _subRouter, _subBroker
//        );
//    }


//    // Note: We use standard Task for the NUnit test signature,
//    // which natively supports awaiting Cysharp's UniTask inside.
//    [Test]
//    public async Task StartAsync_WhenSilentLoginSucceeds_RoutesToMainMenu()
//    {
//        // ARRANGE: Simulate a perfect network environment
//        _subAssetDelivery.InitializeAsync().Returns(UniTask.CompletedTask);
//        _subPushService.InitializeAsync().Returns(UniTask.FromResult(true));
//        _subSettings.LoadLocalSettingsAsync().Returns(UniTask.CompletedTask);

//        // Simulate PlayFab remembering the user
//        _subAuth.TrySilentLoginAsync().Returns(UniTask.FromResult(true));

//        // Simulate Firebase providing a valid token
//        _subPushService.CurrentDeviceToken.Returns("mock_fcm_token_123");


//        // ACT
//        await _bootstrapper.StartAsync(CancellationToken.None);


//        // ASSERT
//        // 1. Verify the push token was handed to PlayFab
//        await _subAuth.Received(1).RegisterPushNotificationTokenAsync("mock_fcm_token_123");


//        // 2. Verify the game broadcasted a successful boot to the Audio/Analytics systems
//        _subBroker.Received(1).Publish(Arg.Is<ApplicationBootCompleteMessage>(msg => msg.Success == true));


//        // 3. Verify the UI Router bypassed the login screen and loaded the 3D Menu
//        _subRouter.Received(1).LoadScene("01_Spoke_MainMenu");
//    }


//    [Test]
//    public async Task StartAsync_WhenSilentLoginFails_RoutesToLoginUI()
//    {
//        // ARRANGE
//        _subAssetDelivery.InitializeAsync().Returns(UniTask.CompletedTask);
//        _subPushService.InitializeAsync().Returns(UniTask.FromResult(true));
//        _subSettings.LoadLocalSettingsAsync().Returns(UniTask.CompletedTask);

//        // Simulate a new player (or expired token)
//        _subAuth.TrySilentLoginAsync().Returns(UniTask.FromResult(false));


//        // ACT
//        await _bootstrapper.StartAsync(CancellationToken.None);


//        // ASSERT
//        // Verify the game broadcasted an unsuccessful boot
//        _subBroker.Received(1).Publish(Arg.Is<ApplicationBootCompleteMessage>(msg => msg.Success == false));


//        // Verify it securely loaded the 2D opaque login canvas
//        _subRouter.Received(1).LoadOpaqueCanvas("UI_Login");

//        // Verify it did NOT attempt to load the 3D menu
//        _subRouter.DidNotReceive().LoadScene(Arg.Any<string>());
//    }


//    [Test]
//    public async Task StartAsync_WhenNetworkFails_ShowsCriticalErrorModal()
//    {
//        // ARRANGE: Simulate the user launching the game in Airplane Mode.
//        // Addressables will instantly throw an exception when it tries to check the CDN catalog.
//        _subAssetDelivery.InitializeAsync().Returns(UniTask.FromException(new Exception("No Internet Connection")));


//        // ACT
//        await _bootstrapper.StartAsync(CancellationToken.None);


//        // ASSERT
//        // 1. Ensure the boot sequence safely caught the crash and didn't execute downstream logic
//        _subAuth.DidNotReceive().TrySilentLoginAsync();
//        _subBroker.DidNotReceive().Publish(Arg.Any<ApplicationBootCompleteMessage>());


//        // 2. Verify the UI Router was commanded to show the localized error popup
//        _subRouter.Received(1).ShowCriticalErrorModal(
//            Arg.Is<string>("Connection Failed"),
//            Arg.Any<string>(),
//            Arg.Any<Action>() // Verifies we passed in the Retry callback
//        );
//    }
//}
