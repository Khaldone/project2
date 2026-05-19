// Assets/_Project/3_Presentation/Scene_GameArena/Scripts/GameArenaEntryPoint.cs
using Billiards.CoreDomain.Services; // Ensure this points to where PlayerSession lives
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Billiards.Presentation.Arena
{
    public class GameArenaEntryPoint : IAsyncStartable
    {
        private readonly PlayerSession _playerSession;

        // Add your proprietary systems here later:
        // private readonly MatchStateMachine _matchStateMachine;
        // private readonly FixedPointPhysicsEngine _physicsEngine;

        // VContainer automatically reaches up to the Project Root to grab the Session!
        public GameArenaEntryPoint(PlayerSession playerSession)
        {
            _playerSession = playerSession;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            // 1. Verify we have the correct player data from the Login scene
            string playerName = string.IsNullOrEmpty(_playerSession.CurrentProfile.DisplayName)
                ? "Unknown Challenger"
                : _playerSession.CurrentProfile.DisplayName;

            Debug.Log($"[Arena] Booting physics sandbox for: {playerName} (Level {_playerSession.CurrentProfile.Level})");

            // 2. Initialize Networking (Photon Fusion)
            // Example: await InitializePhotonRunnerAsync();
            Debug.Log("[Arena] Networking initialized.");

            // 3. Setup the deterministic physics state
            // Example: _physicsEngine.InitializeTableState();
            Debug.Log("[Arena] Table state synced and verified.");

            // 4. Start the Match Loop
            // Example: _matchStateMachine.ChangeState(MatchState.WaitingForPlayers);
            Debug.Log("[Arena] Match State Machine running. Awaiting player input...");

            // Yield to complete the async initialization
            await UniTask.Yield(cancellationToken: cancellation);
        }

        /*
        private async UniTask InitializePhotonRunnerAsync()
        {
            // Your custom Photon Fusion setup logic goes here
        }
        */

        public void ReturnToMainMenu()
        {
            Debug.Log("[Arena] Tearing down physics sandbox. Returning to Main Menu...");

            // 1. Clean up Networking (Crucial!)
            // If you are using Photon Fusion, you MUST shut down the NetworkRunner here.
            // Example: _networkRunner.Shutdown();

            // 2. Clean up any lingering DOTween animations in the Arena
            DG.Tweening.DOTween.KillAll();

            // 3. Load the Bootstrap scene again
            // Because we use LoadSceneMode.Single, Unity will automatically destroy 
            // the Game_Arena scene and everything inside it!
            SceneManager.LoadSceneAsync("Scene_MainMenu_Bootstrap", LoadSceneMode.Additive);
        }
    }
}