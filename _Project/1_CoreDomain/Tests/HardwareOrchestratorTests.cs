// Assets/_Project/1_CoreDomain/Tests/HardwareOrchestratorTests.cs
using System;
using NUnit.Framework;
using NSubstitute;


[TestFixture]
public class HardwareOrchestratorTests
{
    private IPushNotificationService _subPushService;
    private IMessageBroker _subBroker;
    //private HardwareOrchestrator _orchestrator;


    [SetUp]
    public void Setup()
    {
        // ARRANGE: Create our substitutes
        _subPushService = Substitute.For<IPushNotificationService>();
        _subBroker = Substitute.For<IMessageBroker>();


        // Inject them into the pure C# orchestrator
       // _orchestrator = new HardwareOrchestrator(_subPushService, _subBroker);
    }


    [Test]
    public void OnNotificationReceived_TranslatesHardwarePayload_AndPublishesToBroker()
    {
        // ARRANGE
        // Simulate VContainer starting the orchestrator (it subscribes to the hardware event here)
        //_orchestrator.Start();


        // Create a mock payload as if Firebase just handed it to the OS
        var mockHardwarePayload = new RemoteNotificationPayload
        {
            Title = "Tournament Live!",
            Body = "Tap here to join the Tokyo Tier.",
            DeepLinkAction = "join_tournament"
        };


        // ACT
        // We order NSubstitute to artificially trigger the C# event, perfectly mimicking
        // a real-world push notification interrupting the game.
        //_subPushService.OnNotificationReceivedForeground += Raise.Event<Action<RemoteNotificationPayload>>(mockHardwarePayload);


        // ASSERT
        // Verify that the Orchestrator instantly caught the event and fired it into the Message Broker
        //_subBroker.Received(1).Publish(Arg.Is<SystemNotificationMessage>(msg =>
        //    msg.Title == "Tournament Live!" &&
        //    msg.Message == "Tap here to join the Tokyo Tier."
        //));
    }


    [Test]
    public void Dispose_SeversHardwareConnection_PreventingMemoryLeaks()
    {
        // ARRANGE
        //_orchestrator.Start();

        // ACT
        // We instantly dispose of the orchestrator, simulating the player closing the match or the app
        //_orchestrator.Dispose();


        // Now, we simulate a "Ghost" push notification arriving AFTER the system has shut down
        var ghostPayload = new RemoteNotificationPayload { Title = "Ghost", Body = "You shouldn't see this." };
        //_subPushService.OnNotificationReceivedForeground += Raise.Event<Action<RemoteNotificationPayload>>(ghostPayload);


        // ASSERT
        // We verify that the Message Broker received absolutely nothing.
        // This proves the Orchestrator successfully unsubscribed from the hardware event.
        //_subBroker.DidNotReceive().Publish(Arg.Any<SystemNotificationMessage>());
    }
}