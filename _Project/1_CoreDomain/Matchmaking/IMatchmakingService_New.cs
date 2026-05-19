// Assets/_Project/1_CoreDomain/MatchLogic/IMatchmakingService.cs
using Cysharp.Threading.Tasks;


// 1. Pure C# Structs (No Unity or SDK dependencies)
public enum MatchStatus { Searching, Found, Canceled, Failed }


public struct MatchmakingTicket
{
    public string TicketId;
    public int EstimatedWaitTimeSeconds;
}


public struct enMatchResult
{
    public MatchStatus Status;
    public string ServerIp;
    public int ServerPort;
    public string MatchId;
}

// 2. The Interface Contract
public interface IMatchmakingService_New
{
    UniTask<MatchmakingTicket> RequestMatchAsync(string queueName, int playerTrophies);
    UniTask<MatchResult> CheckTicketStatusAsync(string ticketId);
    void CancelMatchmaking(string ticketId);
}