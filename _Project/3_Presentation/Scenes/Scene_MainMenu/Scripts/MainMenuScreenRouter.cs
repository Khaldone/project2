// Assets/_Project/3_Presentation/Scene_MainMenu/Scripts/MainMenuScreenRouter.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

namespace Billiards.Presentation.MainMenu
{
    public class MainMenuScreenRouter : MonoBehaviour
    {
        [SerializeField] private Transform _uiRoot;

        [Header("Automatic Startup Scenes")]
        [SerializeField] private List<AssetReferenceGameObject> _autoLoadScreens;

        private IObjectResolver _container;

        [Inject]
        public void Construct(IObjectResolver container)
        {
            _container = container;
        }

        // This is the "Auto-Run" method called by the LifetimeScope
        public async Task InitializeMenuAsync()
        {
            var tasks = new List<Task<GameObject>>();

            foreach (var @ref in _autoLoadScreens)
            {
                tasks.Add(LoadAndInject(@ref));
            }

            // Wait for ALL additive scenes to finish loading in parallel
            await Task.WhenAll(tasks);
            
            Debug.Log("[Router] All additive UI scenes composed and injected.");
        }

        private async Task<GameObject> LoadAndInject(AssetReferenceGameObject @ref)
        {
            var handle = Addressables.InstantiateAsync(@ref, _uiRoot);
            GameObject go = await handle.Task;

            // Immediately wire up the dependencies (Photon, PlayFab, etc.)
            _container.InjectGameObject(go);
            
            // Keep them hidden initially if needed, or leave them active 
            // if they are part of the main HUD
            return go;
        }
    }
}