// Assets/_Project/2_Infrastructure/Backend/BotProfileService.cs
using System;
using System.Collections.Generic;
using System.Linq;


public interface IBotProfileService
{
    // The Presenter calls this when Photon fails to find a real human
    BotProfile GetBelievableOpponent(int localPlayerTrophies, List<string> recentlyPlayedBotIds);
}


public class BotProfileService : IBotProfileService
{
    // Loaded from PlayFab JSON on boot
    private List<BotProfile> _masterBotDatabase = new List<BotProfile>();


    public BotProfile GetBelievableOpponent(int localPlayerTrophies, List<string> recentlyPlayedBotIds)
    {
        // 1. FILTER: Find bots within a believable Trophy range (e.g., +/- 10% of the player's trophies)
        int minTrophies = (int)(localPlayerTrophies * 0.9f);
        int maxTrophies = (int)(localPlayerTrophies * 1.1f);


        var validBots = _masterBotDatabase
            .Where(b => b.SimulatedTrophies >= minTrophies && b.SimulatedTrophies <= maxTrophies)
            .Where(b => !recentlyPlayedBotIds.Contains(b.Id)) // 2. FILTER: Don't play the same bot twice!
            .ToList();


        // 3. FALLBACK: If the player is rated so high we have no bots, widen the search
        if (validBots.Count == 0)
        {
            validBots = _masterBotDatabase.OrderByDescending(b => b.SimulatedTrophies).Take(50).ToList();
        }


        // 4. RANDOMIZE: Pick a random bot from the valid pool to ensure variety
        Random rng = new Random();
        return validBots[rng.Next(validBots.Count)];
    }
}