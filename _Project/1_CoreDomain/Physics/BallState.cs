// 1. Assets/_Project/CoreDomain/Physics/BallState.cs
using UnityEngine; // Allowed for Vector3 math structs

// A pure data container. No logic. No network dependencies.
public struct BallState
{
    public int BallId;
    public Vector3 Position;
    public Vector3 Velocity;
    public float Radius;
    public bool IsActive;
    public bool IsResting;
}