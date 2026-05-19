// Assets/Scripts/CoreDomain/Levels/LevelDefinition.cs
using UnityEngine; // Only for Vector3 math
public class LevelDefinition
{
    public string LevelId { get; set; }
    public Vector3 CueBallStartPosition { get; set; }
    public Vector3 RackCenterPosition { get; set; }

    // Mathematical Validation: The core logic protects the game from bad data
    public bool IsSetupValid(float tableLength, float tableWidth)
    {
        // Example Rule: The cue ball cannot be placed on the wrong half of the table
        if (CueBallStartPosition.z > 0)
        {
            return false;
        }


        // Example Rule: The rack and cue ball cannot overlap
        if (Vector3.Distance(CueBallStartPosition, RackCenterPosition) < 1.0f)
        {
            return false;
        }


        return true;
    }
}
