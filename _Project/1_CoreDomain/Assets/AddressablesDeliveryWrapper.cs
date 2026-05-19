// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Assets/AddressablesDeliveryWrapper.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Billiards.CoreDomain.Services;
using Cysharp.Threading.Tasks;

public class AddressablesDeliveryWrapper : MonoBehaviour, IAssetDeliveryService
{
    // We track every instantiated asset so we can properly destroy it and free up RAM
    private Dictionary<object, AsyncOperationHandle> _trackedAssets = new Dictionary<object, AsyncOperationHandle>();


    public async Task<T> LoadAssetAsync<T>(string addressableKey) where T : Object
    {
        // Addressables handles checking if it's already downloaded. If not, it fetches it from the CDN.
        //AsyncOperationHandle<T> handle = Addressables.InstantiateAsync(addressableKey);

        //await handle.Task;


        //if (handle.Status == AsyncOperationStatus.Succeeded)
        //{
        //    _trackedAssets.Add(handle.Result, handle);
        //    return handle.Result;
        //}
        //else
        //{
        //    Debug.LogError($"AAA Pipeline: Failed to load asset {addressableKey}");
        //    return null;
        //}

        return null; // To fix the compilation errors.
    }


    public void ReleaseAsset(object instantiatedAsset)
    {
        if (_trackedAssets.TryGetValue(instantiatedAsset, out AsyncOperationHandle handle))
        {
            // This destroys the GameObject AND unloads the mesh/textures from memory
            Addressables.Release(handle);
            _trackedAssets.Remove(instantiatedAsset);
        }
    }


    public async Task DownloadDependenciesAsync(string addressableKey, System.Action<float> onProgress)
    {
        var downloadHandle = Addressables.DownloadDependenciesAsync(addressableKey);


        while (!downloadHandle.IsDone)
        {
            onProgress?.Invoke(downloadHandle.PercentComplete);
            await Task.Yield();
        }


        Addressables.Release(downloadHandle); // Clean up the handle
    }

    public Task InitializeAsync()
    {
        throw new System.NotImplementedException();
    }

    public Task<T> LoadAssetAsync_Old<T>(string address) where T : class
    {
        throw new System.NotImplementedException();
    }

    UniTask<T> IAssetDeliveryService.LoadAssetAsync<T>(string assetKey)
    {
        throw new System.NotImplementedException();
    }

    public void ReleaseAsset(string assetKey)
    {
        throw new System.NotImplementedException();
    }
}