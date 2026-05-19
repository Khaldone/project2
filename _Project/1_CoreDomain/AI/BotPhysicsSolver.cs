// Assets/_Project/1_CoreDomain/AI/BotPhysicsSolver.cs
using UnityEngine;
using System.Collections.Generic;


public interface IPhysicsSolver
{
    bool TryCalculatePottingShot(PhysicsBall[] table, int targetIndex, int pocketIndex, out ShotOption shot);
    Vector3 PredictCueBallRestPosition(PhysicsBall[] table, ShotOption shot);
}


public class BotPhysicsSolver : IPhysicsSolver
{
    private const float BALL_RADIUS = CollisionMath_New.BALL_RADIUS;
    private const float BALL_DIAMETER = BALL_RADIUS * 2.0f;
    private const float BALL_DIAMETER_SQ = BALL_DIAMETER * BALL_DIAMETER;


    public bool TryCalculatePottingShot(PhysicsBall[] table, int targetIndex, int pocketIndex, out ShotOption shot)
    {
        shot = new ShotOption { TargetBallIndex = targetIndex, TargetPocketIndex = pocketIndex, Type = ShotType.Potting };

        Vector3 cuePos = table[0].Position;
        Vector3 targetPos = table[targetIndex].Position;
        Vector3 pocketPos = GetPocketPosition(pocketIndex); // Assuming a method that returns the fixed Vector3 of the pocket


        // 1. REVERSE ENGINEER: The Pocket-to-Target Line
        Vector3 pocketDir = (pocketPos - targetPos).normalized;

        // 2. THE GHOST BALL: Where the cue ball must be at the exact moment of impact
        // We move exactly one ball diameter away from the target, in the opposite direction of the pocket
        Vector3 ghostBallPos = targetPos - (pocketDir * BALL_DIAMETER);


        // 3. THE AIM VECTOR: The line from the current Cue Ball to the Ghost Ball
        Vector3 aimDir = (ghostBallPos - cuePos).normalized;


        // 4. CUT ANGLE CHECK: Is this physically possible?
        // If the dot product is less than 0, the ghost ball is "inside" or "behind" the target ball relative to the cue.
        // We use a threshold of 0.1f to prevent the AI from attempting mathematically perfect 89-degree cuts that always fail in reality.
        float cutAngleDot = Vector3.Dot(aimDir, pocketDir);
        if (cutAngleDot < 0.1f) return false;


        // 5. OBSTRUCTION CHECK A: Cue Ball to Ghost Ball (Can we hit the ghost ball?)
        if (!IsPathClear(table, cuePos, ghostBallPos, ignoreIndexA: 0, ignoreIndexB: targetIndex))
            return false;


        // 6. OBSTRUCTION CHECK B: Target Ball to Pocket (Will the target ball hit another ball on the way in?)
        if (!IsPathClear(table, targetPos, pocketPos, ignoreIndexA: 0, ignoreIndexB: targetIndex))
            return false;


        // ALL CLEAR! This shot is physically possible.
        shot.AimDirection = aimDir;

        // Calculate force based on total travel distance (simplified for this example)
        float totalDistance = Vector3.Distance(cuePos, ghostBallPos) + Vector3.Distance(targetPos, pocketPos);
        shot.RequiredForce = Mathf.Clamp01(totalDistance * 0.3f); // Scaling factor for your physics engine


        return true;
    }


    public Vector3 PredictCueBallRestPosition(PhysicsBall[] table, ShotOption shot)
    {
        Vector3 cuePos = table[0].Position;
        Vector3 targetPos = table[shot.TargetBallIndex].Position;
        Vector3 pocketPos = GetPocketPosition(shot.TargetPocketIndex);


        Vector3 pocketDir = (pocketPos - targetPos).normalized;
        Vector3 ghostBallPos = targetPos - (pocketDir * BALL_DIAMETER);

        // The Cue Ball deflects at exactly 90 degrees (a tangent) from the impact vector
        Vector3 impactNormal = (targetPos - ghostBallPos).normalized;
        Vector3 tangentDir = Vector3.Cross(impactNormal, Vector3.up).normalized;


        // Ensure the tangent goes forward relative to the initial aim
        if (Vector3.Dot(tangentDir, shot.AimDirection) < 0)
        {
            tangentDir = -tangentDir;
        }


        // Estimate how far it rolls based on the cut angle.
        // A straight shot (Dot = 1) kills the cue ball (stops dead). A thin cut (Dot = 0) maintains momentum.
        float momentumKept = 1.0f - Vector3.Dot(shot.AimDirection, pocketDir);
        float rollDistance = shot.RequiredForce * 3.0f * momentumKept; // 3.0f is a fictional friction modifier


        return ghostBallPos + (tangentDir * rollDistance);
    }


    // --- The Pure Math Cylinder Intersection ---
    private bool IsPathClear(PhysicsBall[] table, Vector3 start, Vector3 end, int ignoreIndexA, int ignoreIndexB)
    {
        Vector3 lineDir = end - start;
        float lineLengthSq = lineDir.sqrMagnitude;
        lineDir.Normalize();


        for (int i = 1; i < table.Length; i++)
        {
            if (!table[i].IsActive || i == ignoreIndexA || i == ignoreIndexB) continue;


            Vector3 obstaclePos = table[i].Position;
            Vector3 startToObstacle = obstaclePos - start;


            // Project the obstacle onto our line to find the closest point
            float t = Vector3.Dot(startToObstacle, lineDir);


            // If t < 0, the obstacle is behind the cue ball.
            // If t * t > lineLengthSq, the obstacle is past the target.
            if (t > 0 && (t * t) < lineLengthSq)
            {
                // Find the perpendicular distance from the line to the obstacle
                Vector3 closestPointOnLine = start + (lineDir * t);
                float distanceSq = (obstaclePos - closestPointOnLine).sqrMagnitude;


                // If the distance is less than the diameter of a ball, it's a collision!
                if (distanceSq < BALL_DIAMETER_SQ)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private Vector3 GetPocketPosition(int index) { /* Implementation omitted */ return Vector3.zero; }
}
