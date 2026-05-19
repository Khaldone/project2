// Assets/Scripts/CoreDomain/Physics/IPhysicsBody.cs
using UnityEngine;
public interface IPhysicsBody
{
    float GetMass();
    void ApplyVelocity(Vector3 velocity);
}
