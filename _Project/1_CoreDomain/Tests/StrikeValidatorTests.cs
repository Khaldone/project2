using NUnit.Framework;
public class StrikeValidatorTests
{
    [Test]
    public void SanitizeInput_WhenPowerExceedsMax_ClampsToMax()
    {
        var validator = new StrikeValidator();
        var maliciousInput = new StrikeIntent { Angle = 90f, Power = 5.0f }; // Hacker trying to break the physics


        var safeInput = validator.SanitizeInput(maliciousInput);


        Assert.AreEqual(1.0f, safeInput.Power); // Neutralized!
    }

    [Test]
    public void SanitizeInput_WhenAngleExceeds360_WrapsCorrectly()
    {
        var validator = new StrikeValidator();
        var sloppyInput = new StrikeIntent { Angle = 400f, Power = 0.5f };


        var safeInput = validator.SanitizeInput(sloppyInput);


        Assert.AreEqual(40f, safeInput.Angle); // Wrapped cleanly
    }
}