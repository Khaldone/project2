// Assets/_Project/2_Infrastructure/Assets/UnityAddressablesWrapper.cs
using System;
using System.Collections.Generic;
using Billiards.CoreDomain.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Billiards.Infrastructure.Assets
{
    /// <summary>
    /// Production implementation wrapping the Unity Addressables framework behind core interfaces.
    /// </summary>
    public sealed class UnityAddressablesWrapper : IAssetDeliveryService, IDisposable
    {
        // Tracks active handles to protect mobile memory layers from leaks
        private readonly Dictionary<string, AsyncOperationHandle> _trackedHandlesMap = new();

        public async UniTask<T> LoadAssetAsync<T>(string assetKey) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetKey))
            {
                throw new ArgumentException("[Infrastructure] Asset stream key cannot be null or empty.");
            }

            // If the asset is already streamed or currently loading, await it to prevent race conditions
            if (_trackedHandlesMap.TryGetValue(assetKey, out var activeHandle))
            {
                if (!activeHandle.IsDone)
                {
                    await UniTask.WaitUntil(() => activeHandle.IsDone);
                }

                if (activeHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    return (T)activeHandle.Result;
                }
                
                throw new Exception($"[Infrastructure] Cached Addressable failed loading asset key: '{assetKey}'");
            }

            // Convert standard Addressables operations to highly optimized UniTasks
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetKey);
            _trackedHandlesMap.Add(assetKey, handle);

            await UniTask.WaitUntil(() => handle.IsDone);

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }

            // Cleanup map tracking state automatically if load queries fail on backend CDNs
            _trackedHandlesMap.Remove(assetKey);
            Addressables.Release(handle);
            throw new Exception($"[Infrastructure] Addressables failed loading asset key: '{assetKey}'");
        }

        public void ReleaseAsset(string assetKey)
        {
            if (_trackedHandlesMap.TryGetValue(assetKey, out var handle))
            {
                _trackedHandlesMap.Remove(assetKey);
                Addressables.Release(handle);
            }
        }

        public void Dispose()
        {
            // Safeguard memory space by flushing out all tracked streams when context drops
            foreach (var handle in _trackedHandlesMap.Values)
            {
                Addressables.Release(handle);
            }
            _trackedHandlesMap.Clear();
        }
    }
}