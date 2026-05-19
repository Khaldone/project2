// Assets/_Project/1_CoreDomain/Physics/BilliardsPhysicsEngine.cs
using UnityEngine;
using System.Collections.Generic;


public class BilliardsPhysicsEngine_New
{
    // Evaluates a single network tick (e.g., 0.0166 seconds)
    public void StepSimulation(PhysicsBall[] balls, float tickDeltaTime)
    {
        float timeRemaining = tickDeltaTime;
        int maxSubSteps = 10; // Failsafe to prevent infinite loops in extreme clustered states
        int currentStep = 0;


        while (timeRemaining > 0.0001f && currentStep < maxSubSteps)
        {
            currentStep++;
            float earliestTOI = 1.0f;
            int colBallA = -1;
            int colBallB = -1;


            // 1. SWEEP PHASE: Find the absolute earliest collision in this sub-step
            for (int i = 0; i < balls.Length; i++)
            {
                if (!balls[i].IsActive) continue;


                for (int j = i + 1; j < balls.Length; j++)
                {
                    if (!balls[j].IsActive) continue;


                    if (CollisionMath_New.CalculateBallTOI(
                        balls[i].Position, balls[i].Velocity,
                        balls[j].Position, balls[j].Velocity,
                        timeRemaining, out float toi))
                    {
                        if (toi < earliestTOI)
                        {
                            earliestTOI = toi;
                            colBallA = i;
                            colBallB = j;
                        }
                    }
                }
            }


            // 2. ADVANCE TIME: Move all balls forward to the moment of impact
            float timeToAdvance = timeRemaining * earliestTOI;
            for (int i = 0; i < balls.Length; i++)
            {
                if (balls[i].IsActive)
                {
                    balls[i].Position += balls[i].Velocity * timeToAdvance;
                }
            }


            // 3. RESOLUTION: If a collision was found, transfer the momentum
            if (colBallA != -1 && colBallB != -1)
            {
                ResolveElasticCollision(ref balls[colBallA], ref balls[colBallB]);
            }


            // 4. DECREMENT TIME: Subtract the consumed time and loop again
            timeRemaining -= timeToAdvance;

            // Apply friction for the time advanced
            ApplyTableFriction(balls, timeToAdvance);
        }
    }


    private void ResolveElasticCollision(ref PhysicsBall a, ref PhysicsBall b)
    {
        // 1D Elastic Collision on the Normal vector
        Vector3 collisionNormal = (b.Position - a.Position).normalized;

        float relativeVelocity = Vector3.Dot(a.Velocity - b.Velocity, collisionNormal);

        // If they are already moving apart, do nothing
        if (relativeVelocity < 0) return;


        // Perfect elastic collision (Restitution = 1.0 for ideal pool balls)
        float restitution = 0.98f;
        float impulse = (1.0f + restitution) * relativeVelocity / 2.0f; // Assuming equal mass


        Vector3 impulseVector = impulse * collisionNormal;


        // Apply velocities
        a.Velocity -= impulseVector;
        b.Velocity += impulseVector;
    }


    private void ApplyTableFriction(PhysicsBall[] balls, float time)
    {
        float frictionCoefficient = 0.5f;
        for (int i = 0; i < balls.Length; i++)
        {
            if (balls[i].Velocity.sqrMagnitude > 0)
            {
                // Simple linear deceleration
                balls[i].Velocity = Vector3.MoveTowards(balls[i].Velocity, Vector3.zero, frictionCoefficient * time);
            }
        }
    }
}


public struct PhysicsBall
{
    public Vector3 Position;
    public Vector3 Velocity;
    public BallSuit Suit;
    public bool IsActive;
}
