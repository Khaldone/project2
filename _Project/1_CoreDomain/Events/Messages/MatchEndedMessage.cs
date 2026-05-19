// Assets/_Project/CoreDomain/Events/Messages/MatchEndedMessage.cs
public struct MatchEndedMessage
{
    public string WinnerId;
    public int TotalTurns;
    public bool WasDisconnect;
}