// Assets/_Project/1_CoreDomain/Progression/IPlayerInventoryService.cs
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Billiards.CoreDomain.Progression
{
    public interface IPlayerInventoryService
    {
        UniTask<List<string>> GetOwnedItemIdsAsync();
        UniTask<List<string>> GetEquippedItemIdsAsync();
        UniTask<bool> EquipItemAsync(string itemId);
        UniTask<bool> UnequipItemAsync(string itemId);
        UniTask<bool> PurchaseItemWithVirtualCurrencyAsync(string itemId, string currencyCode, int price);
    }
}
