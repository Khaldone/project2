// Assets/_Project/1_CoreDomain/Tests/BotErrorModelTests.cs
using NUnit.Framework;
using NSubstitute;
using UnityEngine;


[TestFixture]
public class BotErrorModelTests
{
    private IRngProvider _subRng;
    private BotErrorModel _errorModel;


    [SetUp]
    public void Setup()
    {
        _subRng = Substitute.For<IRngProvider>();
        _errorModel = new BotErrorModel(_subRng);
    }


    [Test]
    public void ApplyHumanError_WithBeginnerProfile_AppliesMassiveDeviation()
    {
        // ARRANGE
        // Force the RNG to return its absolute maximum value (1.0)
        _subRng.GetRandomFloatMinus1To1().Returns(1.0f);


        // Create a mathematically perfect shot that is moderately difficult (50% success probability)
        var perfectShot = new ShotOption
        {
            AimDirection = new Vector3(0, 0, 1), // Aiming straight "North"
            RequiredForce = 0.5f,
            ProbabilityOfSuccess = 0.5f
        };


        // ACT
        var corruptedShot = _errorModel.ApplyHumanError(perfectShot, BotProfiles_New.Beginner);


        // ASSERT
        // Beginner Max Aim Error = 4.5 degrees.
        // Difficulty Multiplier = (1.0 - 0.5) = 0.5.
        // Expected Deviation = 4.5 * 0.5 * 1.0(RNG) = +2.25 degrees.

        // Verify the force was corrupted:
        // Beginner Max Force Error = 30%. Multiplier = 0.5. RNG = 1.0. Error = +15%.
        // 0.5f * 1.15f = 0.575f.
        Assert.AreEqual(0.575f, corruptedShot.RequiredForce, 0.001f);

        // Verify the vector is no longer pointing straight North
        Assert.AreNotEqual(new Vector3(0, 0, 1), corruptedShot.AimDirection);
    }
}
