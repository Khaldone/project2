// Assets/_Project/1_CoreDomain/AI/BotErrorModel.cs
using UnityEngine;


public class BotErrorModel
{
    private readonly IRngProvider _rng;


    public BotErrorModel(IRngProvider rng)
    {
        _rng = rng;
    }


    // Takes the perfect shot from the PhysicsSolver and applies realistic human error
    public ShotOption ApplyHumanError(ShotOption perfectShot, BotProfile_New profile)
    {
        // 1. DYNAMIC ERROR SCALING:
        // A straight shot 2 inches from the pocket is easy. A full-table cut shot is hard.
        // We invert the ProbabilityOfSuccess (1.0 = easiest, 0.0 = impossible)
        // to create an Error Multiplier.
        float difficultyMultiplier = 1.0f - perfectShot.ProbabilityOfSuccess;

        // Ensure even the easiest shots have a tiny chance of error (the "choke" factor)
        difficultyMultiplier = Mathf.Clamp(difficultyMultiplier, 0.1f, 1.0f);


        // 2. CORRUPT THE AIM VECTOR (Angular Jitter)
        float currentMaxAimError = profile.MaxAimErrorDegrees * difficultyMultiplier;

        // _rng.GetRandomFloatMinus1To1() returns a value between -1.0 and 1.0
        float angleDeviation = currentMaxAimError * _rng.GetRandomFloatMinus1To1();

        perfectShot.AimDirection = RotateVectorOnYAxis(perfectShot.AimDirection, angleDeviation);


        // 3. CORRUPT THE APPLIED FORCE (Speed Control Jitter)
        float currentMaxForceError = profile.MaxForceErrorPercentage * difficultyMultiplier;
        float forceDeviationPercentage = currentMaxForceError * _rng.GetRandomFloatMinus1To1();

        // Apply the percentage error (e.g., RequiredForce * 1.05 for a 5% over-hit)
        perfectShot.RequiredForce *= (1.0f + forceDeviationPercentage);
        perfectShot.RequiredForce = Mathf.Clamp01(perfectShot.RequiredForce);


        return perfectShot;
    }


    // Pure math rotation to keep this strictly within the CoreDomain without relying on Unity Transforms
    private Vector3 RotateVectorOnYAxis(Vector3 vector, float angleDegrees)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRadians);
        float sin = Mathf.Sin(angleRadians);


        return new Vector3(
            (vector.x * cos) - (vector.z * sin),
            vector.y,
            (vector.x * sin) + (vector.z * cos)
        ).normalized;
    }
}
