// 1. Assets/Scripts/CoreDomain/Matchmaking/IMatchmakingService.cs
using System.Threading;
using Cysharp.Threading.Tasks;


public interface IMatchmakingService
{
    // CancellationToken allows us to abort the search safely
    UniTask<MatchResult> RequestMatchAsync(string playlistId, CancellationToken cancellationToken);
}


public class MatchResult
{
    public bool Success { get; set; }
    public string SessionName { get; set; }
    public string ErrorMessage { get; set; }
}


