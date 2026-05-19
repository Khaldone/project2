using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LeagueOrchestrator : ILeagueOrchestrator
{
    private readonly ILeagueBackend _backend;
    private List<LeaguePlayer> _cachedLeaderboard = new List<LeaguePlayer>();


    public event Action<List<LeaguePlayer>> OnLeaderboardUpdated;
    public IReadOnlyList<LeaguePlayer> CurrentLeaderboard => _cachedLeaderboard;

    public LeagueOrchestrator(ILeagueBackend backend)
    {
        _backend = backend;
    }

    public async Task RefreshLeaderboardAsync()
    {
        // 1. Ask the backend for raw data
        var freshData = await _backend.FetchTopPlayersAsync(50);

        // 2. Process it (e.g., assign tiers based on our pure C# rules)
        for (int i = 0; i < freshData.Count; i++)
        {
            var player = freshData[i];
            player.LeagueTier = CalculateLeagueTier(player.Trophies);
            freshData[i] = player; // Update the struct
        }


        // 3. Cache it and notify anyone listening
        _cachedLeaderboard = freshData;
        OnLeaderboardUpdated?.Invoke(_cachedLeaderboard);
    }


    public string CalculateLeagueTier(int trophies)
    {
        if (trophies >= 2000) return "Diamond";
        if (trophies >= 1000) return "Gold";
        if (trophies >= 500) return "Silver";
        return "Bronze";
    }
}