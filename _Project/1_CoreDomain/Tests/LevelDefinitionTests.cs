using NUnit.Framework;
using UnityEngine;
public class LevelDefinitionTests
{
    [Test]
    public void IsSetupValid_CueBallInRackHalf_ReturnsFalse()
    {
        var level = new LevelDefinition
        {
            CueBallStartPosition = new Vector3(0, 0, 2f), // Invalid: Past the center line
            RackCenterPosition = new Vector3(0, 0, 5f)
        };


        bool isValid = level.IsSetupValid(10f, 5f);
        Assert.IsFalse(isValid);
    }


    [Test]
    public void IsSetupValid_StandardSetup_ReturnsTrue()
    {
        var level = new LevelDefinition
        {
            CueBallStartPosition = new Vector3(0, 0, -5f), // Valid: Behind the head string
            RackCenterPosition = new Vector3(0, 0, 5f)
        };


        bool isValid = level.IsSetupValid(10f, 5f);
        Assert.IsTrue(isValid);
    }
}
