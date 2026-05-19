// Assets/_Project/1_CoreDomain/Progression/ArenaConfig.cs
namespace Billiards.CoreDomain.Progression
{
    /// <summary>
    /// Pure data struct representing a playable city.
    /// Values are hydrated from PlayFab Catalog Custom Data.
    /// </summary>
    public struct ArenaConfig
    {
        public string ItemId;        // e.g., "City_Berlin"
        public string DisplayName;   // e.g., "Berlin Underground"
        public int PrizePool;
        public int EntryFee;
        public int RequiredCups;     // 'CP'
        public int RequiredLevel;

        // Progression State
        public bool IsOwned;         // True if the item is in the player's inventory
        public bool HasEnoughCups;   // True if current CP >= RequiredCups

        public readonly bool IsPlayable => IsOwned && HasEnoughCups;
    }
}