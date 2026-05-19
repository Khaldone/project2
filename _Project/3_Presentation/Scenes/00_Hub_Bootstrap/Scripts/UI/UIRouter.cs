// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/UI/UIRouter.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;


public class UIRouter : IUIRouter
{
    private readonly IObjectResolver _resolver;
    private readonly IDeviceInfoService _deviceInfo;

    // Tracks active UI prefabs so we can destroy them
    private Dictionary<string, GameObject> _activeMenus = new Dictionary<string, GameObject>();

    private string _currentBaseSpoke = string.Empty;


    public UIRouter(IObjectResolver resolver, IDeviceInfoService deviceInfo)
    {
        _resolver = resolver;
        _deviceInfo = deviceInfo;
    }


    public async Task LoadBaseSpokeAsync(string sceneName)
    {
        // 1. Clean up the old 3D environment if it exists
        if (!string.IsNullOrEmpty(_currentBaseSpoke))
        {
            await SceneManager.UnloadSceneAsync(_currentBaseSpoke);
        }


        // 2. Load the new 3D environment additively (keeps the Hub alive!)
        await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        _currentBaseSpoke = sceneName;
    }


    public async Task OpenMenuAsync(string menuAddressableKey)
    {
        if (_activeMenus.ContainsKey(menuAddressableKey)) return; // Already open


        // Bifurcation: Check if we are on a tablet and need a different prefab
        string finalKey = menuAddressableKey;
        if (_deviceInfo.CurrentFormFactor == DeviceFormFactor.Tablet)
        {
            finalKey = $"{menuAddressableKey}_Tablet";
        }


        // 1. INSTANTIATION: Tell Addressables to spawn the Prefab from disk/web
        GameObject uiPrefab = await Addressables.InstantiateAsync(finalKey).Task;

        // 2. DEPENDENCY INJECTION: Tell VContainer to wire up the newly spawned View
        // to its corresponding pure C# Presenter
        _resolver.InjectGameObject(uiPrefab);


        // 3. Track it for memory management
        _activeMenus.Add(menuAddressableKey, uiPrefab);
    }


    public void CloseMenu(string menuAddressableKey)
    {
        if (_activeMenus.TryGetValue(menuAddressableKey, out GameObject activeMenu))
        {
            // DESTRUCTION: Free up the RAM
            Addressables.ReleaseInstance(activeMenu);
            _activeMenus.Remove(menuAddressableKey);
        }
    }

    public Task CloseMenuAsync(string additiveSceneName)
    {
        throw new System.NotImplementedException();
    }
}
