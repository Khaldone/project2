// 2. Assets/_Project/CoreDomain/Physics/BallPhysicsSimulator.cs
// The Brain. This does the heavy lifting entirely offline.
using UnityEngine;
public class BallPhysicsSimulator
{
    private const float FRICTION = 0.98f;
    private const float STOP_THRESHOLD = 0.05f;
    public BallState CalculateNextFrame(BallState currentState, float deltaTime)
    {
        if (currentState.IsResting) return currentState;


        BallState nextState = currentState;


        // Apply pure mathematical physics
        nextState.Position += nextState.Velocity * deltaTime;
        nextState.Velocity *= FRICTION; // Apply drag


        if (nextState.Velocity.magnitude < STOP_THRESHOLD)
        {
            nextState.Velocity = Vector3.zero;
            nextState.IsResting = true;
        }

        return nextState;
    }
}