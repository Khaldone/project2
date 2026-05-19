// Assets/_Project/1_CoreDomain/MatchLogic/AdvancedMatchStructs.cs


// Defines exactly what went where
public struct PocketEvent
{
    public BallSuit Suit;
    public int PocketId; // 0-5 representing the 6 pockets
}


// Emitted every tick so the UI slider can update
public struct TurnTimerSyncMessage
{
    public float TimeRemaining;
    public float TimeTotal;
    public bool IsPanicMode; // Triggers the heartbeat audio
}


// Emitted when the Coordinator demands the player select a pocket
public struct CallPocketRequestedMessage
{
    public BallSuit TargetSuit; // e.g., "Call the 8-Ball pocket"
}