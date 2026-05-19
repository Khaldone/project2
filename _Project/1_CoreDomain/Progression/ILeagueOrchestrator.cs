// 3. The Orchestrator Contract (What the UI will actually talk to)
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
public interface ILeagueOrchestrator
{
    event Action<List<LeaguePlayer>> OnLeaderboardUpdated;

    // The Orchestrator caches data so the UI doesn't spam the backend
    IReadOnlyList<LeaguePlayer> CurrentLeaderboard { get; }

    Task RefreshLeaderboardAsync();
    string CalculateLeagueTier(int trophies);
}