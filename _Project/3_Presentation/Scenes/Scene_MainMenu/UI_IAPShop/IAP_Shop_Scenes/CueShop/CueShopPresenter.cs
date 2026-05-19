// Assets/_Project/3_Presentation/Scene_CueShop/Scripts/CueShopPresenter.cs
using System;
using Billiards.CoreDomain.Monetization;
using UnityEngine;
using VContainer.Unity;
namespace Billiards.Presentation.Shop
{
    public class CueShopPresenter : IInitializable, IDisposable
    {
        private readonly Cue_ShopScreen _view;
        private readonly IStoreDataSource _storeDataSource;
        private readonly Billiards.CoreDomain.Progression.IPlayerInventoryService _inventoryService;
        private readonly Billiards.CoreDomain.Assets.IStoreAssetProvider _assetProvider;
        
        private string _currentStoreId = "cue_standard"; // Keep track of current tab for refreshing

        public CueShopPresenter(
            Cue_ShopScreen view, 
            IStoreDataSource storeDataSource, 
            Billiards.CoreDomain.Progression.IPlayerInventoryService inventoryService,
            Billiards.CoreDomain.Assets.IStoreAssetProvider assetProvider)
        {
            _view = view;
            _storeDataSource = storeDataSource;
            _inventoryService = inventoryService;
            _assetProvider = assetProvider;

            // Wire up the UI events
            _view.OnStandardCuesClicked += HandleStandardCues;
            _view.OnRareCuesClicked += HandleRareCues;
            _view.OnLegendaryCuesClicked += HandleLegendaryCues;

            // Wire up the Purchase/Equip events
            _view.OnPurchaseCue += HandlePurchaseCue;
            _view.OnEquipCue += HandleEquipCue;
            _view.OnUnequipCue += HandleUnequipCue;

        }

        // We assume "cue_standard", "cue_rare", etc., are the Store IDs (or CBS Categories) you setup in PlayFab/CBS
        private void HandleStandardCues() => PushDataToView("cue_standard");
        private void HandleRareCues() => PushDataToView("cue_rare");
        private void HandleLegendaryCues() => PushDataToView("cue_legendary");

        private async void HandlePurchaseCue(string productId)
        {
            // First, find the product so we know the price and currency code
            var products = await _storeDataSource.FetchStoreItemsAsync(_currentStoreId);
            var product = products.Find(p => p.Id == productId);

            // Using GD as the default fallback
            string currencyCode = string.IsNullOrEmpty(product.VirtualCurrencyCode) ? "GD" : product.VirtualCurrencyCode;
            int price = product.VirtualCurrencyPrice;

            bool success = await _inventoryService.PurchaseItemWithVirtualCurrencyAsync(productId, currencyCode, price);
            if (success)
            {
                // Refresh the UI to show it's now owned
                PushDataToView(_currentStoreId);
            }
        }

        private async void HandleEquipCue(string productId)
        {
            bool success = await _inventoryService.EquipItemAsync(productId);
            if (success) PushDataToView(_currentStoreId);
        }

        private async void HandleUnequipCue(string productId)
        {
            bool success = await _inventoryService.UnequipItemAsync(productId);
            if (success) PushDataToView(_currentStoreId);
        }

        private async void PushDataToView(string storeId)
        {
            _currentStoreId = storeId;
            
            // Fetch directly from the data source and inventory cache (instant RAM lookup)
            var products = await _storeDataSource.FetchStoreItemsAsync(storeId);
            var ownedIds = await _inventoryService.GetOwnedItemIdsAsync();
            var equippedIds = await _inventoryService.GetEquippedItemIdsAsync();

            if (products != null && products.Count > 0)
            {
                await _view.DisplayProductsAsync(products, ownedIds, equippedIds, _assetProvider);
            }
            else
            {
                Debug.LogWarning($"[CueShop] No items found in CBS Category: '{storeId}'. Make sure you assigned items to this Category in the CBS Dashboard!");
                
                // Even if empty, pass it so the View clears out the old buttons
                await _view.DisplayProductsAsync(new System.Collections.Generic.List<StoreProduct>(), ownedIds, equippedIds, _assetProvider);
            }
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.OnStandardCuesClicked -= HandleStandardCues;
                _view.OnRareCuesClicked -= HandleRareCues;
                _view.OnLegendaryCuesClicked -= HandleLegendaryCues;

                _view.OnPurchaseCue -= HandlePurchaseCue;
                _view.OnEquipCue -= HandleEquipCue;
                _view.OnUnequipCue -= HandleUnequipCue;
            }
        }

        public void Initialize()
        {
            // VContainer automatically calls this on launch
            HandleStandardCues();
        }
    }
}