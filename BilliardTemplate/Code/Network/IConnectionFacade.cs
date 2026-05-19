/*using System;
using System.Threading;
using System.Threading.Tasks;
using Fusion;

namespace ibc.network
{
    public enum NetFailCode
    {
        None,
        Cancelled,
        Timeout,
        AuthFailed,
        MatchmakingFailed,
        StartFailed,
        JoinFailed,
        SpectateFailed,
        RejoinNotPossible,
        Disconnected,
        Unknown,
    }
    
    public interface IConnectionFacade
    {
        Task SignInAsync(CancellationToken cancellationToken);
        Task FindMatchAsync(MatchRequest req, CancellationToken ct);
        Task SpectateAsync(SpectateRequest req, CancellationToken ct);
        Task<bool> TryRejoinAsync(CancellationToken ct);
        Task LeaveToMenuAsync(CancellationToken ct);
    }
    
    public interface IFusionClient
    {
        
        event Action<FusionDisconnectInfo> Disconnected;
        event Action<FusionConnectInfo> Connected;
        bool IsRunning { get; }
        
        Task StartAsync(FusionStartArgs args, CancellationToken ct);
        Task StopAsync(CancellationToken ct);
    }

    public interface IMatchmakingService
    {
        Task<JoinPlan> FindMatchAsync(PlayerProfile me, MatchRequest req, CancellationToken ct);
        Task<JoinPlan> GetRejoinPlanAsync(PlayerProfile me, RejoinTicket ticket, CancellationToken ct);
        Task<IReadOnlyList<SpectateEntry>> ListSpectatableAsync(SpectateQuery q, CancellationToken ct);
    }
    
    public interface IRejoinStore
    {
        bool TryLoad(out RejoinTicket ticket);
        void Save(RejoinTicket ticket);
        void Clear();
    }
}*/