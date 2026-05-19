// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Backend/PlayFabLootboxWrapper.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public interface ILootboxOrchestrator
{
    Task<List<LootboxReward>> TryOpenLootboxAsync(LootboxInstance box);
    Task<bool> SpeedUpLootboxAsync(LootboxInstance box, int premiumCurrencyCost);
}

public class PlayFabLootboxWrapper : MonoBehaviour, ILootboxOrchestrator
{
    public async Task<List<LootboxReward>> TryOpenLootboxAsync(LootboxInstance box)
    {
        // 1. Client-Side Sanity Check
        if (!box.IsReadyToOpen)
        {
            Debug.LogWarning("AAA Pipeline: Box is still locked! Show speed-up prompt.");
            return null; // The Presenter will catch this null and show a popup
        }


        // 2. The Server Call
        // Make the PlayFab API call: UnlockContainerInstanceRequest
        // PlayFab will double-check the server clock. If valid, it consumes the box
        // and returns the 'GrantedItems' array.

        // 3. Map PlayFab's GrantedItems to our pure C# struct
        var rewards = new List<LootboxReward>
        {
            new LootboxReward { ItemId = "coins", Quantity = 500, Rarity = "Common" },
            new LootboxReward { ItemId = "cue_piece_dragon", Quantity = 2, Rarity = "Epic" },
            new LootboxReward { ItemId = "chat_pack_taunts", Quantity = 1, Rarity = "Rare" }
        };


        return rewards; // Hand the list back to the UI
    }

    public async Task<bool> SpeedUpLootboxAsync(LootboxInstance box, int premiumCurrencyCost)
    {
        // Call a PlayFab CloudScript: "SpeedUpLootbox"
        // The server deducts the Gems, changes the UnlockTime to 'Now', and saves.
        return true;
    }
}