// Assets/_Project/CoreDomain/Tournaments/TournamentState.cs
public struct TournamentState
{
    public string TournamentId; // e.g., "sydney_silver_cup"
    public int CurrentRound;    // 1 = QF, 2 = SF, 3 = Finals
    public bool IsActive;       // True if they paid the entry fee and haven't lost/won yet
}