// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/Gameplay/CueStickSpawner.cs
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Billiards.CoreDomain.Services;
public class CueStickSpawner : IStartable, IDisposable
{
    private readonly IEquipmentService _equipment;
    private readonly IAssetDeliveryService _assetDelivery;
    private readonly IObjectResolver _resolver; // VContainer


    private GameObject _spawnedCueInstance;


    public CueStickSpawner(IEquipmentService equipment, IAssetDeliveryService assetDelivery, IObjectResolver resolver)
    {
        _equipment = equipment;
        _assetDelivery = assetDelivery;
        _resolver = resolver;
    }


    public async void Start()
    {
        // 1. Get the String ID of what the player wants to hold
        string cueToLoad = _equipment.EquippedCueId; // e.g., "cue_neon_strike"


        // 2. Ask Addressables to pull the Prefab from the CDN or local cache
        GameObject cuePrefab = await _assetDelivery.LoadAssetAsync<GameObject>(cueToLoad);


        if (cuePrefab != null)
        {
            // 3. Instantiate via VContainer so the cue stick's scripts get injected
            _spawnedCueInstance = _resolver.Instantiate(cuePrefab);

            // 4. Attach it to the camera or the player avatar
            // _spawnedCueInstance.transform.SetParent(playerHandTransform);
        }
    }


    public void Dispose()
    {
        // Memory Management: When the match ends, destroy the cue
        // and tell Addressables to clear the 4K textures from RAM.
        if (_spawnedCueInstance != null)
        {
            UnityEngine.Object.Destroy(_spawnedCueInstance);
        }

        _assetDelivery.ReleaseAsset(_equipment.EquippedCueId);
    }
}
