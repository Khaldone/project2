//// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Assets/UnityAddressablesWrapper.cs
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.AddressableAssets;
//using UnityEngine.ResourceManagement.AsyncOperations;
//using Billiards.CoreDomain.Services;
//using Cysharp.Threading.Tasks;
//public class UnityAddressablesWrapper : MonoBehaviour, IAssetDeliveryService
//{
//    // We keep a dictionary of active operations so we can properly release them later
//    private readonly Dictionary<string, AsyncOperationHandle> _activeHandles = new Dictionary<string, AsyncOperationHandle>();

//    public Task DownloadDependenciesAsync(string addressableKey, Action<float> onProgress)
//    {
//        throw new NotImplementedException();
//    }

//    public async Task InitializeAsync()
//    {
//        var handle = Addressables.InitializeAsync();
//        await handle.Task;
//        Debug.Log("AAA Pipeline: Addressables Catalog Initialized.");
//    }

//    public Task<T> LoadAssetAsync<T>(string addressableKey) where T : UnityEngine.Object
//    {
//        throw new NotImplementedException();
//    }

//    public async Task<T> LoadAssetAsync_Old<T>(string address) where T : class
//    {
//        // If we already loaded it, return it instantly
//        if (_activeHandles.TryGetValue(address, out AsyncOperationHandle existingHandle))
//        {
//            return existingHandle.Result as T;
//        }

//        try
//        {
//            // Ask Addressables to fetch it (from local disk or a remote CDN)
//            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);

//            _activeHandles[address] = handle; // Store the handle for memory management

//            await handle.Task;


//            if (handle.Status == AsyncOperationStatus.Succeeded)
//            {
//                return handle.Result;
//            }
//            else
//            {
//                Debug.LogError($"Failed to load asset at address: {address}");
//                return null;
//            }
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError($"Addressables Exception on {address}: {ex.Message}");
//            return null;
//        }
//    }

//    public void ReleaseAsset(string address)
//    {
//        if (_activeHandles.TryGetValue(address, out AsyncOperationHandle handle))
//        {
//            Addressables.Release(handle);
//            _activeHandles.Remove(address);
//            Debug.Log($"AAA Pipeline: Released asset {address} from RAM.");
//        }
//    }

//    public void ReleaseAsset(object instantiatedAsset)
//    {
//        throw new NotImplementedException();
//    }

//    UniTask<T> IAssetDeliveryService.LoadAssetAsync<T>(string assetKey)
//    {
//        throw new NotImplementedException();
//    }
//}