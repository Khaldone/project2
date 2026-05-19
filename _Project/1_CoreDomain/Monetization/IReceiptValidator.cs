using Cysharp.Threading.Tasks;
using Billiards.CoreDomain.Monetization;

public interface IReceiptValidator
{
    UniTask<bool> ValidateReceiptAndGrantItemsAsync(string productId, PurchaseReceipt receipt);
}