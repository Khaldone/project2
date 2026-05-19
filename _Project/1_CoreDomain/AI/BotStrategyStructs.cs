// Assets/_Project/1_CoreDomain/AI/BotStrategyStructs.cs (Appended)

using UnityEngine;
public struct BotProfile_New
{
    // Strategy Weights (From earlier)
    public float PottingWeight;
    public float PositionalWeight;
    public float SafetyThreshold;


    // --- NEW: The Error Model Profiles ---

    // The maximum angular deviation applied to the aim vector (in degrees)
    public float MaxAimErrorDegrees;

    // The maximum percentage the bot can miscalculate the power (e.g., 0.20 = 20% error)
    public float MaxForceErrorPercentage;
}

public enum ShotType { Potting, Safety, Break }

// The blueprint of a shot the AI is considering
public struct ShotOption
{
    public ShotType Type;
    public int TargetBallIndex;
    public int TargetPocketIndex;

    // The physical execution
    public Vector3 AimDirection;
    public float RequiredForce;

    // The Heuristic Scores (0.0f to 1.0f)
    public float ProbabilityOfSuccess;
    public float PositionalValue;      // Where does the cue ball end up?
    public float ClusterBreakValue;    // Does it free up our trapped balls?

    // The final combined score based on the bot's difficulty weighting
    public float TotalUtilityScore;
}


public static class BotProfiles_New
{
    public static BotProfile_New Beginner => new BotProfile_New
    {
        PottingWeight = 0.9f,
        PositionalWeight = 0.1f,
        SafetyThreshold = 0.1f,
        MaxAimErrorDegrees = 4.5f,        // Wildly inaccurate
        MaxForceErrorPercentage = 0.30f   // Can hit 30% too hard or too soft
    };


    public static BotProfile_New Pro => new BotProfile_New
    {
        PottingWeight = 0.6f,
        PositionalWeight = 0.6f,
        SafetyThreshold = 0.4f,
        MaxAimErrorDegrees = 0.8f,        // Very accurate
        MaxForceErrorPercentage = 0.05f   // Excellent speed control
    };


    public static BotProfile_New Master => new BotProfile_New
    {
        PottingWeight = 0.5f,
        PositionalWeight = 0.8f,
        SafetyThreshold = 0.6f,
        MaxAimErrorDegrees = 0.1f,        // Practically flawless aim
        MaxForceErrorPercentage = 0.01f   // Perfect speed control
    };
}
