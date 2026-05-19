// Assets/Scripts/CoreDomain/UI/MatchUIPresenter.cs
using System;
using VContainer.Unity;

public class MatchUIPresenter : IStartable, IDisposable
{
    private readonly IMatchUIView _view;
    private readonly MatchCoordinator _matchCoordinator;
    private readonly MatchmakingOrchestrator _matchOrchestrator;

    
    // Dependency Injection brings the View and the Model together
    public MatchUIPresenter(IMatchUIView view, MatchCoordinator matchCoordinator, MatchmakingOrchestrator matchmakingOrchestrator)
    {
        _view = view;
        _matchCoordinator = matchCoordinator;
        _matchOrchestrator = matchmakingOrchestrator;

        // Subscribe to the pure C# events
        _matchCoordinator.OnPlayerTurnChanged += HandleTurnChanged;

        // Initialize the view with the current state
        HandleTurnChanged(_matchCoordinator.CurrentPlayerId);
    }


    private void HandleTurnChanged(int newPlayerId)
    {
        _view.UpdateTurnIndicator(newPlayerId);

        // Example logic: Only Player 1 can click "End Turn" manually
        _view.SetEndTurnButtonInteractable(newPlayerId == 1);
    }

    public void Start()
    {
        // The View paints the screen.
        // If it's a bot, it says "Guest_8492 from Brazil (Level 12)".
        // If it's a human, it says the exact same format. The UI is none the wiser.
        var opponent = _matchOrchestrator.CurrentOpponent;

        //.SetOpponentData(opponent.DisplayName, opponent.CountryCode, opponent.Level);
    }

    // AAA Standard: Always unsubscribe to prevent memory leaks
    public void Dispose()
    {
        _matchCoordinator.OnPlayerTurnChanged -= HandleTurnChanged;
    }
}
