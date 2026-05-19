// Assets/_Project/Infrastructure/Monetization/PlayFabStoreService.cs
using System.Collections.Generic;
using System.Linq;
using PlayFab;
using PlayFab.ClientModels;
using Cysharp.Threading.Tasks;
using Billiards.CoreDomain.Monetization;
using UnityEngine;

namespace Billiards.Infrastructure.Monetization
{
    public class PlayFabStoreService : IStoreDataSource
    {
        // 1. Implementation of the existing Fetch method
        public async UniTask<List<StoreProduct>> FetchStoreItemsAsync(string storeId)
        {
            var tcs = new UniTaskCompletionSource<List<StoreProduct>>();

            PlayFabClientAPI.GetStoreItems(new GetStoreItemsRequest { StoreId = storeId },
                result =>
                {
                    var products = result.Store.Select(item => {
                        // Extract RM price if available
                        uint priceInCents = 0;
                        if (item.VirtualCurrencyPrices != null && item.VirtualCurrencyPrices.ContainsKey("RM"))
                        {
                            priceInCents = item.VirtualCurrencyPrices["RM"];
                        }

                        return new StoreProduct
                        {
                            Id = item.ItemId,
                            Name = item.ItemId,
                            LocalizedPrice = priceInCents > 0 ? $"${(priceInCents / 100f):0.00}" : "Loading..."
                        };
                    }).ToList();
                    tcs.TrySetResult(products);
                },
                error => {
                    Debug.LogError($"[PlayFab] Store {storeId} fetch failed: {error.ErrorMessage}");
                    tcs.TrySetResult(new List<StoreProduct>());
                }
            );

            return await tcs.Task;
        }

        public UniTask<List<StoreProduct>> FetchStorePacksAsync(string categoryId)
        {
            throw new System.NotImplementedException();
        }

        public UniTask ForceRefreshAsync()
        {
            throw new System.NotImplementedException();
        }

        public UniTask<List<string>> GetAllRealMoneyProductIdsAsync()
        {
            throw new System.NotImplementedException();
        }

        // 2. THE NEW INTERFACE METHOD: Discovery
        // For pure PlayFab Client API, we often maintain a list of active Store IDs.
        public async UniTask<List<string>> GetAllStoreIdsAsync()
        {
            // Since standard PlayFab Client API doesn't have "GetStoreList",
            // we return the IDs you've defined in your dashboard.
            // This allows the rest of your app to stay dynamic!
            var activeStores = new List<string>
            {
                "Main_Store",
                "Cue_Store",
                "Coin_Store"
            };

            return await UniTask.FromResult(activeStores);
        }
    }
}