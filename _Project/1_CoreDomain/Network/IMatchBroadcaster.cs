// Assets/Scripts/CoreDomain/Network/IMatchBroadcaster.cs
public interface IMatchBroadcaster
{
    // Tells all connected clients who the active player is
    void BroadcastActivePlayer(int playerId);

    // Announces specific events to trigger UI or sounds locally on clients
    void BroadcastMatchEvent(MatchEventType eventType, int offendingPlayerId);
}


public enum MatchEventType { Scratch, PocketedValidBall, MatchWon }