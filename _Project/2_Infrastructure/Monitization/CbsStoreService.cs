// Assets/_Project/Infrastructure/Monetization/CbsStoreService.cs
using System.Collections.Generic;
using System.Linq;
using CBS;
using CBS.Models;
using Cysharp.Threading.Tasks;
using Billiards.CoreDomain.Monetization;
using Billiards.CoreDomain.Assets;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Billiards.Infrastructure.Monetization
{
    public class CbsStoreService : IStoreDataSource, IStoreAssetProvider
    {
        private ICBSItems ItemModule => CBSModule.Get<CBSItemsModule>();
        private ICurrency CurrencyModule => CBSModule.Get<CBSCurrencyModule>();
        private IProfile ProfileModule { get; set; }
        public async UniTask<T> GetStoreItemIconAsync<T>(string itemId) where T : class
        {
            // 1. Try CBS Items
            var item = ItemModule.AllItems?.FirstOrDefault(x => x.ItemID == itemId);
            if (item != null && item.GetSprite() is T tSprite1) return tSprite1;

            // 2. Try CBS Packs
            var pack = ItemModule.AllPacks?.FirstOrDefault(x => x.ItemID == itemId);
            if (pack != null && pack.GetSprite() is T tSprite2) return tSprite2;

            // 3. Fallback to Addressables
            try
            {
                var asset = await Addressables.LoadAssetAsync<Object>(itemId).Task.AsUniTask();
                if (asset is Sprite s && s is T tSprite3) return tSprite3;
                
                // If the Addressable returned a Texture2D, build a Sprite dynamically
                if (asset is Texture2D t2)
                {
                    var dynamicSprite = Sprite.Create(t2, new Rect(0, 0, t2.width, t2.height), new Vector2(0.5f, 0.5f));
                    if (dynamicSprite is T tSprite4) return tSprite4;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[CbsStoreService] No CBS Icon or Addressable found for {itemId}. Error: {ex.Message}");
            }

            return null;
        }

        public async UniTask<List<StoreProduct>> FetchStoreItemsAsync(string storeId)
        {
            // 1. CBS caches this locally at login, so reading AllItems is instant!
            var items = ItemModule.AllItems;

            if (items == null) return new List<StoreProduct>();

            // 2. Filter by the store ID (which matches the CBS Item Category)
            var filteredItems = items.Where(i => i.Category == storeId).ToList();

            // 3. The Anti-Corruption Step: Map to internal Domain Struct
            var products = filteredItems.Select(cbsItem =>
            {
                ParsePrices(cbsItem.Prices, out string priceStr, out int vcPrice, out string vcCode);

                return new StoreProduct
                {
                    Id = cbsItem.ItemID,
                    Name = cbsItem.DisplayName,
                    LocalizedPrice = priceStr,
                    VirtualCurrencyPrice = vcPrice,
                    VirtualCurrencyCode = vcCode
                };
            }).ToList();

            return await UniTask.FromResult(products);
        }

        public async UniTask<List<StoreProduct>> FetchStorePacksAsync(string categoryId)
        {
            // 1. CBS caches this locally at login, so reading AllPacks is instant!
            var packs = ItemModule.AllPacks;

            if (packs == null) return new List<StoreProduct>();

            // 2. Filter by the category
            var filteredPacks = packs.Where(p => p.Category == categoryId).ToList();

            // 3. Map to internal Domain Struct
            var products = filteredPacks.Select(cbsPack =>
            {
                ParsePrices(cbsPack.Prices, out string priceStr, out int vcPrice, out string vcCode);

                return new StoreProduct
                {
                    Id = cbsPack.ItemID,
                    Name = cbsPack.DisplayName,
                    LocalizedPrice = priceStr,
                    VirtualCurrencyPrice = vcPrice,
                    VirtualCurrencyCode = vcCode
                };
            }).ToList();

            return await UniTask.FromResult(products);
        }

        private void ParsePrices(Dictionary<string, uint> prices, out string priceStr, out int vcPrice, out string vcCode)
        {
            priceStr = "Free";
            vcPrice = 0;
            vcCode = "RM";

            if (prices == null || prices.Count == 0) return;

            // 1. Check dynamic Virtual Currencies
            if (CurrencyModule != null && CurrencyModule.CacheCurrencies != null)
            {
                foreach (var cachedCurrency in CurrencyModule.CacheCurrencies.Keys)
                {
                    if (prices.ContainsKey(cachedCurrency))
                    {
                        vcCode = cachedCurrency;
                        vcPrice = (int)prices[cachedCurrency];
                        priceStr = $"{vcPrice} {vcCode}";
                        return; // Exit early as we found a matching VC
                    }
                }
            }

            // 2. Fallback to Real Money (Fiat)
            if (prices.ContainsKey("RM"))
            {
                vcCode = "RM";
                uint priceInCents = prices["RM"];
                priceStr = $"${(priceInCents / 100f):0.00}";
            }
        }

        public async UniTask ForceRefreshAsync()
        {
            var tcs = new UniTaskCompletionSource<bool>();
            ItemModule.FetchAll(result => tcs.TrySetResult(result.IsSuccess));
            await tcs.Task;
        }

        public async UniTask<List<string>> GetAllStoreIdsAsync()
        {
            return await UniTask.FromResult(ItemModule.ItemCategories.ToList());
        }

        public async UniTask<List<string>> GetAllRealMoneyProductIdsAsync()
        {
            var rmProductIds = new List<string>();

            if (ItemModule.AllPacks != null)
            {
                rmProductIds.AddRange(ItemModule.AllPacks
                    .Where(p => p.Prices != null && p.Prices.ContainsKey("RM"))
                    .Select(p => p.ItemID));
            }

            if (ItemModule.AllItems != null)
            {
                rmProductIds.AddRange(ItemModule.AllItems
                    .Where(i => i.Prices != null && i.Prices.ContainsKey("RM"))
                    .Select(i => i.ItemID));
            }

            return await UniTask.FromResult(rmProductIds);
        }
    }
}