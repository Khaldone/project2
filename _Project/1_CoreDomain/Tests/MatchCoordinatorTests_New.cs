//// Assets/_Project/1_CoreDomain/Tests/MatchCoordinatorTests.cs
//using NUnit.Framework;
//using NSubstitute;
//using System.Collections.Generic;


//[TestFixture]
//public class MatchCoordinatorTests_New
//{
//    private IMessageBroker_New _subBroker;
//    private MatchCoordinator _coordinator;


//    private const string PLAYER_1 = "p1_guid";
//    private const string PLAYER_2 = "p2_guid";


//    [SetUp]
//    public void Setup()
//    {
//        // ARRANGE: Set up the pure C# environment
//        _subBroker = Substitute.For<IMessageBroker_New>();
//        _coordinator = new MatchCoordinator(_subBroker);


//        // Force the match state: It is Player 1's turn, and they are assigned Solids.
//        _coordinator.InitializeMatch(PLAYER_1, PLAYER_2);
//        _coordinator.AssignBallTypes(player1Type: BallType_New.Solid, player2Type: BallType_New.Stripe);
//        _coordinator.SetCurrentTurn(PLAYER_1);
//    }


//    [Test]
//    public void ProcessShot_ValidShot_SinksOwnBall_KeepsTurn()
//    {
//        // ARRANGE: The physics engine reports Player 1 hit a Solid first, and pocketed a Solid.
//        var shotResult = new ShotResult
//        {
//            FirstBallHit = BallType_New.Solid,
//            PocketedBalls = new List<BallType> { BallType.Solid },
//            DidCueBallHitRail = true
//        };


//        // ACT: The Coordinator processes the result
//        _coordinator.ProcessShotResult(shotResult);


//        // ASSERT
//        // 1. Verify NO fouls were committed
//        _subBroker.DidNotReceive().Publish(Arg.Any<FoulCommittedMessage>());


//        // 2. Verify it is STILL Player 1's turn, and they do NOT have ball-in-hand
//        _subBroker.Received(1).Publish(Arg.Is<TurnChangedMessage>(msg =>
//            msg.ActivePlayerId == PLAYER_1 &&
//            msg.IsBallInHand == false
//        ));
//    }


//    [Test]
//    public void ProcessShot_WrongBallHitFirst_TriggersFoul_AndTurnover()
//    {
//        // ARRANGE: Player 1 (Solids) hits a Stripe first, but doesn't pocket anything.
//        var shotResult = new ShotResult
//        {
//            FirstBallHit = BallType_New.Stripe,
//            PocketedBalls = new List<BallType_New>(),
//            DidCueBallHitRail = true
//        };


//        // ACT
//        _coordinator.ProcessShotResult(shotResult);


//        // ASSERT
//        // 1. Verify a Foul was broadcasted specifically for hitting the wrong ball
//        _subBroker.Received(1).Publish(Arg.Is<FoulCommittedMessage>(msg =>
//            msg.PlayerId == PLAYER_1 &&
//            msg.Reason == "Hit opponent's ball first."
//        ));


//        // 2. Verify the turn transitioned to Player 2, WITH Ball-in-Hand!
//        _subBroker.Received(1).Publish(Arg.Is<TurnChangedMessage>(msg =>
//            msg.ActivePlayerId == PLAYER_2 &&
//            msg.IsBallInHand == true
//        ));
//    }


//    [Test]
//    public void ProcessShot_Scratch_TriggersFoul_AndTurnover()
//    {
//        // ARRANGE: Player 1 legally hits a Solid first, but accidentally sinks the Cue ball.
//        var shotResult = new ShotResult
//        {
//            FirstBallHit = BallType_New.Solid,
//            PocketedBalls = new List<BallType_New> { BallType_New.Cue },
//            DidCueBallHitRail = true
//        };


//        // ACT
//        _coordinator.ProcessShotResult(shotResult);


//        // ASSERT
//        _subBroker.Received(1).Publish(Arg.Is<FoulCommittedMessage>(msg =>
//            msg.Reason == "Cue ball pocketed."
//        ));


//        _subBroker.Received(1).Publish(Arg.Is<TurnChangedMessage>(msg =>
//            msg.ActivePlayerId == PLAYER_2 &&
//            msg.IsBallInHand == true
//        ));
//    }
//}
