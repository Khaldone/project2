// Assets/_Project/1_CoreDomain/Services/IAssetDeliveryService.cs
using Cysharp.Threading.Tasks;

namespace Billiards.CoreDomain.Services
{
    /// <summary>
    /// Decoupled core interface mapping dynamic, asynchronous mobile asset loading sequences.
    /// </summary>
    public interface IAssetDeliveryService
    {
        /// <summary>
        /// Asynchronously streams a typed asset from storage boundaries using its registration key.
        /// </summary>
        UniTask<T> LoadAssetAsync<T>(string assetKey) where T : UnityEngine.Object;

        /// <summary>
        /// Explicitly unloads an asset memory layout to safeguard the mobile RAM budget.
        /// </summary>
        void ReleaseAsset(string assetKey);
    }
}