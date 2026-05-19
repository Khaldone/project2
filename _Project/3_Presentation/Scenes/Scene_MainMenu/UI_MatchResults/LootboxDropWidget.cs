// Attached to: Lootbox_Drop_Anchor
using UnityEngine;
using System;
using Billiards.CoreDomain.Services;

public class LootboxDropWidget : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Animator _dropAnimator;

    // We would use VContainer property injection or pass this down from the View
    private IAssetDeliveryService _assetDelivery;

    public async void PlayDropAnimation(LootboxInstance box, Action onComplete)
    {
        gameObject.SetActive(true);


        // Dynamically load the correct 3D visual for the box they won
        GameObject boxVisual = await _assetDelivery.LoadAssetAsync<GameObject>(box.CatalogItemId);
        boxVisual.transform.SetParent(_spawnPoint, false);


        // Trigger the animation that drops it from the top of the screen
        _dropAnimator.SetTrigger("DropIn");


        // Wait for the animation to finish (using a hardcoded delay or Animation Event)
        Invoke(nameof(FinishDrop), 2.0f);

        void FinishDrop() { onComplete?.Invoke(); }
    }
}