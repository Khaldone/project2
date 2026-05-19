// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/Gameplay/ArenaEnvironmentLoader.cs
using System;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Billiards.CoreDomain.Services;
public class ArenaEnvironmentLoader : IStartable, IDisposable
{
    private readonly IAssetDeliveryService _assetDelivery;
    private readonly IObjectResolver _resolver; // VContainer's instantiation engine

    private string _currentTableAddress;
    private GameObject _spawnedTableInstance;

    public ArenaEnvironmentLoader(IAssetDeliveryService assetDelivery, IObjectResolver resolver)
    {
        _assetDelivery = assetDelivery;
        _resolver = resolver;
    }

    public async void Start()
    {
        // 1. Determine which table to load (perhaps from the Matchmaking Orchestrator)
        _currentTableAddress = "Environments/Tables/NeonCyberpunkTable";

        // 2. Load the Prefab into RAM via the global service
        // (This might take 2 seconds if downloading from AWS)
        GameObject tablePrefab = await _assetDelivery.LoadAssetAsync<GameObject>(_currentTableAddress);

        if (tablePrefab != null)
        {
            // 3. Instantiate the prefab using VContainer!
            // This ensures any scripts on the table get their [Inject] dependencies fulfilled.
            _spawnedTableInstance = _resolver.Instantiate(tablePrefab);

            // Move it to the center of the room
            _spawnedTableInstance.transform.position = Vector3.zero;
        }
    }

    public void Dispose()
    {
        // THE GOLDEN RULE OF ASSET DELIVERY
        // When the Arena scene unloads, we MUST clean up the heavy assets.

        if (_spawnedTableInstance != null)
        {
            UnityEngine.Object.Destroy(_spawnedTableInstance);
        }


        if (!string.IsNullOrEmpty(_currentTableAddress))
        {
            // Tell the Hub to flush the 4K textures from RAM
            _assetDelivery.ReleaseAsset(_currentTableAddress);
        }
    }
}