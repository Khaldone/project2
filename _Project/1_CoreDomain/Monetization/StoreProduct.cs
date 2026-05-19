using System;
using System.Collections.Generic;

namespace Billiards.CoreDomain.Monetization
{
    public struct StoreProduct
    {
        public string Id;          // e.g., "com.yourstudio.billiards.coins_100"
        public string Name;        // e.g., "Pile of Coins"
        public string LocalizedPrice; // e.g., "$0.99" or "250 GD"
        public int VirtualCurrencyPrice; // e.g., 250
        public string VirtualCurrencyCode; // e.g., "GD"
    }
}
