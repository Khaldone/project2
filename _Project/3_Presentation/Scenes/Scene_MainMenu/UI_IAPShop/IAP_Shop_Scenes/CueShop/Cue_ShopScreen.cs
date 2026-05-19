// Assets/_Project/3_Presentation/Scene_CueShop/Scripts/Cue_ShopScreen.cs
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Billiards.CoreDomain.Monetization;
using Billiards.Presentation.Shop;
using static MainMenuRouter;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Billiards.Presentation
{
    public class Cue_ShopScreen : SsBaseMenu
    {
        private MainMenuRouter _router;

        [Header("Tab Navigation")]
        [SerializeField] private Button _standardCuesBtn;
        [SerializeField] private Button _rareCuesBtn;
        [SerializeField] private Button _legendaryCuesBtn;
        [SerializeField] private Button _backBtn;

        [Header("Virtualization Layout")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _scrollContentContainer;
        [SerializeField] private float _rowHeight = 200f;
        [SerializeField] private float _spacing = 15f;
        [SerializeField] private float _topPadding = 20f;
        [SerializeField] private float _bottomPadding = 50f;

        // Events for the Presenter
        public event Action OnStandardCuesClicked;
        public event Action OnRareCuesClicked;
        public event Action OnLegendaryCuesClicked;

        // Action events for specific cues
        public event Action<string> OnPurchaseCue;
        public event Action<string> OnEquipCue;
        public event Action<string> OnUnequipCue;

        // The Zero-GC Object Pool & Virtualization state
        private readonly List<ShopProductButton> _buttonPool = new();
        private List<StoreProduct> _currentProducts = new();
        private List<string> _ownedItemIds = new();
        private List<string> _equippedItemIds = new();
        private int _visibleCount = 0;
        private int _previousTopIndex = -1;
        private Billiards.CoreDomain.Assets.IStoreAssetProvider _assetProvider;

        public override void Awake()
        {
            base.Awake();

            _standardCuesBtn.onClick.AddListener(() => OnStandardCuesClicked?.Invoke());
            _rareCuesBtn.onClick.AddListener(() => OnRareCuesClicked?.Invoke());
            _legendaryCuesBtn.onClick.AddListener(() => OnLegendaryCuesClicked?.Invoke());

            _backBtn.onClick.AddListener(GoBack);

            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.AddListener(OnScroll);
            }
        }

        public void InitializeRouter(MainMenuRouter sharedRouter)
        {
            _router = sharedRouter;
            Debug.Log($"[CueShop] Received Router ID: {_router.GetHashCode()}");
        }

        public void GoBack()
        {
            if (_router != null) _router.TransitionTo<HomeMenu>(ShowStyle.FromLeft, HideStyle.ToRight);
        }

        /// <summary>
        /// Recycles buttons and populates them with new data instantly using Virtualization.
        /// </summary>
        public async UniTask DisplayProductsAsync(List<StoreProduct> products, List<string> ownedIds, List<string> equippedIds, Billiards.CoreDomain.Assets.IStoreAssetProvider assetProvider)
        {
            _assetProvider = assetProvider;
            _currentProducts = products ?? new List<StoreProduct>();
            _ownedItemIds = ownedIds ?? new List<string>();
            _equippedItemIds = equippedIds ?? new List<string>();

            if (_currentProducts.Count == 0)
            {
                // Clear UI if empty
                foreach (var btn in _buttonPool) btn.gameObject.SetActive(false);
                return;
            }

            // 1. Set Content Height Mathematically
            float totalHeight = _currentProducts.Count * (_rowHeight + _spacing) + _topPadding + _bottomPadding;
            _scrollContentContainer.sizeDelta = new Vector2(_scrollContentContainer.sizeDelta.x, totalHeight);

            // 2. Calculate exactly how many items fit on screen + 2 for offscreen buffer
            float viewportHeight = _scrollRect.viewport != null ? _scrollRect.viewport.rect.height : _scrollRect.GetComponent<RectTransform>().rect.height;
            _visibleCount = Mathf.CeilToInt(viewportHeight / (_rowHeight + _spacing)) + 2;

            // 3. Pool Instantiation (Max ~10 items ever)
            int itemsToSpawn = Mathf.Min(_visibleCount, _currentProducts.Count);
            for (int i = _buttonPool.Count; i < itemsToSpawn; i++)
            {
                var rowObj = await Addressables.InstantiateAsync("CueProductBtn", _scrollContentContainer).Task.AsUniTask();
                
                // Ensure correct pivot for math positioning and stretch horizontally
                var rect = rowObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.offsetMin = new Vector2(0, rect.offsetMin.y);
                rect.offsetMax = new Vector2(0, rect.offsetMax.y);
                rect.sizeDelta = new Vector2(0, _rowHeight);
                
                var button = rowObj.GetComponent<ShopProductButton>();
                
                // Wire up the inner button click to bubble to our View event
                button.OnPrimaryActionClicked += HandleButtonAction;
                
                _buttonPool.Add(button);
            }

            _previousTopIndex = -1;
            UpdateVisibleRows();
        }

        private void HandleButtonAction(string productId)
        {
            if (_equippedItemIds.Contains(productId))
            {
                OnUnequipCue?.Invoke(productId);
            }
            else if (_ownedItemIds.Contains(productId))
            {
                OnEquipCue?.Invoke(productId);
            }
            else
            {
                OnPurchaseCue?.Invoke(productId);
            }
        }

        private void OnScroll(Vector2 scrollPos)
        {
            UpdateVisibleRows();
        }

        private void UpdateVisibleRows()
        {
            if (_currentProducts == null || _currentProducts.Count == 0 || _buttonPool.Count == 0) return;

            // Calculate which index is currently at the top of the viewport
            float currentY = _scrollContentContainer.anchoredPosition.y;
            int topIndex = Mathf.FloorToInt(Mathf.Max(0, currentY - _topPadding) / (_rowHeight + _spacing));
            
            // Clamp topIndex so we don't try to draw out of bounds at the bottom
            topIndex = Mathf.Clamp(topIndex, 0, Mathf.Max(0, _currentProducts.Count - _visibleCount));

            // Only update if we crossed a row threshold to save CPU
            if (topIndex == _previousTopIndex) return;
            _previousTopIndex = topIndex;

            // Update the pool
            for (int i = 0; i < _buttonPool.Count; i++)
            {
                int dataIndex = topIndex + i;
                var button = _buttonPool[i];
                var rect = button.GetComponent<RectTransform>();

                if (dataIndex < _currentProducts.Count)
                {
                    button.gameObject.SetActive(true);
                    
                    // Manually position the row with the top padding included
                    float yPos = - (dataIndex * (_rowHeight + _spacing)) - _topPadding;
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, yPos);
                    
                    var product = _currentProducts[dataIndex];
                    bool isOwned = _ownedItemIds.Contains(product.Id);
                    bool isEquipped = _equippedItemIds.Contains(product.Id);
                    
                    // Inject data (Fire and forget async setup)
                    _ = button.SetupAsync(product, _assetProvider, isOwned, isEquipped);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }
        }
    }
}