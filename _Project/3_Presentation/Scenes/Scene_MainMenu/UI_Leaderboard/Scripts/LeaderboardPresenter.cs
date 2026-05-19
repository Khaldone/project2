// Assets/_Project/Scenes/UI_Leaderboard/Scripts/LeaderboardPresenter.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VContainer.Unity;
using UnityEngine; // Only used here for Debug.LogWarning in edge cases


public class LeaderboardPresenter : IStartable, IDisposable
{
    // 1. Injected Dependencies
    private readonly ILeagueOrchestrator _orchestrator;
    private readonly ILeaderboardView _view;

    // 2. Local State (The Master List in RAM)
    private IReadOnlyList<LeaguePlayer> _currentData;


    public LeaderboardPresenter(ILeagueOrchestrator orchestrator, ILeaderboardView view)
    {
        _orchestrator = orchestrator;
        _view = view;
    }


    // VContainer calls this automatically the moment the UI_Leaderboard scene finishes loading
    public async void Start()
    {
        // 1. Set the initial UI state
        //_view.ShowLoadingSpinner(true);
        //_view.ShowErrorState(false);


        // 2. Wire up the Observer Pattern (Subscribe to events)
        _view.OnRequestRowData += ProvideDataForRow;
        _orchestrator.OnLeaderboardUpdated += HandleDataReceived;


        // 3. Command the backend to fetch fresh data
        await FetchLeaderboardSafelyAsync();
    }


    private async Task FetchLeaderboardSafelyAsync()
    {
        try
        {
            // Ask the pure C# Brain to do the heavy lifting
            await _orchestrator.RefreshLeaderboardAsync();
        }
        catch (Exception ex)
        {
            // If PlayFab is down or the player has no internet, we catch it here
            // so the game doesn't crash, and we tell the View to show an error popup.
            Debug.LogWarning($"AAA Pipeline: Leaderboard fetch failed. {ex.Message}");
            //_view.ShowLoadingSpinner(false);
            //_view.ShowErrorState(true);
        }
    }


    // Triggered automatically whenever the Orchestrator finishes a successful backend call
    private void HandleDataReceived(IReadOnlyList<LeaguePlayer> newData)
    {
        _currentData = newData;

        //_view.ShowLoadingSpinner(false);

        // We tell the View exactly how many items exist.
        // The View's Recycled Scroll View math takes over from here.
        _view.SetTotalItemCount(_currentData.Count);
    }


    // Triggered 60 times a second by the View while the user is rapidly scrolling
    private LeaguePlayer ProvideDataForRow(int index)
    {
        // Strict bounds checking. If the View asks for row 1,005 but we only have 1,000,
        // we return an empty struct instead of throwing an IndexOutOfRangeException.
        if (_currentData != null && index >= 0 && index < _currentData.Count)
        {
            return _currentData[index];
        }

        return default;
    }


    // VContainer calls this automatically the moment the UI_Leaderboard scene unloads
    public void Dispose()
    {
        // THE GOLDEN RULE: Always unsubscribe to prevent memory leaks and zombie references
        if (_view != null)
        {
            _view.OnRequestRowData -= ProvideDataForRow;
        }


        if (_orchestrator != null)
        {
            _orchestrator.OnLeaderboardUpdated -= HandleDataReceived;
        }
    }
}
