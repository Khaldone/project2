// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Tournaments/PlayFabTournamentOrchestrator.cs
using System;
using System.Threading.Tasks;
using VContainer.Unity;


public class PlayFabTournamentOrchestrator : ITournamentOrchestrator, IStartable
{
    public event Action<TournamentState> OnTournamentStateChanged;
    public TournamentState CurrentState { get; private set; }


    public void Start()
    {
        // On boot, check PlayFab to see if they force-closed during an active tournament
        // FetchLatestStateFromPlayFab();
    }


    public async Task<bool> EnterTournamentAsync(string tournamentId, int entryFee)
    {
        // 1. Call a PlayFab CloudScript function: "EnterTournament"
        // 2. The CloudScript deducts the entry fee securely on the server.
        // 3. The CloudScript sets the player's internal data: "ActiveTournament" = tournamentId, "Round" = 1.

        // If successful:
        CurrentState = new TournamentState { TournamentId = tournamentId, CurrentRound = 1, IsActive = true };
        OnTournamentStateChanged?.Invoke(CurrentState);
        return true;
    }


    public async Task ReportMatchResultAsync(bool didWin)
    {
        if (!CurrentState.IsActive) return;


        // In a real AAA game, your Dedicated Server or a secure PlayFab CloudScript
        // verifies this win so hackers can't just send "I won!" to the server.

        if (didWin)
        {
            if (CurrentState.CurrentRound == 3) // They won the Finals!
            {
                // CloudScript grants the massive coin prize and sets IsActive = false
                CurrentState = new TournamentState { IsActive = false };
            }
            else
            {
                // CloudScript increments Round to 2 or 3
                CurrentState = new TournamentState
                {
                    TournamentId = CurrentState.TournamentId,
                    CurrentRound = CurrentState.CurrentRound + 1,
                    IsActive = true
                };
            }
        }
        else
        {
            // They lost. CloudScript wipes their active tournament status.
            CurrentState = new TournamentState { IsActive = false };
        }


        OnTournamentStateChanged?.Invoke(CurrentState);
    }

    public Task ForfeitTournamentAsync() { /* ... */ return Task.CompletedTask; }
}