// Assets/_Project/CoreDomain/Physics/CollisionMath.cs
using UnityEngine;
public static class CollisionMath
{
    // The physics engine needs to know the exact point and normal of a hit
    public struct SweepResult
    {
        public bool Hit;
        public float TimeOfImpact; // 0.0 to 1.0
        public Vector3 HitPoint;
        public Vector3 SurfaceNormal;
    }
}