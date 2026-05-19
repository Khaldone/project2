// Assets/_Project/3_Presentation/Scene_IAP/Scripts/DealColumnView.cs
using System.Collections.Generic;
using UnityEngine;
using Billiards.CoreDomain.Monetization;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using System;

namespace Billiards.Presentation.Shop
{
    public class DealColumnView : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private RectTransform _contentContainer;
        [SerializeField] private UnityEngine.UI.ScrollRect _scrollRect;
        [SerializeField] private float _rowHeight = 220f; // Adjusted for button height
        [SerializeField] private string _addressablePrefabKey = "CoinProductBtn"; // Use a square prefab here!
        
        // Zero-GC Object Pool
        private readonly List<ShopProductButton> _buttonPool = new();

        public event Action<string> OnPurchaseDealClicked;

        /// <summary>
        /// Instantiates ShopProductButtons into this vertical column.
        /// </summary>
        public async UniTask PopulateDealsAsync(List<StoreProduct> deals, Billiards.CoreDomain.Assets.IStoreAssetProvider assetProvider)
        {
            if (deals == null || deals.Count == 0)
            {
                foreach (var btn in _buttonPool) btn.gameObject.SetActive(false);
                return;
            }

            // Spawn enough buttons for the deals
            for (int i = _buttonPool.Count; i < deals.Count; i++)
            {
                var rowObj = await Addressables.InstantiateAsync(_addressablePrefabKey, _contentContainer).Task.AsUniTask();
                
                // Set to default stretching behavior for vertical lists
                var rect = rowObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.offsetMin = new Vector2(0, rect.offsetMin.y);
                rect.offsetMax = new Vector2(0, rect.offsetMax.y);
                rect.sizeDelta = new Vector2(0, _rowHeight);

                var button = rowObj.GetComponent<ShopProductButton>();
                button.OnPrimaryActionClicked += (productId) => OnPurchaseDealClicked?.Invoke(productId);
                
                _buttonPool.Add(button);
            }

            // Populate and enable
            for (int i = 0; i < _buttonPool.Count; i++)
            {
                var button = _buttonPool[i];
                if (i < deals.Count)
                {
                    button.gameObject.SetActive(true);
                    // Coin packs are consumable, they are never "owned" or "equipped" in the standard sense.
                    _ = button.SetupAsync(deals[i], assetProvider, false, false);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }

            // Reset scroll to top
            if (_scrollRect != null)
            {
                _scrollRect.StopMovement();
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_contentContainer);
                
                // Yield to ensure Unity's UI system has processed the layout changes
                await UniTask.Yield();
                
                _scrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }
}
