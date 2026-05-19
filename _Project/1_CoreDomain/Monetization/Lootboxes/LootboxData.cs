// Assets/_Project/CoreDomain/Monetization/Lootboxes/LootboxData.cs
using System;

public struct LootboxInstance
{
    public string InstanceId;     // PlayFab's unique ID for this specific box
    public string CatalogItemId;  // e.g., "box_sydney_victory"
    public DateTime UnlockTime;   // When this box is legally allowed to open

    public bool IsReadyToOpen => DateTime.UtcNow >= UnlockTime;
    public TimeSpan TimeRemaining => IsReadyToOpen ? TimeSpan.Zero : UnlockTime - DateTime.UtcNow;
}


public struct LootboxReward
{
    public string ItemId;
    public int Quantity;
    public string Rarity; // "Common", "Epic", "Legendary"
}
