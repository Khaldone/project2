// Assets/_Project/1_CoreDomain/MatchLogic/MatchMessages.cs


// Fired when the turn changes
public struct TurnChangedMessage_New
{
    public string ActivePlayerId;
}


// Fired when a suit is assigned or a ball is pocketed
public struct PlayerStateUpdatedMessage
{
    public string PlayerId;
    public BallSuit AssignedSuit;
    public int BallsRemaining;
}