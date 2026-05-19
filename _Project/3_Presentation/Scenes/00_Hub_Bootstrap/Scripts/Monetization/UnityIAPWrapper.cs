// 1. Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Monetization/UnityIAPWrapper.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing; // Unity's IAP SDK

using Billiards.CoreDomain.Monetization;
public class UnityIAPWrapper : MonoBehaviour, IPlatformStore, IStoreListener
{
    public event Action<string, string> OnReceiptReceived;
    public event Action<string> OnPlatformPurchaseFailed;

    // ... Unity IAP Initialization logic ...


    public void BuyProduct(string productId)
    {
        //_storeController.InitiatePurchase(productId);
    }

    public Task<List<StoreProduct>> FetchProductsAsync()
    {
        throw new NotImplementedException();
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        throw new NotImplementedException();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        throw new NotImplementedException();
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message = null)
    {
        throw new NotImplementedException();
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        throw new NotImplementedException();
    }


    // Unity IAP calls this when Apple/Google confirms the charge
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string productId = args.purchasedProduct.definition.id;
        string receipt = args.purchasedProduct.receipt;


        // Pass the receipt up to our Pure C# Orchestrator
        OnReceiptReceived?.Invoke(productId, receipt);


        // We return 'Pending' so Unity IAP knows the transaction isn't officially
        // closed until PlayFab confirms it. This prevents lost purchases if the app crashes!
        return PurchaseProcessingResult.Pending;
    }
}
