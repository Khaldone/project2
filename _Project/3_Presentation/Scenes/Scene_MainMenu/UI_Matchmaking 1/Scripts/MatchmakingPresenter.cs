// Assets/_Project/3_Presentation/Scenes/Scene_MainMenu/UI_Matchmaking 1/Scripts/MatchmakingPresenter.cs
// The Presenter. Pure C# orchestrator — no MonoBehaviour, no UnityEngine.UI references.
// Drives the MatchmakingScreen (View) through its public methods.
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Billiards.Presentation
{
    public class MatchmakingPresenter : IInitializable, IDisposable
    {
        private readonly MatchmakingScreen _view;

        private CancellationTokenSource _searchCts;
        private bool _isSearching;

        public MatchmakingPresenter(MatchmakingScreen view)
        {
            _view = view;
        }

        public void Initialize()
        {
            Debug.Log("[MatchmakingPresenter] Initialized. Subscribing to View events.");

            _view.OnCancelClicked += HandleCancelClicked;
            _view.OnMenuShown += HandleMenuShown;
            _view.OnMenuHidden += HandleMenuHidden;
        }

        private void HandleMenuHidden()
        {
            if (_isSearching && _searchCts != null && !_searchCts.IsCancellationRequested)
            {
                Debug.Log("[MatchmakingPresenter] Menu hidden. Cancelling background search sequence.");
                _searchCts.Cancel();
            }
        }

        private void HandleMenuShown()
        {
            if (!_isSearching)
            {
                RunMatchmakingSequenceAsync().Forget();
            }
        }

        /// <summary>
        /// The full matchmaking visual sequence:
        /// 1. Reset the UI
        /// 2. Populate player info
        /// 3. Play intro animations (panels slide in, UV scroll starts)
        /// 4. Wait for a match (simulated 5s delay for now)
        /// 5. Populate opponent info
        /// 6. Play match found animations (flash, level bars, coins)
        /// </summary>
        private async UniTaskVoid RunMatchmakingSequenceAsync()
        {
            // Dispose any previous CTS to prevent leaks
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            
            _searchCts = new CancellationTokenSource();
            _isSearching = true;

            try
            {
                var token = _searchCts.Token;

                // 1. Reset everything to pre-animation state
                _view.ResetToDefault();

                // 2. Populate local player info (hardcoded placeholders until IPlayerDataService is wired)
                _view.SetPlayerInfo("You", 12, "5,000");
                _view.SetBetAmount("1,000");

                // 3. Play the intro — panels slide in, search scroll starts
                Debug.Log("[MatchmakingPresenter] Playing intro animation...");
                await _view.PlayIntroAnimationAsync(token);

                // 4. Simulate searching for an opponent
                // TODO: Replace with real IMatchmakingService.RequestMatchAsync() when networking is integrated
                Debug.Log("[MatchmakingPresenter] Searching for opponent (simulated 2s)...");
                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);

                // 5. Match "found" — populate the opponent info
                _view.SetOpponentInfo("Opponent", 15, "5,000");

                // 6. Play the match found sequence (flash, level bars, coins)
                Debug.Log("[MatchmakingPresenter] Match found! Playing match found animation...");
                await _view.PlayMatchFoundAnimationAsync(token);

                _isSearching = false;
                Debug.Log("[MatchmakingPresenter] Matchmaking sequence complete.");

                // TODO: After real integration, transition to the Game Arena scene here
                // await _entryPoint.TransitionToGameArenaAsync();
            }
            catch (OperationCanceledException)
            {
                _isSearching = false;
                Debug.Log("[MatchmakingPresenter] Search was cancelled by the player.");
                // The NavHandler's GoToHome() handles the actual screen transition
            }
        }

        private void HandleCancelClicked()
        {
            if (_isSearching && _searchCts != null && !_searchCts.IsCancellationRequested)
            {
                Debug.Log("[MatchmakingPresenter] Cancel requested.");
                _searchCts.Cancel();
            }
        }

        public void Dispose()
        {
            Debug.Log("[MatchmakingPresenter] Disposing. Unsubscribing from View events.");

            if (_view != null)
            {
                _view.OnCancelClicked -= HandleCancelClicked;
                _view.OnMenuShown -= HandleMenuShown;
                _view.OnMenuHidden -= HandleMenuHidden;
            }

            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = null;
        }
    }
}
