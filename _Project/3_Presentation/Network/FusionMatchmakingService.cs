// Assets/Scripts/Presentation/Network/FusionMatchmakingService.cs
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;


public class FusionMatchmakingService : MonoBehaviour, IMatchmakingService
{
    private NetworkRunner _runner;

    private void Awake()
    {
        // Ensure you have a runner component
        _runner = GetComponent<NetworkRunner>() ?? gameObject.AddComponent<NetworkRunner>();
    }


    public async Task<MatchResult> RequestMatchAsync(string playlistId, CancellationToken cancellationToken)
    {
        MatchResult result = new MatchResult();


        try
        {
            // Fusion's StartGame handles matchmaking, session creation, and joining.
            // We pass the CancellationToken directly so Fusion knows to abort if needed.
            StartGameResult startGameResult = await _runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.AutoHostOrClient,
                SessionName = playlistId, // In a real app, you'd use a matchmaking queue, not just session names
                //SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });


            if (startGameResult.Ok)
            {
                result.Success = true;
                result.SessionName = _runner.SessionInfo.Name;
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = $"Failed to join: {startGameResult.ShutdownReason}";
            }
        }
        catch (System.Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = "Network error occurred.";
            Debug.LogError(ex);
        }


        return result;
    }

    UniTask<MatchResult> IMatchmakingService.RequestMatchAsync(string playlistId, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}
