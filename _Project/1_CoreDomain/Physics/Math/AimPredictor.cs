// Assets/_Project/1_CoreDomain/Physics/Math/AimPredictor.cs
using UnityEngine;
using System.Collections.Generic;


public struct AimTrajectoryData
{
    public Vector3 CueBallStart;
    public Vector3 GhostBallPosition;
    public Vector3 TargetBallDeflectionEnd;
    public Vector3 CueBallTangentEnd;
    public bool DidHitTarget;
    public int TargetBallIndex;
}


public class AimPredictor
{
    private const float BALL_RADIUS = CollisionMath_New.BALL_RADIUS;
    private const float TWO_RADII_SQ = (BALL_RADIUS * 2) * (BALL_RADIUS * 2);


    // This method is called every frame the user drags their finger to aim
    public AimTrajectoryData CalculateTrajectory(
        Vector3 cueStart,
        Vector3 aimDirection,
        PhysicsBall[] currentTableState,
        float cueAccuracyStat, // E.g., 0.0 to 1.0 based on their equipped Cue
        float difficultyLineMultiplier) // Shortens lines in high tiers
    {
        AimTrajectoryData result = new AimTrajectoryData
        {
            CueBallStart = cueStart,
            DidHitTarget = false,
            TargetBallIndex = -1
        };


        float closestImpactDistance = float.MaxValue;


        // 1. Find the first ball we intersect with
        for (int i = 0; i < currentTableState.Length; i++)
        {
            // Skip the cue ball itself or pocketed balls
            if (i == 0 || !currentTableState[i].IsActive) continue;


            Vector3 targetCenter = currentTableState[i].Position;
            Vector3 L = targetCenter - cueStart;
            float tca = Vector3.Dot(L, aimDirection);


            if (tca < 0) continue; // Target is behind us


            float d2 = L.sqrMagnitude - (tca * tca);

            if (d2 > TWO_RADII_SQ) continue; // We missed this ball


            float thc = Mathf.Sqrt(TWO_RADII_SQ - d2);
            float impactDistance = tca - thc;


            if (impactDistance < closestImpactDistance)
            {
                closestImpactDistance = impactDistance;
                result.DidHitTarget = true;
                result.TargetBallIndex = i;
                result.GhostBallPosition = cueStart + (aimDirection * impactDistance);
            }
        }


        // 2. Calculate the Deflection (Tangent) Vectors
        if (result.DidHitTarget)
        {
            Vector3 targetBallPos = currentTableState[result.TargetBallIndex].Position;

            // The target ball is pushed perfectly away from the ghost ball's center
            Vector3 targetDeflectionDir = (targetBallPos - result.GhostBallPosition).normalized;

            // The cue ball deflects at a 90-degree tangent.
            // We use the cross product with the UP vector (Y-axis) to find the 2D perpendicular.
            Vector3 cueTangentDir = Vector3.Cross(targetDeflectionDir, Vector3.up).normalized;

            // Ensure the cue tangent goes "forward" relative to the original aim
            if (Vector3.Dot(cueTangentDir, aimDirection) < 0)
            {
                cueTangentDir = -cueTangentDir;
            }


            // 3. Apply RPG Stats & Tier Difficulty to shorten the visual lines
            // A basic cue might only show 0.5 meters of line. A Legendary cue shows 2.5 meters.
            float targetLineLength = Mathf.Lerp(0.3f, 2.5f, cueAccuracyStat) * difficultyLineMultiplier;
            float cueLineLength = targetLineLength * 0.3f; // Cue tangent is usually drawn much shorter


            result.TargetBallDeflectionEnd = targetBallPos + (targetDeflectionDir * targetLineLength);
            result.CueBallTangentEnd = result.GhostBallPosition + (cueTangentDir * cueLineLength);
        }
        else
        {
            // If we hit nothing, just draw a straight line to the rail
            // (Rail intersection math omitted for brevity, but uses standard Ray-Plane intersection)
            result.GhostBallPosition = cueStart + (aimDirection * 5.0f);
        }


        return result;
    }
}