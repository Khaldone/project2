// Assets/_Project/1_CoreDomain/Messaging/Messages.cs


// Fired when PlayFab confirms a currency change
public struct CurrencyUpdatedMessage
{
    public int NewCoinBalance { get; }
    public int NewGemBalance { get; }


    public CurrencyUpdatedMessage(int coins, int gems)
    {
        NewCoinBalance = coins;
        NewGemBalance = gems;
    }
}


// Fired when the physics engine detects a foul
public struct FoulCommittedMessage
{
    public string PlayerId { get; }
    public string FoulReason { get; } // e.g., "Sunk the Cue Ball"


    public FoulCommittedMessage(string playerId, string reason)
    {
        PlayerId = playerId;
        FoulReason = reason;
    }
}