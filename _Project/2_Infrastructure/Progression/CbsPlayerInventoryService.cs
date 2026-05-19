// Assets/_Project/2_Infrastructure/Progression/CbsPlayerInventoryService.cs
using System.Collections.Generic;
using System.Linq;
using CBS;
using CBS.Models;
using Cysharp.Threading.Tasks;
using Billiards.CoreDomain.Progression;

namespace Billiards.Infrastructure.Progression
{
    public class CbsPlayerInventoryService : IPlayerInventoryService
    {
        private ICBSInventory InventoryModule => CBSModule.Get<CBSInventoryModule>();
        private ICBSItems ItemsModule => CBSModule.Get<CBSItemsModule>();

        public async UniTask<List<string>> GetOwnedItemIdsAsync()
        {
            var tcs = new UniTaskCompletionSource<List<string>>();
            
            InventoryModule.GetInventoryFromCache(result =>
            {
                if (result.IsSuccess)
                {
                    var ids = result.AllItems.Select(x => x.ItemID).ToList();
                    tcs.TrySetResult(ids);
                }
                else
                {
                    UnityEngine.Debug.LogError($"[CBS Inventory] Failed to get owned items: {result.Error.Message}");
                    tcs.TrySetResult(new List<string>());
                }
            });

            return await tcs.Task;
        }

        public async UniTask<List<string>> GetEquippedItemIdsAsync()
        {
            var tcs = new UniTaskCompletionSource<List<string>>();
            
            InventoryModule.GetInventoryFromCache(result =>
            {
                if (result.IsSuccess)
                {
                    var ids = result.EquippedItems.Select(x => x.ItemID).ToList();
                    tcs.TrySetResult(ids);
                }
                else
                {
                    UnityEngine.Debug.LogError($"[CBS Inventory] Failed to get equipped items: {result.Error.Message}");
                    tcs.TrySetResult(new List<string>());
                }
            });

            return await tcs.Task;
        }

        public async UniTask<bool> EquipItemAsync(string itemId)
        {
            var tcs = new UniTaskCompletionSource<bool>();
            
            InventoryModule.GetInventoryFromCache(result =>
            {
                if (result.IsSuccess)
                {
                    var instance = result.AllItems.FirstOrDefault(x => x.ItemID == itemId);
                    if (instance != null)
                    {
                        InventoryModule.EquipItem(instance.InstanceID, equipResult =>
                        {
                            tcs.TrySetResult(equipResult.IsSuccess);
                        });
                    }
                    else
                    {
                        UnityEngine.Debug.LogError($"[CBS Inventory] Cannot equip {itemId}, not found in inventory.");
                        tcs.TrySetResult(false);
                    }
                }
                else
                {
                    tcs.TrySetResult(false);
                }
            });

            return await tcs.Task;
        }

        public async UniTask<bool> UnequipItemAsync(string itemId)
        {
            var tcs = new UniTaskCompletionSource<bool>();
            
            InventoryModule.GetInventoryFromCache(result =>
            {
                if (result.IsSuccess)
                {
                    var instance = result.AllItems.FirstOrDefault(x => x.ItemID == itemId);
                    if (instance != null)
                    {
                        InventoryModule.UnEquipItem(instance.InstanceID, unequipResult =>
                        {
                            tcs.TrySetResult(unequipResult.IsSuccess);
                        });
                    }
                    else
                    {
                        tcs.TrySetResult(false);
                    }
                }
                else
                {
                    tcs.TrySetResult(false);
                }
            });

            return await tcs.Task;
        }

        public async UniTask<bool> PurchaseItemWithVirtualCurrencyAsync(string itemId, string currencyCode, int price)
        {
            var tcs = new UniTaskCompletionSource<bool>();

            ItemsModule.PurchaseCBSItemWithCurrency(itemId, currencyCode, price, result =>
            {
                if (result.IsSuccess)
                {
                    tcs.TrySetResult(true);
                }
                else
                {
                    UnityEngine.Debug.LogError($"[CBS Store] Purchase failed: {result.Error.Message}");
                    tcs.TrySetResult(false);
                }
            });

            return await tcs.Task;
        }
    }
}
