using NUnit.Framework;
using NSubstitute;
using UnityEngine;


public class TurnControllerStrikeTests
{
    [Test]
    public void ExecuteStrike_WhenReleased_AppliesCorrectVelocityToBall()
    {
        // ARRANGE: Set up the mocks
        var mockInput = Substitute.For<IPlayerInput>();
        var mockTable = Substitute.For<ITableProperties>();
        var mockBall = Substitute.For<IPhysicsBody>();


        // Configure the simulation state
        mockTable.GetFrictionCoefficient().Returns(0.1f);
        mockBall.GetMass().Returns(2.0f);

        // Simulate pulling the cue back to 50% power and releasing it
        mockInput.GetStrikePower().Returns(0.5f);
        mockInput.IsStrikeReleased().Returns(true);


        var calculator = new StrikeCalculator(mockTable);
        var turnController = new TurnController(mockInput, calculator, mockBall);


        // ACT: Run the game loop for one frame
        turnController.Tick(0.016f, 0.5f);


        // ASSERT
        // Mathematical expectation:
        // Force = 0.5 power * 20 max = 10f.
        // Velocity = (10 force / 2 mass) - 0.1 friction = 4.9f.
        // Direction is forward (0, 0, 1) by default.
        Vector3 expectedVelocity = new Vector3(0, 0, 4.9f);


        // NSubstitute verifies that the TurnController actually issued the command
        // to the ball with the exact vector we expect.
        mockBall.Received(1).ApplyVelocity(expectedVelocity);
    }
}
