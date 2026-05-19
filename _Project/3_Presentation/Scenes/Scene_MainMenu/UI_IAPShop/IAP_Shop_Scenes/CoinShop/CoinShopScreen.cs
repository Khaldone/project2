// Assets/_Project/3_Presentation/Scene_CoinShop/Scripts/CoinShopScreen.cs
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using System;
using Billiards.CoreDomain.Player;
using Billiards.CoreDomain.Monetization;
using Billiards.Presentation.Shop;

namespace Billiards.Presentation
{
    public class CoinShopScreen : SsBaseMenu
    {
        private MainMenuRouter _router;

        [Header("Tab Navigation")]
        [SerializeField] private Button _tab1CoinsBtn;
        [SerializeField] private Button _tab2AchievementsBtn;
        [SerializeField] private Button _tab3PlaceholderBtn;

        [Header("Lazy Load Addressables")]
        [SerializeField] private AssetReferenceGameObject _coinsPanelRef;
        [SerializeField] private AssetReferenceGameObject _achievementsPanelRef;
        [SerializeField] private AssetReferenceGameObject _placeholderPanelRef;

        [Header("Spawn Location")]
        [SerializeField] private Transform _panelContainer; // Drag your 'Main Panel' here!

        private GameObject _activePanel;
        private Dictionary<string, GameObject> _instantiatedPanels = new();

        public event Action OnCoinsTabClicked;
        public event Action OnAchievementsTabClicked;
        public event Action OnPlaceholderTabClicked;

        public event Action<string> OnPurchasePackRequested;

        public override void Awake()
        {
            base.Awake();
            _tab1CoinsBtn.onClick.AddListener(() => OnCoinsTabClicked?.Invoke());
            _tab2AchievementsBtn.onClick.AddListener(() => OnAchievementsTabClicked?.Invoke());
            _tab3PlaceholderBtn.onClick.AddListener(() => OnPlaceholderTabClicked?.Invoke());
        }

        public void InitializeRouter(MainMenuRouter sharedRouter)
        {
            _router = sharedRouter;
        }

        private async UniTask<T> GetOrSpawnPanelAsync<T>(string panelKey, AssetReferenceGameObject assetRef) where T : MonoBehaviour
        {
            if (_activePanel != null) _activePanel.SetActive(false);

            if (_instantiatedPanels.TryGetValue(panelKey, out GameObject existingPanel))
            {
                existingPanel.SetActive(true);
                _activePanel = existingPanel;
                return existingPanel.GetComponent<T>();
            }

            // Spawn the Addressable and parent it to your empty Main Panel
            GameObject newPanel = await assetRef.InstantiateAsync(_panelContainer).Task.AsUniTask();

            _instantiatedPanels.Add(panelKey, newPanel);
            _activePanel = newPanel;

            return newPanel.GetComponent<T>();
        }

        public async UniTask DisplayCoinsPanelAsync(List<StoreProduct> goldDeals, List<StoreProduct> cashDeals, Billiards.CoreDomain.Assets.IStoreAssetProvider assetProvider)
        {
            var coinsView = await GetOrSpawnPanelAsync<CoinsPanelView>("Coin_Panel", _coinsPanelRef);
            
            // Clean up to prevent double subscription on re-entry
            coinsView.OnPurchasePackClicked -= HandlePurchasePack;
            coinsView.OnPurchasePackClicked += HandlePurchasePack;

            await coinsView.PopulateDealsAsync(goldDeals, cashDeals, assetProvider);
        }

        private void HandlePurchasePack(string productId)
        {
            OnPurchasePackRequested?.Invoke(productId);
        }

        public async UniTask DisplayAchievementsPanelAsync(List<AchievementData> achievements)
        {
            var achievementsView = await GetOrSpawnPanelAsync<AchievementsPanelView>("Achivements_Panel", _achievementsPanelRef);
            if(achievements.Count >= 1)
            {
                await achievementsView.PopulateProgressAsync(achievements);
            }
            else
            {
                Debug.Log("No achievements to display. Count = " + achievements.Count);
            }
            
        }

        public async UniTask DisplayPlaceholderPanelAsync()
        {
            var placeholderView = await GetOrSpawnPanelAsync<PlaceholderPanelView>("Placeholder", _placeholderPanelRef);
            placeholderView.PopulateData();
        }

        private void OnDestroy()
        {
            foreach (var kvp in _instantiatedPanels)
            {
                if (kvp.Value != null) Addressables.ReleaseInstance(kvp.Value);
            }
            _instantiatedPanels.Clear();
        }
    }
}