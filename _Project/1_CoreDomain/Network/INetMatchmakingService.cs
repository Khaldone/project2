// 1. Assets/_Project/CoreDomain/Networking/IMatchmakingService.cs
using System.Threading.Tasks;


public interface INetMatchmakingService
{
    Task<bool> JoinOrCreateMatchAsync(string roomName);
    Task DisconnectAsync();
}
