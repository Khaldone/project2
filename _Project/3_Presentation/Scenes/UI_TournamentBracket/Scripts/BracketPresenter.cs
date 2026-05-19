// Assets/_Project/Scenes/UI_TournamentBracket/Scripts/BracketPresenter.cs
using PlayFab.EconomyModels;
using VContainer.Unity;


public class BracketPresenter : IStartable
{
    private readonly ITournamentOrchestrator _tournament;
    private readonly MatchmakingOrchestrator _matchmaking;
    //private readonly IBracketView _view;

    //public BracketPresenter(ITournamentOrchestrator tournament, MatchmakingOrchestrator matchmaking, IBracketView view)
    //{
    //    _tournament = tournament;
    //    _matchmaking = matchmaking;
    //    _view = view;
    //}

    public BracketPresenter(ITournamentOrchestrator tournament, MatchmakingOrchestrator matchmaking)
    {
        _tournament = tournament;
        _matchmaking = matchmaking;
    }

    public void Start()
    {
        //_view.OnPlayButtonClicked += HandlePlayClicked;

        // Draw the visual bracket based on current round
        //_view.UpdateBracketVisuals(_tournament.CurrentState.CurrentRound);
    }

    private async void HandlePlayClicked()
    {
        //_view.ShowSearchingSpinner(true);

        // Pass the state to our updated matchmaking logic
        await _matchmaking.StartTournamentMatchmakingAsync(_tournament.CurrentState);


        // Once connected (to a real player or a bot), overwrite the bracket UI
        // with the opponent's actual data so the player believes this opponent
        // was waiting for them in the bracket all along.
        //_view.SetOpponentData(_matchmaking.CurrentOpponent);

        // Tell the UI router to load the Game Arena
    }
}