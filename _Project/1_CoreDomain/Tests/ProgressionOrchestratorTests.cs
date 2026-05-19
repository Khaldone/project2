// Assets/_Project/1_CoreDomain/Tests/ProgressionOrchestratorTests.cs
using NUnit.Framework;
using NSubstitute;


[TestFixture]
public class ProgressionOrchestratorTests
{
    private IProfileDataService _subProfileService;
    private IMessageBroker_New _subBroker;
    //private ProgressionOrchestrator _orchestrator;


    [SetUp]
    public void Setup()
    {
        // ARRANGE: Set up our pure C# substitutes
        _subProfileService = Substitute.For<IProfileDataService>();
        _subBroker = Substitute.For<IMessageBroker_New>();


        // Inject them into the Orchestrator
        //_orchestrator = new ProgressionOrchestrator(_subProfileService, _subBroker);

        // Simulate the AppBootstrapper calling Start()
        //_orchestrator.Start();
    }


    [Test]
    public void OnMatchFinished_WhenPlayerWins_CalculatesCorrectGains_AndSyncs()
    {
        // ARRANGE: The player currently has 1000 Trophies and is Level 5
        var currentStats = new PlayerProfileStats { Level = 5, CurrentXP = 100, Trophies = 1000 };
        _subProfileService.GetCurrentStats().Returns(currentStats);


        // Simulate the MatchCoordinator broadcasting a win against a slightly better player
        //var matchMessage = new MatchFinishedMessage(
        //    DidLocalPlayerWin: true,
        //    WasBotMatch: false,
        //    OpponentTrophies: 1050,
        //    MatchDurationSeconds: 180,
        //    ArenaTierName: "Tokyo"
        //);


        // ACT: Fire the event
        //_subBroker.OnMessagePublished += Raise.Event<System.Action<MatchFinishedMessage>>(matchMessage);


        // ASSERT
        // Let's assume our proprietary math awards 30 base trophies + 5 underdog bonus = 35.
        // And a flat 50 XP per win.
        PlayerProfileStats expectedNewStats = new PlayerProfileStats { Level = 5, CurrentXP = 150, Trophies = 1035 };


        // 1. Verify the exact new stats were sent to the cloud wrapper
        _subProfileService.Received(1).SyncNewStatsToServer(Arg.Is<PlayerProfileStats>(s =>
            s.Trophies == 1035 &&
            s.CurrentXP == 150
        ));


        // 2. Verify the UI was told to play the specific delta animations
        _subBroker.Received(1).Publish(Arg.Is<ProgressionUpdatedMessage>(msg =>
            msg.TrophyDelta == 35 &&
            msg.XpDelta == 50 &&
            msg.DidLevelUp == false
        ));
    }


    [Test]
    public void OnMatchFinished_WhenPlayerLosesAtZeroTrophies_ClampsToZero()
    {
        // ARRANGE: A brand new player with exactly 5 Trophies
        var currentStats = new PlayerProfileStats { Level = 1, CurrentXP = 10, Trophies = 5 };
        _subProfileService.GetCurrentStats().Returns(currentStats);


        //var matchMessage = new MatchFinishedMessage(
        //    DidLocalPlayerWin: false,
        //    WasBotMatch: false,
        //    OpponentTrophies: 100,
        //    MatchDurationSeconds: 180,
        //    ArenaTierName: "Tokyo"
        //);


        // ACT
        //_subBroker.OnMessagePublished += Raise.Event<System.Action<MatchFinishedMessage>>(matchMessage);


        // ASSERT
        // A standard loss might be -15 Trophies. We must assert the math clamps to 0
        // and doesn't give the player -10 Trophies!
        _subProfileService.Received(1).SyncNewStatsToServer(Arg.Is<PlayerProfileStats>(s =>
            s.Trophies == 0
        ));


        // Verify the UI shows a loss of exactly 5 (since they only had 5 to lose)
        _subBroker.Received(1).Publish(Arg.Is<ProgressionUpdatedMessage>(msg =>
            msg.TrophyDelta == -5
        ));
    }


    [Test]
    public void OnMatchFinished_WhenXpExceedsThreshold_IncrementsLevel()
    {
        // ARRANGE: Player is very close to Level Up (Assuming 500 XP is the threshold for Lv 6)
        var currentStats = new PlayerProfileStats { Level = 5, CurrentXP = 480, Trophies = 1000 };
        _subProfileService.GetCurrentStats().Returns(currentStats);


        //var matchMessage = new MatchFinishedMessage(
        //    DidLocalPlayerWin: true, // Grants +50 XP
        //    WasBotMatch: false,
        //    OpponentTrophies: 1000,
        //    MatchDurationSeconds: 180,
        //    ArenaTierName: "Tokyo"
        //);


        // ACT
        //_subBroker.OnMessagePublished += Raise.Event<System.Action<MatchFinishedMessage>>(matchMessage);


        // ASSERT
        // 480 + 50 = 530. It should subtract 500 for the level, leaving 30 XP rollover.
        _subProfileService.Received(1).SyncNewStatsToServer(Arg.Is<PlayerProfileStats>(s =>
            s.Level == 6 &&
            s.CurrentXP == 30
        ));


        // Verify the UI is specifically told to trigger the massive "LEVEL UP!" visual sequence
        _subBroker.Received(1).Publish(Arg.Is<ProgressionUpdatedMessage>(msg =>
            msg.DidLevelUp == true
        ));
    }
}