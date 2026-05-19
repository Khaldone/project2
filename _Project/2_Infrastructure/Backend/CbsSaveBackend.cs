// Assets/_Project/2_Infrastructure/Backend/CbsSaveBackend.cs
using Billiards.CoreDomain.Progression;
using CBS;
using CBS.Models;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace Billiards.Infrastructure.Backend
{
    public class CbsSaveBackend : ICloudSaveBackend
    {
        private ICurrency CurrencyModule => CBSModule.Get<CBSCurrencyModule>();
        private ICBSInventory InventoryModule => CBSModule.Get<CBSInventoryModule>();
        private ICBSItems ItemModule => CBSModule.Get<CBSItemsModule>();
        
        // The standard PlayFab/CBS currency code for Coins. Adjust if needed.
        private const string COIN_CURRENCY_CODE = "GD"; 

        public async UniTask<PlayerProfileSave> LoadProfileAsync()
        {
            var tcs = new UniTaskCompletionSource<PlayerProfileSave>();
            var profile = new PlayerProfileSave();

            // 1. Fetch Currencies from CBS Cache synchronously
            if (CurrencyModule != null && CurrencyModule.CacheCurrencies != null)
            {
                if (CurrencyModule.CacheCurrencies.ContainsKey(COIN_CURRENCY_CODE))
                {
                    // CacheCurrencies value type depends on CBS version, usually it's CBSCurrency or decimal/int.
                    // Assuming .Value exists or it can be casted directly based on earlier code.
                    profile.Coins = (int)CurrencyModule.CacheCurrencies[COIN_CURRENCY_CODE].Value; 
                }
            }

            // 2. Fetch Inventory from CBS Cache
            InventoryModule.GetInventoryFromCache(inventoryResult =>
            {
                if (inventoryResult.IsSuccess && inventoryResult.AllItems != null && inventoryResult.AllItems.Count > 0)
                {
                    ProcessInventoryResult(inventoryResult, profile);
                    tcs.TrySetResult(profile);
                }
                else
                {
                    // Fallback: If cache is empty, fetch from network
                    InventoryModule.GetInventory(netInventoryResult =>
                    {
                        if (netInventoryResult.IsSuccess && netInventoryResult.AllItems != null)
                        {
                            ProcessInventoryResult(netInventoryResult, profile);
                        }
                        tcs.TrySetResult(profile);
                    });
                }
            });

            return await tcs.Task;
        }

        private void ProcessInventoryResult(CBSGetInventoryResult inventoryResult, PlayerProfileSave profile)
        {
            var cues = inventoryResult.AllItems
                .Select(x => x.ItemID)
                .Distinct()
                .ToList();
            
            if (cues.Count > 0)
            {
                profile.UnlockedCues = cues;
            }

            // Get the first equipped item (assuming it's the cue)
            if (inventoryResult.EquippedItems != null)
            {
                var equipped = inventoryResult.EquippedItems.FirstOrDefault();
                if (equipped != null)
                {
                    profile.EquippedCueId = equipped.ItemID;
                }
            }
        }

        public async UniTask<bool> SaveProfileAsync(PlayerProfileSave profile)
        {
            var tcs = new UniTaskCompletionSource<bool>();

            // 1. Sync Coins (Differential)
            if (CurrencyModule != null && CurrencyModule.CacheCurrencies != null)
            {
                int currentCoins = CurrencyModule.CacheCurrencies.ContainsKey(COIN_CURRENCY_CODE) ? (int)CurrencyModule.CacheCurrencies[COIN_CURRENCY_CODE].Value : 0;
                int diff = profile.Coins - currentCoins;
                
                if (diff > 0)
                {
                    CurrencyModule.AddCurrencyToProfile(COIN_CURRENCY_CODE, diff, _ => {});
                }
                else if (diff < 0)
                {
                    CurrencyModule.SubtractCurrencyFromProfile(COIN_CURRENCY_CODE, -diff, _ => {});
                }
            }

            // 2. Sync Inventory (Grant missing cues and equip)
            InventoryModule.GetInventoryFromCache(invRes =>
            {
                if (invRes.IsSuccess)
                {
                    var currentItemIds = invRes.AllItems.Select(x => x.ItemID).ToList();
                    var missingCues = profile.UnlockedCues.Except(currentItemIds).ToList();

                    if (missingCues.Count > 0)
                    {
                        // Grant any cues unlocked locally that CBS doesn't know about using ItemModule
                        foreach (var cueId in missingCues)
                        {
                            ItemModule.GrantItem(cueId, _ => {});
                        }
                    }
                    
                    // 3. Sync Equipped Cue
                    var currentlyEquipped = invRes.EquippedItems?.FirstOrDefault()?.ItemID;
                    if (profile.EquippedCueId != currentlyEquipped && !string.IsNullOrEmpty(profile.EquippedCueId))
                    {
                        var instanceToEquip = invRes.AllItems.FirstOrDefault(x => x.ItemID == profile.EquippedCueId);
                        if (instanceToEquip != null)
                        {
                            InventoryModule.EquipItem(instanceToEquip.InstanceID, _ => {});
                        }
                    }
                }
            });

            // Return true immediately since we fired off fire-and-forget sync tasks
            tcs.TrySetResult(true);
            return await tcs.Task;
        }
    }
}
