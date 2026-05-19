// 2. Assets/_Project/CoreDomain/AI/StandardBotBrain.cs
using System.Collections.Generic;
using UnityEngine; // Allowed for basic Vector3 math
public class StandardBotBrain : IBotBrain
{
    public StrikeIntent CalculateBestShot(IReadOnlyList<BallState> currentTableState, BallState cueBallState)
    {
        // ... Complex raycasting and angle calculation goes here ...
        // For demonstration, we simply aim directly at the first available ball

        if (currentTableState.Count == 0)
            return new StrikeIntent { Angle = 0, Power = 0.5f };


        BallState targetBall = currentTableState[0];


        // Calculate the vector from the Cue Ball to the Target Ball
        Vector3 directionToTarget = targetBall.Position - cueBallState.Position;

        // Convert the 3D direction into a 2D angle (0-360 degrees)
        float angle = Mathf.Atan2(directionToTarget.z, directionToTarget.x) * Mathf.Rad2Deg;


        return new StrikeIntent
        {
            Angle = angle,
            Power = 0.75f // Hit it with 75% power
        };
    }
}