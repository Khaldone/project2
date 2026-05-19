// Assets/_Project/1_CoreDomain/MatchLogic/StateHydrationStructs.cs
using UnityEngine;
using System.Collections.Generic;


// This is the payload sent from the Server/Host to the reconnecting Client
public struct MatchSnapshot
{
    // 1. Physics State
    public PhysicsBall[] BallStates; // Contains Position, Velocity, and IsActive for all 16 balls

    // 2. Turn & Rules State
    public string ActivePlayerId;
    public float TurnTimeRemaining;
    public bool IsOpenTable;
    public bool IsBallInHand;

    // 3. Player Profiles
    public Dictionary<string, BallSuit> PlayerSuits;
    public Dictionary<string, int> PlayerBallsRemaining;
}