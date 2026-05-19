// 2. Assets/Scripts/CoreDomain/Monetization/IStoreOrchestrator.cs
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Billiards.CoreDomain.Monetization;

// The interface exposed to the Service Locator for the UI to use

public interface IStoreOrchestrator
{
    string GetPrice(string productId);
    IReadOnlyList<StoreProduct> AvailableProducts { get; }

    // UI Events
    event Action<string> OnPurchaseStarted;
    event Action<StoreProduct> OnPurchaseSuccessful;
    event Action<string> OnPurchaseFailed;

    UniTask InitializeStoreAsync();

    // ONE unified method for the UI Presenter to call
    UniTask<bool> ProcessFullPurchaseFlowAsync(string productId, int coinRewardAmount);
}