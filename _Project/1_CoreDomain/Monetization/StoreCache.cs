// Assets/_Project/CoreDomain/Monetization/StoreCache.cs
using System.Collections.Generic;

namespace Billiards.CoreDomain.Monetization
{
    public class StoreCache
    {
        // Key: StoreID (e.g., "Main_Store", "Limited_Time_Store")
        // Value: List of products in that specific store
        private readonly Dictionary<string, List<StoreProduct>> _cachedStores = new();

        public void UpdateStore(string storeId, List<StoreProduct> products)
        {
            _cachedStores[storeId] = products;
        }

        public List<StoreProduct> GetStore(string storeId)
        {
            return _cachedStores.TryGetValue(storeId, out var products) ? products : new List<StoreProduct>();
        }

        public bool IsPopulated => _cachedStores.Count > 0;
    }
}