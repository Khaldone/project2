// Assets/_Project/Infrastructure/Monetization/PlayFabStoreSyncService.cs
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using Cysharp.Threading.Tasks;
using System.Linq;
using Billiards.CoreDomain.Monetization;

namespace Billiards.Infrastructure.Monetization
{
    public class PlayFabStoreSyncService
    {
        private readonly StoreCache _cache;

        public PlayFabStoreSyncService(StoreCache cache)
        {
            _cache = cache;
        }

        public async UniTask SyncAllStoresAsync(List<string> storeIds)
        {
            var tasks = storeIds.Select(id => FetchAndCacheStore(id)).ToList();
            await UniTask.WhenAll(tasks);
        }

        private async UniTask FetchAndCacheStore(string storeId)
        {
            var tcs = new UniTaskCompletionSource<List<StoreProduct>>();

            PlayFabClientAPI.GetStoreItems(new GetStoreItemsRequest { StoreId = storeId },
                result => {
                    var products = result.Store.Select(item => {

                        // 1. Extract the Real Money (RM) price from PlayFab (stored in cents)
                        uint priceInCents = 0;
                        if (item.VirtualCurrencyPrices != null && item.VirtualCurrencyPrices.ContainsKey("RM"))
                        {
                            priceInCents = item.VirtualCurrencyPrices["RM"];
                        }

                        // 2. Format it into a standard currency string (e.g., $0.99)
                        string playfabPriceString = priceInCents > 0 ? $"${(priceInCents / 100f):0.00}" : "Free";

                        return new StoreProduct
                        {
                            Id = item.ItemId,
                            Name = item.ItemId, // In a real app, you might query the Catalog for the actual Display Name
                            LocalizedPrice = playfabPriceString // Store the PlayFab price immediately!
                        };
                    }).ToList();

                    tcs.TrySetResult(products);
                },
                error => {
                    UnityEngine.Debug.LogError($"[PlayFab] Store sync failed: {error.ErrorMessage}");
                    tcs.TrySetResult(new List<StoreProduct>());
                }
            );

            var productList = await tcs.Task;
            _cache.UpdateStore(storeId, productList);
        }
    }
}