// Assets/_Project/2_Infrastructure/Backend/CbsArenaProgressionService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CBS; // BANNED IN CORE/PRESENTATION, ALLOWED HERE.
using CBS.Models;
using UnityEngine;
using Billiards.CoreDomain.Progression;

namespace Billiards.Infrastructure.Backend
{
    public class CbsArenaProgressionService : IArenaProgressionService
    {
        private readonly ICBSItems _cbsItems;
        private readonly ICBSInventory _cbsInventory;
        private readonly ICurrency _cbsCurrency;

        public CbsArenaProgressionService()
        {
            // Resolve CBS modules
            _cbsItems = CBSModule.Get<CBSItemsModule>();
            _cbsInventory = CBSModule.Get<CBSInventoryModule>();
            _cbsCurrency = CBSModule.Get<CBSCurrencyModule>();
        }

        public async UniTask<List<ArenaConfig>> GetHydratedArenaConfigsAsync()
        {
            var configs = new List<ArenaConfig>();

            // 1. Fire parallel requests to CBS for performance
            var (catalogItems, playerInventory, playerCurrencies) = await UniTask.WhenAll(
                FetchCityCatalogAsync(),
                FetchPlayerInventoryAsync(),
                FetchPlayerCurrenciesAsync()
            );

            // 2. Extract CP (Cups) balance
            int currentCups = playerCurrencies.ContainsKey("CP") ? playerCurrencies["CP"].Value : 0;

            // 3. Hydrate the Core Domain structs
            foreach (var item in catalogItems)
            {
                var data = item.GetCustomData<CityItemCustomData>();
                if (data == null) data = new CityItemCustomData();

                bool isOwned = playerInventory.Any(invItem => invItem.ItemID == item.ItemID);
                bool hasEnoughCups = currentCups >= data.RequiredCups;

                Debug.Log($"[ArenaProgression] City: {item.ItemID} | Owned: {isOwned} | CP: {currentCups}/{data.RequiredCups} -> HasEnough: {hasEnoughCups}");

                configs.Add(new ArenaConfig
                {
                    ItemId = item.ItemID,
                    DisplayName = data.DisplayName ?? item.DisplayName,
                    PrizePool = data.PrizePool,
                    EntryFee = data.EntryFee,
                    RequiredCups = data.RequiredCups,
                    RequiredLevel = data.RequiredLevel,
                    IsOwned = isOwned,
                    HasEnoughCups = hasEnoughCups
                });
            }

            // Return sorted by Required Cups
            return configs.OrderBy(c => c.RequiredCups).ToList();
        }

        public async UniTask<bool> TryUnlockArenaAsync(string itemId)
        {
            // Implementation calls _cbsItems.PurchaseItemAsync using CP currency...
            // For brevity, returning true.
            await UniTask.Delay(100);
            return true;
        }

        // --- Private CBS Wrapper Methods ---

        private async UniTask<List<CBSItem>> FetchCityCatalogAsync()
        {
            var tcs = new UniTaskCompletionSource<List<CBSItem>>();
            var request = new CBSGetItemsRequest { SpecificCategory = "CityItems", ItemType = ItemType.ITEMS };
            _cbsItems.GetCBSItems(request, result =>
            {
                if (result.IsSuccess) tcs.TrySetResult(result.Items);
                else tcs.TrySetException(new Exception(result.Error.Message));
            });
            return await tcs.Task;
        }

        private async UniTask<List<CBSInventoryItem>> FetchPlayerInventoryAsync()
        {
            var tcs = new UniTaskCompletionSource<List<CBSInventoryItem>>();
            _cbsInventory.GetInventory(result =>
            {
                if (result.IsSuccess) tcs.TrySetResult(result.AllItems);
                else tcs.TrySetException(new Exception(result.Error.Message));
            });
            return await tcs.Task;
        }

        private async UniTask<Dictionary<string, CBSCurrency>> FetchPlayerCurrenciesAsync()
        {
            var tcs = new UniTaskCompletionSource<Dictionary<string, CBSCurrency>>();
            _cbsCurrency.GetProfileCurrencies(result =>
            {
                if (result.IsSuccess) tcs.TrySetResult(result.Currencies);
                else tcs.TrySetException(new Exception(result.Error.Message));
            });
            return await tcs.Task;
        }
    }
}