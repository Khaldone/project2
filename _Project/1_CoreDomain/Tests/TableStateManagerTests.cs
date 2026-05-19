

using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

public class TableStateManagerTests
{
    [Test]
    public void AreAllBallsResting_OneBallStillMoving_ReturnsFalse()
    {
        // ARRANGE: Mock the tracker
        var mockTracker = Substitute.For<IBallStateTracker>();

        // Simulate 3 balls: two stopped, one moving at 0.5f speed
        mockTracker.GetActiveBallSpeeds().Returns(new List<float> { 0.0f, 0.0f, 0.5f });

        var stateManager = new TableStateManager(mockTracker);


        // ACT
        bool result = stateManager.AreAllBallsResting();


        // ASSERT
        Assert.IsFalse(result);
    }


    [Test]
    public void AreAllBallsResting_AllBallsBelowThreshold_ReturnsTrue()
    {
        // ARRANGE
        var mockTracker = Substitute.For<IBallStateTracker>();

        // Simulate 3 balls: all moving slightly, but below the 0.01f threshold
        mockTracker.GetActiveBallSpeeds().Returns(new List<float> { 0.001f, 0.005f, 0.009f });

        var stateManager = new TableStateManager(mockTracker);


        // ACT
        bool result = stateManager.AreAllBallsResting();


        // ASSERT
        Assert.IsTrue(result);
    }
}
