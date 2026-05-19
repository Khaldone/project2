// Assets/_Project/CoreDomain/Monetization/IStoreDataSource.cs
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Billiards.CoreDomain.Monetization
{
    public interface IStoreDataSource
    {
        // Fetches products from a specific PlayFab Store ID
        UniTask<List<StoreProduct>> FetchStoreItemsAsync(string storeId);
        
        // Fetches packs from a specific CBS Pack Category
        UniTask<List<StoreProduct>> FetchStorePacksAsync(string categoryId);
        
        UniTask<List<string>> GetAllStoreIdsAsync();
        UniTask<List<string>> GetAllRealMoneyProductIdsAsync();
        UniTask ForceRefreshAsync();
    }
}