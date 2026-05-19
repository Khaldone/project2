using Cysharp.Threading.Tasks;
using Billiards.CoreDomain.Monetization;
public interface IStorePlatform
{
    UniTask InitializePlatformAsync(string[] productIds);
    string GetLocalizedPrice(string productId);

    // Returns the receipt payload and metadata if successful, or null/default if it failed/was cancelled
    UniTask<PurchaseReceipt?> ProcessRealMoneyPurchaseAsync(string productId);
}