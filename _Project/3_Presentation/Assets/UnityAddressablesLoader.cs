// Assets/Scripts/Presentation/Assets/UnityAddressablesLoader.cs
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


public class UnityAddressablesLoader : MonoBehaviour, IAssetLoader
{
    public async Task<object> LoadAndInstantiateAsync(string address)
    {
        // Ask Addressables to find the asset (either locally or on the CDN) and spawn it
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address);

        await handle.Task;


        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result; // Return the spawned GameObject to the C# domain
        }
        else
        {
            Debug.LogError($"Failed to load Addressable: {address}");
            return null;
        }
    }


    public void Release(object instance)
    {
        if (instance is GameObject go)
        {
            // This destroys the object AND decrements the memory reference count,
            // allowing Unity to unload the texture from RAM if nothing else is using it.
            Addressables.ReleaseInstance(go);
        }
    }
}
