using NUnit.Framework;
using UnityEngine;
public class BallPhysicsSimulatorTests
{
    [Test]
    public void CalculateNextFrame_WhenMoving_UpdatesPositionAndAppliesFriction()
    {
        var simulator = new BallPhysicsSimulator();
        var startState = new BallState { Position = Vector3.zero, Velocity = new Vector3(0, 0, 10f), IsResting = false };

        // Simulate 1 second (1.0f) passing
        var nextState = simulator.CalculateNextFrame(startState, 1.0f);


        // Position moved forward by 10 units
        Assert.AreEqual(new Vector3(0, 0, 10f), nextState.Position);

        // Velocity decreased due to the 0.98f friction multiplier
        Assert.AreEqual(new Vector3(0, 0, 9.8f), nextState.Velocity);
    }
}