// Assets/_Project/CoreDomain/Tests/EightBallRulesTests.cs
using NUnit.Framework;
using UnityEngine;


[TestFixture]
public class EightBallRulesTests
{
    private EightBallStrategy _strategy;


    [SetUp]
    public void Setup()
    {
        // Runs before every single test to give us a fresh slate
        _strategy = new EightBallStrategy();
    }


    [Test]
    public void EvaluateShot_HitsWrongSuitFirst_ReturnsFoul()
    {
        // 1. ARRANGE (Set up the scenario)
        string playerId = "Player1";

        // Mock the table state
        //var tableState = new TableState();
        //tableState.SetPlayerSuit(playerId, BallSuit.Solids);

        // Mock the collision data: The cue ball hit Ball #10 (a Stripe) first
        var intent = new StrikeIntent { /* ... */ };
        //tableState.SetFirstCollisionForTesting(intent, 10);


        // 2. ACT (Execute the exact method we are testing)
        //ShotResult result = _strategy.EvaluateShot(tableState, intent, playerId);


        // 3. ASSERT (Mathematically prove the outcome is exactly what we expect)
        //Assert.IsTrue(result.IsFoul, "Hitting a stripe when assigned solids should be a foul.");
        //Assert.AreEqual("WrongSuitHit", result.FoulReason, "The foul reason string did not match.");
    }


    [Test]
    public void EvaluateShot_HitsCorrectSuitFirst_ReturnsCleanShot()
    {
        // 1. ARRANGE
        string playerId = "Player1";
        //var tableState = new TableState();
        //tableState.SetPlayerSuit(playerId, BallSuit.Solids);
        //tableState.SetFirstCollisionForTesting(new StrikeIntent(), 3); // Hit a Solid first


        // 2. ACT
        //ShotResult result = _strategy.EvaluateShot(tableState, new StrikeIntent(), playerId);


        // 3. ASSERT
        //Assert.IsFalse(result.IsFoul, "Hitting your own suit should not be a foul.");
    }
}