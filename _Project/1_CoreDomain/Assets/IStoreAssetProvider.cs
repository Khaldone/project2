// Assets/_Project/1_CoreDomain/Assets/IStoreAssetProvider.cs
using Cysharp.Threading.Tasks;

namespace Billiards.CoreDomain.Assets
{
    public interface IStoreAssetProvider
    {
        /// <summary>
        /// Fetches the icon for a store item dynamically. 
        /// Uses generic <T> to ensure pure C# Core Domain doesn't hard-couple to UnityEngine.Sprite.
        /// </summary>
        UniTask<T> GetStoreItemIconAsync<T>(string itemId) where T : class;
    }
}
