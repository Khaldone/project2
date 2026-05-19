// Assets/_Project/1_CoreDomain/MatchLogic/MatchCoordinator.BallInHand.cs
using UnityEngine;
using System.Collections.Generic;


public partial class MatchCoordinator
{
    private const float CUE_BALL_RADIUS = 0.0285f;
    private const float HEADSTRING_X = -0.6f; // Assuming 0 is center, left side is headstring


    // The UI attempts to place the ball and asks the server for permission
    public bool TryPlaceCueBall(Vector3 requestedPosition, List<Vector3> otherBallPositions)
    {
        if (!_state.IsBallInHand) return false;


        // 1. CHEAT CHECK: Is it off the table entirely?
        if (!IsInsideTableBounds(requestedPosition)) return false;


        // 2. RULES CHECK: If it was a break foul, it MUST be behind the headstring
        if (_state.WasBreakFoul && requestedPosition.x > HEADSTRING_X)
        {
            return false; // UI should show a red error circle
        }


        // 3. PHYSICS CHECK: Are they placing it inside another ball?
        float minimumDistanceSq = (CUE_BALL_RADIUS * 2) * (CUE_BALL_RADIUS * 2);
        foreach (var ballPos in otherBallPositions)
        {
            if ((requestedPosition - ballPos).sqrMagnitude < minimumDistanceSq)
            {
                return false; // Intersecting another ball
            }
        }


        // Placement is legal!
        _state.IsBallInHand = false;

        // Tell the network/physics engine to finalize the placement
        //_broker.Publish(new CueBallPlacedMessage(requestedPosition));
        return true;
    }


    private bool IsInsideTableBounds(Vector3 pos)
    {
        // Simple bounding box check based on your table dimensions
        return pos.x >= -1.2f && pos.x <= 1.2f && pos.z >= -0.6f && pos.z <= 0.6f;
    }
}
