using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;


namespace Billiards.Presentation.MainMenu
{
    /// <summary>
    /// This service manages the loading and unloading of UI scenes using Unity Addressables.
    /// It ensures that any loaded Additive Scene automatically inherits the dependencies 
    /// (like Authentication or PlayerData) from the Main Menu.
    /// </summary>
    public class UIAddressablesLoader
    {
        // Reference to the Main Menu's LifetimeScope, used to parent new scenes.
        private readonly LifetimeScope _mainMenuScope;

        public UIAddressablesLoader(LifetimeScope mainMenuScope)
        {
            _mainMenuScope = mainMenuScope;
        }

        /// <summary>
        /// Loads a UI Scene additively from Addressables and parents its DI Container to the Main Menu.
        /// </summary>
        /// <param name="addressableKey">The 'Address' string of the scene in the Addressables Group.</param>
        /// <returns>A SceneInstance handle, required for unloading the scene later.</returns>
        public async UniTask<SceneInstance> LoadUISceneAdditiveAsync(string addressableKey)
        {

            // 1. Manually set parent for the static queue

            using (LifetimeScope.EnqueueParent(_mainMenuScope))
            {


                // 2. Start the asynchronous load
                AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(addressableKey, UnityEngine.SceneManagement.LoadSceneMode.Additive);

                // 3. Await the void task just to pause execution until loading finishes
                await handle.ToUniTask();

                // 4. Extract the object directly from the completed handle
                SceneInstance sceneInstance = handle.Result;

                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

                return sceneInstance;
            }
        }

        /// <summary>
        /// Properly unloads a previously loaded UI scene and releases its memory.
        /// </summary>
        /// <param name="sceneInstance">The handle returned by LoadUISceneAdditiveAsync.</param>
        public async UniTask UnloadUISceneAsync(SceneInstance sceneInstance)
        {
            // Safety check: Don't try to unload an invalid or empty scene.
            if (!sceneInstance.Scene.IsValid())
            {
                Debug.LogWarning("[UI Loader] Attempted to unload an invalid SceneInstance.");
                return;
            }

            Debug.Log($"[UI Loader] Unloading scene: {sceneInstance.Scene.name}");

            // Unload the scene and wait for completion to ensure memory is freed.
            var handle = Addressables.UnloadSceneAsync(sceneInstance);
            await handle.Task.AsUniTask();

            Debug.Log("[UI Loader] Unload complete.");
        }
    }
}