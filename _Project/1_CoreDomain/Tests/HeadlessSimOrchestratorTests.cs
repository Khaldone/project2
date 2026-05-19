using NUnit.Framework;
using NSubstitute;
using System;
using System.Collections.Generic;
using UnityEngine;


public class HeadlessSimOrchestratorTests
{
    [Test]
    public void ValidatePhysicsState_WhenVelocityIsNaN_ThrowsException()
    {
        // Prove the detector works independently
        var detector = new PhysicsAnomalyDetector();
        var positions = new List<Vector3> { Vector3.zero };
        var badVelocities = new List<Vector3> { new Vector3(float.NaN, 0, 0) };


        Assert.Throws<Exception>(() => detector.ValidatePhysicsState(positions, badVelocities));
    }


    [Test]
    public void RunSimulatedMatch_WhenAnomalyDetected_HaltsSimulation()
    {
        // ARRANGE
        var mockTurn = Substitute.For<TurnController>(/* mock dependencies */);
        var mockState = Substitute.For<TableStateManager>(/* mock dependencies */);
        var mockDetector = Substitute.For<IAnomalyDetector>();


        // Force the state manager to enter the physics loop
        mockState.AreAllBallsResting().Returns(false, true);


        // Force the mock detector to simulate finding a bug on the first frame
        mockDetector.When(x => x.ValidatePhysicsState(Arg.Any<IEnumerable<Vector3>>(), Arg.Any<IEnumerable<Vector3>>()))
                    .Do(x => throw new Exception("SIMULATION FAILED: Velocity became NaN/Infinity."));


        var orchestrator = new HeadlessSimOrchestrator(mockTurn, mockState, mockDetector);


        // ACT & ASSERT
        // Verify that the orchestrator surfaces the exact bug so Jenkins can read it
        var ex = Assert.Throws<Exception>(() => orchestrator.RunSimulatedMatch());
        Assert.That(ex.Message, Does.Contain("NaN/Infinity"));
    }
}
