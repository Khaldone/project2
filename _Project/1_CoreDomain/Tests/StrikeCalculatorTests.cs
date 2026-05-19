using NUnit.Framework;
using NSubstitute;


public class StrikeCalculatorTests
{
    [Test]
    public void CalculateFinalVelocity_WithValidForce_ReturnsCorrectVelocity()
    {
        // ARRANGE: Set up the mock and the isolated environment
        var mockTable = Substitute.For<ITableProperties>();
       
        // We instruct the mock to return exactly 0.2f whenever friction is requested
        mockTable.GetFrictionCoefficient().Returns(0.2f);


        var calculator = new StrikeCalculator(mockTable);
        float initialForce = 10f;
        float mass = 2f;


        // ACT: Execute the logic
        // Math: (10 force / 2 mass) - 0.2 friction = 4.8
        float result = calculator.CalculateFinalVelocity(initialForce, mass);


        // ASSERT: Verify the outcome
        Assert.AreEqual(4.8f, result, 0.001f);
    }


    [Test]
    public void CalculateFinalVelocity_FrictionOvercomesForce_ReturnsZero()
    {
        // ARRANGE
        var mockTable = Substitute.For<ITableProperties>();
        mockTable.GetFrictionCoefficient().Returns(5.0f); // Very high friction


        var calculator = new StrikeCalculator(mockTable);


        // ACT
        // Math: (5 force / 2 mass) = 2.5. Friction is 5. Logic should clamp to 0.
        float result = calculator.CalculateFinalVelocity(5f, 2f);


        // ASSERT
        Assert.AreEqual(0f, result);
    }
}
