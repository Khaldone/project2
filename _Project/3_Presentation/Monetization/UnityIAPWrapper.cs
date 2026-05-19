//// Assets/Scripts/Presentation/Monetization/UnityIAPWrapper.cs
//using UnityEngine;
//using UnityEngine.Purchasing;
//using System.Threading.Tasks;


//public class UnityIAPWrapper : MonoBehaviour, IStoreListener, IStorePlatform
//{
//    private IStoreController _storeController;
//    private TaskCompletionSource<bool> _currentTransactionTcs;


//    public void Initialize(ConfigurationBuilder builder)
//    {
//        UnityPurchasing.Initialize(this, builder);
//    }


//    public string GetLocalizedPrice(string productId)
//    {
//        if (_storeController != null)
//        {
//            Product product = _storeController.products.WithID(productId);
//            if (product != null) return product.metadata.localizedPriceString;
//        }
//        return "$--.--";
//    }


//    public Task<bool> ProcessRealMoneyPurchaseAsync(string productId)
//    {
//        _currentTransactionTcs = new TaskCompletionSource<bool>();

//        Product product = _storeController.products.WithID(productId);
//        if (product != null && product.availableToPurchase)
//        {
//            _storeController.InitiatePurchase(product);
//        }
//        else
//        {
//            _currentTransactionTcs.SetResult(false);
//        }


//        return _currentTransactionTcs.Task;
//    }


//    // --- IStoreListener Callbacks ---
//    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
//    {
//        // The real money cleared. Return true to the pure C# orchestrator.
//        _currentTransactionTcs?.TrySetResult(true);
//        return PurchaseProcessingResult.Complete;
//    }


//    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
//    {
//        Debug.LogWarning($"Purchase of {product.definition.id} failed: {failureReason}");
//        _currentTransactionTcs?.TrySetResult(false);
//    }

//    // (Other IStoreListener methods omitted for brevity)
//    public void OnInitialized(IStoreController controller, IExtensionProvider extensions) { _storeController = controller; }
//    public void OnInitializeFailed(InitializationFailureReason error) { }
//    public void OnInitializeFailed(InitializationFailureReason error, string message) { }
//}
