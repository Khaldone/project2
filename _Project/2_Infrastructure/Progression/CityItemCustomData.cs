// Assets/_Project/2_Infrastructure/Progression/CityItemCustomData.cs
using CBS;
using CBS.Models;

namespace Billiards.Infrastructure.Backend
{
    /// <summary>
    /// Custom data mapped to CBS Items under the "CityItems" category.
    /// Used by CbsArenaProgressionService to hydrate ArenaConfig structs.
    /// </summary>
    public class CityItemCustomData : CBSItemCustomData
    {
        public string DisplayName;
        public int PrizePool;
        public int EntryFee;
        public int RequiredCups;
        public int RequiredLevel;
    }
}
