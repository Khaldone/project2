// Internal bridges for the Orchestrator
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Billiards.CoreDomain.Monetization;
public interface IPlatformStore // Wraps Unity IAP
{
    Task<List<StoreProduct>> FetchProductsAsync();
    void BuyProduct(string productId);
    event Action<string, string> OnReceiptReceived; // ProductID, ReceiptData
    event Action<string> OnPlatformPurchaseFailed;
}
//public interface IReceiptValidator // Wraps PlayFab
//{
//    Task<bool> ValidateReceiptAndGrantItemsAsync(string productId, string receiptData);
//}