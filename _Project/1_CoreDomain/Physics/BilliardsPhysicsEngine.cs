// Assets/_Project/CoreDomain/Physics/BilliardsPhysicsEngine.cs
using UnityEngine;

public class BilliardsPhysicsEngine
{
    private const float RESTITUTION = 0.85f; // How "bouncy" the rails are (0 to 1)

    // Simulates one deterministic network tick for a single ball
    public void SimulateBallStep(ref BallState ball, float deltaTime, LayerMask collisionLayers)
    {
        if (ball.Velocity.sqrMagnitude < 0.0001f)
        {
            ball.Velocity = Vector3.zero;
            return;
        }


        Vector3 intendedTranslation = ball.Velocity * deltaTime;
        float travelDistance = intendedTranslation.magnitude;
        Vector3 direction = intendedTranslation / travelDistance;


        // THE CCD SWEEP
        // We use Unity's math library for the sweep, but we control the state
        if (Physics.SphereCast(ball.Position, ball.Radius, direction, out RaycastHit hit, travelDistance, collisionLayers))
        {
            // 1. A collision is guaranteed this frame! Calculate TOI.
            float timeOfImpact = hit.distance / travelDistance;


            // 2. Move the ball perfectly to the point of contact
            // We pull it back slightly (0.001f) so it doesn't get mathematically stuck inside the wall
            ball.Position += direction * (hit.distance - 0.001f);


            // 3. Calculate the Reflection (Bounce)
            // Mathematical reflection: V_new = V - 2(V · N)N
            Vector3 reflectedVelocity = Vector3.Reflect(ball.Velocity, hit.normal);

            // Apply energy loss from the bounce
            ball.Velocity = reflectedVelocity * RESTITUTION;


            // 4. Resolve the remaining time in the frame!
            // If it hit halfway through the frame, it needs to spend the remaining time bouncing away
            float remainingTime = deltaTime * (1.0f - timeOfImpact);
            ball.Position += ball.Velocity * remainingTime;
        }
        else
        {
            // No collision detected. Safe to move normally.
            ball.Position += intendedTranslation;
        }


        // Apply table friction to slow the ball down
        ApplyFriction(ref ball, deltaTime);
    }

    private void ResolveElasticCollision(ref PhysicsBall a, ref PhysicsBall b)
    {
        Vector3 collisionNormal = (b.Position - a.Position).normalized;
        float relativeVelocity = Vector3.Dot(a.Velocity - b.Velocity, collisionNormal);

        if (relativeVelocity < 0) return; // Moving apart


        // ... (Momentum transfer math from earlier) ...


        // NEW: Broadcast the exact force of the impact!
        // We establish a minimum threshold (e.g., 0.1f) so micro-movements don't spam the audio bus.
        if (relativeVelocity > 0.1f)
        {
            // Find the exact midpoint of the collision
            Vector3 impactPoint = (a.Position + b.Position) * 0.5f;


            //MessageBroker.Instance.Publish(new PhysicsCollisionAudioMessage
            //{
            //    Position = impactPoint,
            //    ImpactVelocity = relativeVelocity,
            //    MaterialType = CollisionMaterial.BallVsBall
            //});
        }

        //if (relativeVelocity > 0.05f) // Sound Threshold: Ignore micro-jitters
        //{
        //    MessageBroker.Instance.Publish(new PhysicsCollisionAudioMessage
        //    {
        //        Position = (a.Position + b.Position) * 0.5f, // Midpoint
        //        ImpactVelocity = relativeVelocity,
        //        Material = CollisionMaterial.BallVsBall
        //    });
        //}

    }


    private void ApplyFriction(ref BallState ball, float deltaTime)
    {
        float frictionCoefficient = 0.98f;
        ball.Velocity *= Mathf.Pow(frictionCoefficient, deltaTime * 60f);
    }
}