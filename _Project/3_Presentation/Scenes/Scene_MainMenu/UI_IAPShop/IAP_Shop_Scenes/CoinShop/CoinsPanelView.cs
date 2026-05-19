// Assets/_Project/3_Presentation/Scene_IAP/Scripts/CoinsPanelView.cs
using System.Collections.Generic;
using UnityEngine;
using Billiards.CoreDomain.Monetization;

namespace Billiards.Presentation.Shop
{
    public class CoinsPanelView : MonoBehaviour
    {
        [Header("Deal Columns")]
        [SerializeField] private DealColumnView _goldColumn;
        [SerializeField] private DealColumnView _cashColumn;

        public event System.Action<string> OnPurchasePackClicked;

        private void Awake()
        {
            if (_goldColumn != null) _goldColumn.OnPurchaseDealClicked += HandlePurchase;
            if (_cashColumn != null) _cashColumn.OnPurchaseDealClicked += HandlePurchase;
        }

        private void HandlePurchase(string productId)
        {
            OnPurchasePackClicked?.Invoke(productId);
        }

        /// <summary>
        /// Takes the categorized data from the Presenter and populates the vertical columns.
        /// </summary>
        public async Cysharp.Threading.Tasks.UniTask PopulateDealsAsync(List<StoreProduct> goldPacks, List<StoreProduct> cashPacks, Billiards.CoreDomain.Assets.IStoreAssetProvider assetProvider)
        {
            if (_goldColumn != null)
            {
                await _goldColumn.PopulateDealsAsync(goldPacks, assetProvider);
            }
            
            if (_cashColumn != null)
            {
                await _cashColumn.PopulateDealsAsync(cashPacks, assetProvider);
            }
        }
    }
}