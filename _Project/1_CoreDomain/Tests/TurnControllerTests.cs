using NUnit.Framework;
using NSubstitute;
using UnityEngine;
public class TurnControllerTests
{
    [Test]
    public void Tick_WithAimDelta_UpdatesAimDirection()
    {
        // ARRANGE
        var mockInput = Substitute.For<IPlayerInput>();
        var mockTable = Substitute.For<ITableProperties>();
        var mockCueBall = Substitute.For<IPhysicsBody>(); // Added.
        var calculator = new StrikeCalculator(mockTable);
        // Simulate dragging the finger to the right
        mockInput.GetAimDelta().Returns(1.0f);


        var turnController = new TurnController(mockInput, calculator, mockCueBall);
        Vector3 initialAim = turnController.CurrentAimDirection;


        // ACT
        turnController.Tick(1.0f, 2.0f); // Simulate 1 second passing


        // ASSERT
        Assert.AreNotEqual(initialAim, turnController.CurrentAimDirection);
    }
}
