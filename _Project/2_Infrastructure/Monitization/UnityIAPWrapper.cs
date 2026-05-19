// Assets/_Project/Infrastructure/Monetization/UnityIAPWrapper.cs
// Unity IAP v5 API — event-driven, no IStoreListener
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Billiards.Infrastructure
{
    /// <summary>
    /// Infrastructure wrapper for Unity IAP v5.
    /// Uses the new StoreController event-driven API instead of the deprecated IStoreListener pattern.
    /// </summary>
    public class UnityIAPWrapper : IStorePlatform
    {
        private StoreController _storeController;
        private List<Product> _fetchedProducts;
        private bool _isConnected;

        private UniTaskCompletionSource _initTcs;
        private UniTaskCompletionSource<Billiards.CoreDomain.Monetization.PurchaseReceipt?> _currentTransactionTcs;

        public async UniTask InitializePlatformAsync(string[] productIds)
        {
            if (_isConnected && _fetchedProducts != null) return; // Already initialized

            _initTcs = new UniTaskCompletionSource();

            if (productIds == null || productIds.Length == 0)
            {
                Debug.LogWarning("[UnityIAPWrapper] No RM products found. Aborting Unity IAP initialization.");
                _initTcs.TrySetResult();
                return;
            }

            // Ensure we are on a clean frame, fully decoupled from any SDK callback stack
            await UniTask.NextFrame(PlayerLoopTiming.Update);

            try
            {
                // 1. Get the v5 StoreController
                _storeController = UnityIAPServices.StoreController();

                // 2. Subscribe to events BEFORE connecting
                _storeController.OnPurchasePending += OnPurchasePending;
                _storeController.OnPurchaseFailed += OnPurchaseFailed;
                _storeController.OnProductsFetched += OnProductsFetched;
                _storeController.OnProductsFetchFailed += OnProductsFetchFailed;

                // 3. Connect to the store (Apple/Google/FakeStore)
                Debug.Log("[UnityIAPWrapper] Connecting to store via IAP v5 API...");
                await _storeController.Connect().AsUniTask();
                _isConnected = true;
                Debug.Log("[UnityIAPWrapper] Store connected successfully.");

                // 4. Fetch products dynamically from our PlayFab catalog
                Debug.Log($"[UnityIAPWrapper] Fetching {productIds.Length} products...");
                var productDefinitions = new List<ProductDefinition>();
                foreach (var id in productIds)
                {
                    productDefinitions.Add(new ProductDefinition(id, ProductType.Consumable));
                }
                _storeController.FetchProducts(productDefinitions);

                // 5. Wait for OnProductsFetched to fire (with 15s timeout)
                var timeoutIndex = await UniTask.WhenAny(
                    _initTcs.Task,
                    UniTask.Delay(TimeSpan.FromSeconds(15), cancellationToken: CancellationToken.None)
                );

                if (timeoutIndex == 1)
                {
                    Debug.LogError("[UnityIAPWrapper] TIMEOUT: Products were not fetched within 15 seconds.");
                }
                else
                {
                    Debug.Log($"[UnityIAPWrapper] IAP v5 Initialization complete. {_fetchedProducts?.Count ?? 0} products ready.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityIAPWrapper] Exception during IAP v5 initialization: {ex}");
                _initTcs.TrySetResult(); // Unblock callers even on failure
            }
        }

        public string GetLocalizedPrice(string productId)
        {
            if (_fetchedProducts == null) return "$--.--";

            foreach (var product in _fetchedProducts)
            {
                if (product.definition.id == productId)
                    return product.metadata.localizedPriceString;
            }
            return "$--.--";
        }

        public async UniTask<Billiards.CoreDomain.Monetization.PurchaseReceipt?> ProcessRealMoneyPurchaseAsync(string productId)
        {
            _currentTransactionTcs = new UniTaskCompletionSource<Billiards.CoreDomain.Monetization.PurchaseReceipt?>();

            if (!_isConnected || _fetchedProducts == null)
            {
                Debug.LogError("[UnityIAPWrapper] FATAL: Store is not initialized. Cannot process purchase.");
                return null;
            }

            // Find the product among our fetched products
            Product targetProduct = null;
            foreach (var product in _fetchedProducts)
            {
                if (product.definition.id == productId && product.availableToPurchase)
                {
                    targetProduct = product;
                    break;
                }
            }

            if (targetProduct == null)
            {
                Debug.LogWarning($"[UnityIAPWrapper] Product {productId} not found or not available to purchase!");
                return null;
            }

            Debug.Log($"[UnityIAPWrapper] Initiating purchase for {productId}...");
            _storeController.PurchaseProduct(targetProduct);

            return await _currentTransactionTcs.Task;
        }

        // --- IAP v5 Event Handlers ---

        private void OnProductsFetched(List<Product> products)
        {
            Debug.Log($"[UnityIAPWrapper] OnProductsFetched: {products.Count} products received.");
            _fetchedProducts = products;
            _initTcs?.TrySetResult();
        }

        private void OnProductsFetchFailed(ProductFetchFailed failure)
        {
            Debug.LogError($"[UnityIAPWrapper] OnProductsFetchFailed: {failure.FailureReason}");
            _initTcs?.TrySetResult(); // Unblock callers
        }

        private void OnPurchasePending(PendingOrder pendingOrder)
        {
            Debug.Log("[UnityIAPWrapper] OnPurchasePending received.");

            // Extract receipt data from the order info
            var receiptData = pendingOrder.Info?.Receipt ?? string.Empty;

            // Get the product from the cart
            var cartItems = pendingOrder.CartOrdered?.Items();
            if (cartItems == null || cartItems.Count == 0)
            {
                Debug.LogError("[UnityIAPWrapper] PendingOrder has no cart items!");
                _currentTransactionTcs?.TrySetResult(null);
                return;
            }

            var product = cartItems[0].Product;
            Debug.Log($"[UnityIAPWrapper] Purchase pending for {product.definition.id}. Confirming...");

            // Confirm the order to complete the transaction
            _storeController.ConfirmPurchase(pendingOrder);

            _currentTransactionTcs?.TrySetResult(new Billiards.CoreDomain.Monetization.PurchaseReceipt
            {
                ReceiptData = receiptData,
                CurrencyCode = product.metadata.isoCurrencyCode,
                PriceInCents = (int)((decimal)product.metadata.localizedPrice * 100)
            });
        }

        private void OnPurchaseFailed(FailedOrder failedOrder)
        {
            Debug.LogWarning($"[UnityIAPWrapper] Purchase failed: {failedOrder.FailureReason} - {failedOrder.Details}");
            _currentTransactionTcs?.TrySetResult(null);
        }
    }
}
