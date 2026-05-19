// Assets/_Project/2_Infrastructure/Assets/AddressableDeliveryService.cs
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Billiards.CoreDomain.Services;
using Cysharp.Threading.Tasks;

public class AddressableDeliveryService : IAssetDeliveryService
{
    public Task DownloadDependenciesAsync(string addressableKey, Action<float> onProgress)
    {
        throw new NotImplementedException();
    }

    // Called by the AppBootstrapper on game launch
    public async Task InitializeAsync()
    {
        // Pings the remote CDN to see if there is a new content catalog
        // (e.g., you released a new Cue skin over the weekend)
        var initOp = Addressables.InitializeAsync();
        await initOp.Task;


        var checkOp = Addressables.CheckForCatalogUpdates();
        var updates = await checkOp.Task;


        if (updates.Count > 0)
        {
            await Addressables.UpdateCatalogs(updates).Task;
        }
    }


    // Called when the player equips a specific cue
    public async Task<GameObject> InstantiateCueAsync(string cueAddressId, Transform parent)
    {
        // Addressables handles the magic:
        // If it's already on the phone, it loads instantly.
        // If it's on the cloud, it downloads it, caches it to the hard drive, and then loads it.
        var handle = Addressables.InstantiateAsync(cueAddressId, parent);
        await handle.Task;


        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }


        Debug.LogError($"Failed to load asset: {cueAddressId}");
        return null;
    }

    public Task<T> LoadAssetAsync<T>(string addressableKey) where T : UnityEngine.Object
    {
        throw new NotImplementedException();
    }

    public Task<T> LoadAssetAsync_Old<T>(string address) where T : class
    {
        throw new NotImplementedException();
    }

    public void ReleaseAsset(object instantiatedAsset)
    {
        throw new NotImplementedException();
    }

    public void ReleaseAsset(string assetKey)
    {
        throw new NotImplementedException();
    }

    UniTask<T> IAssetDeliveryService.LoadAssetAsync<T>(string assetKey)
    {
        throw new NotImplementedException();
    }
}
