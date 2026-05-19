// Assets/_Project/1_CoreDomain/Physics/Math/CollisionMath.cs
using UnityEngine;
using System;


public static class CollisionMath_New
{
    // The standard radius for a pool ball (e.g., 0.0285 meters)
    public const float BALL_RADIUS = 0.0285f;
    private const float TWO_RADII_SQ = (BALL_RADIUS * 2) * (BALL_RADIUS * 2);


    /// <summary>
    /// Calculates the Time of Impact (TOI) between two moving balls.
    /// Returns true if they collide within the time step (0.0 to 1.0).
    /// </summary>
    public static bool CalculateBallTOI(
        Vector3 p1, Vector3 v1,
        Vector3 p2, Vector3 v2,
        float timeStep,
        out float toi)
    {
        toi = 1.0f; // Default to end of tick


        Vector3 deltaP = p2 - p1;
        Vector3 deltaV = (v2 - v1) * timeStep; // Scale velocity by the tick duration


        float a = Vector3.Dot(deltaV, deltaV);
        float b = 2.0f * Vector3.Dot(deltaP, deltaV);
        float c = Vector3.Dot(deltaP, deltaP) - TWO_RADII_SQ;


        // If 'a' is near zero, the balls are moving at the exact same velocity and won't collide
        if (Mathf.Abs(a) < 0.0001f) return false;


        // The Discriminant (b^2 - 4ac)
        float discriminant = (b * b) - (4.0f * a * c);


        // If discriminant is negative, the mathematical roots are imaginary (the balls miss each other completely)
        if (discriminant < 0) return false;


        // Calculate the smallest positive root (the first moment of impact)
        float sqrtDisc = Mathf.Sqrt(discriminant);
        float t1 = (-b - sqrtDisc) / (2.0f * a);


        // Check if the collision happens within this specific network tick (between time 0 and 1)
        if (t1 >= 0.0f && t1 <= 1.0f)
        {
            toi = t1;
            return true;
        }


        return false;
    }
}