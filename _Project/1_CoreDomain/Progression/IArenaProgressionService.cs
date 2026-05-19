// Assets/_Project/1_CoreDomain/Progression/IArenaProgressionService.cs
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Billiards.CoreDomain.Progression
{
    public interface IArenaProgressionService
    {
        /// <summary>
        /// Fetches all city configurations from the catalog and cross-references 
        /// them with the player's inventory and 'CP' currency.
        /// </summary>
        UniTask<List<ArenaConfig>> GetHydratedArenaConfigsAsync();

        /// <summary>
        /// Attempts to purchase the city unlock on the server using 'CP'.
        /// </summary>
        UniTask<bool> TryUnlockArenaAsync(string itemId);
    }
}