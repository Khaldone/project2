using NUnit.Framework;
using NSubstitute;


public class MatchCoordinatorTests
{
    [Test]
    public void ResolveTurnEnd_WhenFoulOccurs_BroadcastsScratchAndPassesTurn()
    {
        // ARRANGE
        var mockBroadcaster = Substitute.For<IMatchBroadcaster>();
        var rulesEngine = new RulesEngine();
        var context = new TurnContext();

        // Simulate a scratch by manually putting the cue ball in the ledger
        context.RegisterPocketedBall(enBallType.Cue);


        //var coordinator = new MatchCoordinator(mockBroadcaster, rulesEngine, context);
        //int initialPlayer = coordinator.CurrentPlayerId; // Starts as Player 1


        // ACT
        //coordinator.ResolveTurnEnd();


        // ASSERT
        // 1. Did we tell the network about the foul?
        //mockBroadcaster.Received(1).BroadcastMatchEvent(MatchEventType.Scratch, initialPlayer);

        // 2. Did we tell the network to change the active player?
        mockBroadcaster.Received(1).BroadcastActivePlayer(2);

        // 3. Did the internal state update correctly?
        //Assert.AreEqual(2, coordinator.CurrentPlayerId);
    }
}
