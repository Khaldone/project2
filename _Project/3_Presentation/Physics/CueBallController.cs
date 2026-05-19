// Assets/Scripts/Presentation/Physics/CueBallController.cs
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class CueBallController : MonoBehaviour, IPhysicsBody
{
    private Rigidbody _rigidbody;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }


    public float GetMass()
    {
        return _rigidbody.mass;
    }


    public void ApplyVelocity(Vector3 velocity)
    {
        // Simply pass the pure C# math directly into the Unity physics engine
        _rigidbody.linearVelocity = velocity;
    }
}
