// Assets/_Project/1_CoreDomain/Tests/TournamentOrchestratorTests.cs
using NUnit.Framework;
using NSubstitute;
using System.Collections.Generic;


[TestFixture]
public class TournamentOrchestratorTests
{
    private ITournamentCloudService _subCloudService;
    private IMessageBroker_New _subBroker;
    private IRngProvider _subRng;
    //private TournamentOrchestrator _orchestrator;


    [SetUp]
    public void Setup()
    {
        // ARRANGE: Set up our pure C# substitutes
        _subCloudService = Substitute.For<ITournamentCloudService>();
        _subBroker = Substitute.For<IMessageBroker_New>();
        _subRng = Substitute.For<IRngProvider>();


        // Inject them into the Orchestrator
        //_orchestrator = new TournamentOrchestrator(_subCloudService, _subBroker, _subRng);
    }


    [Test]
    public void ReportMatchResult_WhenPlayerWins_AdvancesToNextRound()
    {
        // ARRANGE: Player is in the QuarterFinals. There are 7 bots in the tournament.
        var startingBracket = new TournamentData
        {
            State = TournamentState_New.Active,
            CurrentRound = BracketRound.QuarterFinal,
            ActiveBotIds = new List<string> { "Bot1", "Bot2", "Bot3", "Bot4", "Bot5", "Bot6", "Bot7" }
        };
        _subCloudService.GetLocalBracketState().Returns(startingBracket);


        // ACT: The MatchCoordinator reports a win
        //_orchestrator.ReportMatchResult(didPlayerWin: true);


        // ASSERT:
        // 1. The player must be moved to the SemiFinals
        // 2. The bot list must be halved (down to 3 surviving bots for the semis)
        _subCloudService.Received(1).SaveBracketState(Arg.Is<TournamentData>(data =>
            data.CurrentRound == BracketRound.SemiFinal &&
            data.ActiveBotIds.Count == 3 &&
            data.State == TournamentState_New.Active
        ));


        // 3. The UI must be told to draw the new bracket lines
        _subBroker.Received(1).Publish(Arg.Is<BracketUpdatedMessage>(msg =>
            msg.NewRound == BracketRound.SemiFinal &&
            msg.BotsRemaining == 3
        ));
    }


    [Test]
    public void ReportMatchResult_WhenPlayerLoses_LocksStateToEliminated()
    {
        // ARRANGE: Player is in the SemiFinals
        var startingBracket = new TournamentData
        {
            State = TournamentState_New.Active,
            CurrentRound = BracketRound.SemiFinal,
            ActiveBotIds = new List<string> { "Bot1", "Bot2", "Bot3" }
        };
        _subCloudService.GetLocalBracketState().Returns(startingBracket);


        // ACT: The player loses
        //_orchestrator.ReportMatchResult(didPlayerWin: false);


        // ASSERT: The state MUST be marked as eliminated to prevent retries
        _subCloudService.Received(1).SaveBracketState(Arg.Is<TournamentData>(data =>
            data.State == TournamentState_New.Eliminated &&
            data.CurrentRound == BracketRound.SemiFinal // They stay where they died
        ));
    }


    [Test]
    public void SimulateBotBracket_UsesRngToDetermineSurvivors()
    {
        // ARRANGE: We test the isolated bot-vs-bot simulation logic.
        // We have 4 bots playing 2 matches.
        var initialBots = new List<string> { "BotA", "BotB", "BotC", "BotD" };

        // We FORCE the RNG to return specific values so our test is 100% deterministic!
        // Match 1 (BotA vs BotB): Return 0.2f (BotA wins)
        // Match 2 (BotC vs BotD): Return 0.8f (BotD wins)
        _subRng.GetRandomFloat01().Returns(0.2f, 0.8f);


        // ACT
        //List<string> survivingBots = _orchestrator.SimulateBotMatches(initialBots);


        // ASSERT: Exactly 2 bots must survive, and they must be the specific ones the RNG selected
        //Assert.AreEqual(2, survivingBots.Count);
        //Assert.Contains("BotA", survivingBots);
        //Assert.Contains("BotD", survivingBots);
    }
}