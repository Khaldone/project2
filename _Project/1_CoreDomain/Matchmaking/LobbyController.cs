// 3. Assets/Scripts/CoreDomain/Matchmaking/LobbyController.cs
// This is the Brain. It dictates the flow of the matchmaking experience.
using System;
using System.Threading;
using Cysharp.Threading.Tasks;


public class LobbyController : IDisposable
{
    private readonly IMatchmakingService _matchmakingService;
    private readonly ILobbyUIView _uiView;
    private readonly ISceneLoader _sceneLoader;

    private CancellationTokenSource _searchCancellationTokenSource;


    public LobbyController(IMatchmakingService matchmakingService, ILobbyUIView uiView, ISceneLoader sceneLoader)
    {
        _matchmakingService = matchmakingService;
        _uiView = uiView;
        _sceneLoader = sceneLoader;

        _uiView.ShowDefaultState();
    }


    public async UniTask StartSearchAsync(string playlistId)
    {
        _uiView.EnableInteractability(false);
        _uiView.ShowSearchingState("Calculating...");


        // Create a new token so the player can cancel this specific search
        _searchCancellationTokenSource = new CancellationTokenSource();


        try
        {
            MatchResult result = await _matchmakingService.RequestMatchAsync(playlistId, _searchCancellationTokenSource.Token);


            if (result.Success)
            {
                _uiView.ShowMatchFoundState();
                // Wait a brief moment for players to see the "Match Found!" UI
                await UniTask.Delay(1500);
                _sceneLoader.LoadScene("Game_Arena");
            }
            else
            {
                _uiView.ShowError(result.ErrorMessage);
                _uiView.ShowDefaultState();
                _uiView.EnableInteractability(true);
            }
        }
        catch (OperationCanceledException)
        {
            // The user clicked cancel. Reset gracefully.
            _uiView.ShowDefaultState();
            _uiView.EnableInteractability(true);
        }
    }


    public void CancelSearch()
    {
        if (_searchCancellationTokenSource != null && !_searchCancellationTokenSource.IsCancellationRequested)
        {
            _searchCancellationTokenSource.Cancel();
        }
    }


    public void Dispose()
    {
        _searchCancellationTokenSource?.Dispose();
    }
}
