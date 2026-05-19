// Assets/_Project/3_Presentation/Scene_IAP/Scripts/IAPShopPresenter.cs
using Billiards.CoreDomain.Monetization;
using System;

namespace Billiards.Presentation.Shop
{
    public class IAPShopPresenter : IDisposable
    {
        private readonly IAP_Screen _view;
        private readonly IStoreOrchestrator _storeOrchestrator;

        public IAPShopPresenter(IAP_Screen view, IStoreOrchestrator storeOrchestrator)
        {
            _view = view;
            _storeOrchestrator = storeOrchestrator;

            // Listen to the Global Backend Events
            _storeOrchestrator.OnPurchaseStarted += HandlePurchaseStarted;
            _storeOrchestrator.OnPurchaseSuccessful += HandlePurchaseSuccessful;
            _storeOrchestrator.OnPurchaseFailed += HandlePurchaseFailed;
        }

        private void HandlePurchaseStarted(string productId)
        {
            // Blocks the screen when Apple/Google overlay appears
            _view.ShowLoading("Connecting to Store...");
        }

        private void HandlePurchaseSuccessful(StoreProduct product)
        {
            // Called after PlayFab successfully validates the receipt
            _view.ShowSuccess(product.Name);
        }

        private void HandlePurchaseFailed(string errorMessage)
        {
            _view.ShowError(errorMessage);
        }

        public void Dispose()
        {
            // Crucial: Unsubscribe to prevent memory leaks when returning to the Home Menu
            if (_storeOrchestrator != null)
            {
                _storeOrchestrator.OnPurchaseStarted -= HandlePurchaseStarted;
                _storeOrchestrator.OnPurchaseSuccessful -= HandlePurchaseSuccessful;
                _storeOrchestrator.OnPurchaseFailed -= HandlePurchaseFailed;
            }
        }
    }
}