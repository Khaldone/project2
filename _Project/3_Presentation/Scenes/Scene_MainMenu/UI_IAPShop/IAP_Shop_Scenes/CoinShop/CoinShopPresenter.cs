// Assets/_Project/3_Presentation/Scene_CoinShop/Scripts/CoinShopPresenter.cs
using System;
using Billiards.CoreDomain.Monetization;
using Billiards.CoreDomain.Player;
using UnityEngine;
using VContainer.Unity; // 1. ADD THIS for IInitializable

namespace Billiards.Presentation.Shop
{
    // 2. ADD IInitializable to the class signature
    public class CoinShopPresenter : IInitializable, IDisposable
    {
        private readonly CoinShopScreen _view;
        private readonly IStoreDataSource _storeDataSource;
        private readonly IPlayerDataService _playerDataService;
        private readonly Billiards.CoreDomain.Assets.IStoreAssetProvider _assetProvider;
        private readonly IStoreOrchestrator _storeOrchestrator;

        public CoinShopPresenter(CoinShopScreen view, IStoreDataSource storeDataSource, IPlayerDataService playerDataService, Billiards.CoreDomain.Assets.IStoreAssetProvider assetProvider, IStoreOrchestrator storeOrchestrator)
        {
            _view = view;
            _storeDataSource = storeDataSource;
            _playerDataService = playerDataService;
            _assetProvider = assetProvider;
            _storeOrchestrator = storeOrchestrator;

            // Wire up the events
            _view.OnCoinsTabClicked += HandleCoinsTabClicked;
            _view.OnAchievementsTabClicked += HandleAchievementsTabClicked;
            _view.OnPlaceholderTabClicked += HandlePlaceholderTabClicked;
            
            _view.OnPurchasePackRequested += HandlePurchasePackRequested;
        }

        // 3. VContainer will automatically call this method the moment the scene loads!
        public void Initialize()
        {
            Debug.Log("[CoinShop] Presenter awake and listening!");
            HandleCoinsTabClicked(); // Force the first tab to open
        }

        private async void HandleCoinsTabClicked()
        {
            try 
            { 
                var goldDeals = await _storeDataSource.FetchStorePacksAsync("goldpack");
                var cashDeals = await _storeDataSource.FetchStorePacksAsync("cashpack");
                
                await _view.DisplayCoinsPanelAsync(goldDeals, cashDeals, _assetProvider); 
            }
            catch (Exception ex) { Debug.LogError($"Failed to load Coins: {ex.Message}"); }
        }

        private async void HandlePurchasePackRequested(string productId)
        {
            Debug.Log($"[CoinShop] Real-Money Purchase Requested for Pack ID: {productId}. IAP Flow pending.");
            bool success = await _storeOrchestrator.ProcessFullPurchaseFlowAsync(productId, 0);
            if (success)
            {
                Debug.Log($"[CoinShop] Successfully purchased {productId}!");
            }
            else
            {
                Debug.LogError($"[CoinShop] Purchase failed or cancelled for {productId}.");
            }
        }
        

        private async void HandleAchievementsTabClicked()
        {
            try 
            {
                var achievements = await _playerDataService.GetAchievementsAsync();
                await _view.DisplayAchievementsPanelAsync(achievements); 
            }
            catch (Exception ex) { Debug.LogError($"Failed to load Achievements: {ex.Message}"); }
        }

        private async void HandlePlaceholderTabClicked()
        {
            try { await _view.DisplayPlaceholderPanelAsync(); }
            catch (Exception ex) { Debug.LogError($"Failed to load Placeholder: {ex.Message}"); }
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.OnCoinsTabClicked -= HandleCoinsTabClicked;
                _view.OnAchievementsTabClicked -= HandleAchievementsTabClicked;
                _view.OnPlaceholderTabClicked -= HandlePlaceholderTabClicked;
                
                _view.OnPurchasePackRequested -= HandlePurchasePackRequested;
            }
        }
    }
}
