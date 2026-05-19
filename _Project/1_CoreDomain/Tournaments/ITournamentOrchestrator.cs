// Assets/_Project/CoreDomain/Tournaments/ITournamentOrchestrator.cs
using System;
using System.Threading.Tasks;

public interface ITournamentOrchestrator
{
    event Action<TournamentState> OnTournamentStateChanged;
    TournamentState CurrentState { get; }

    Task<bool> EnterTournamentAsync(string tournamentId, int entryFee);
    Task ForfeitTournamentAsync();

    // Called securely by your MatchCoordinator when a match ends
    Task ReportMatchResultAsync(bool didWin);
}