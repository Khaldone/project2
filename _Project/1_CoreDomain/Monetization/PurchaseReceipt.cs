using System;

namespace Billiards.CoreDomain.Monetization
{
    public struct PurchaseReceipt
    {
        public string ReceiptData;
        public string CurrencyCode;
        public int PriceInCents;
    }
}
