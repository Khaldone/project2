// Assets/_Project/1_CoreDomain/Audio/AudioMessages.cs
using UnityEngine;

public enum CollisionMaterial { BallVsBall, BallVsRail, BallVsPocket }

public struct PhysicsCollisionAudioMessage
{
    public Vector3 Position;
    public float ImpactVelocity; // The kinetic energy of the crash
    public CollisionMaterial MaterialType;
}