// 3. Assets/Scripts/CoreDomain/Monetization/StoreOrchestrator.cs
// The Brain. This links real money to the virtual economy.
using Billiards.CoreDomain.Monetization;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public class StoreOrchestrator : IStoreOrchestrator
{
    private readonly IStorePlatform _storePlatform;
    private readonly IPlayerDataService _playerDataService;
    private readonly IReceiptValidator _backendValidator;
    private readonly IStoreDataSource _storeDataSource;

    public event Action<string> OnPurchaseStarted;
    public event Action<StoreProduct> OnPurchaseSuccessful;
    public event Action<string> OnPurchaseFailed;

    public IReadOnlyList<StoreProduct> AvailableProducts { get; private set; }

    private bool _isInitialized = false;

    public StoreOrchestrator(
        IStorePlatform storePlatform, 
        IPlayerDataService playerDataService, 
        IReceiptValidator backendValidator, 
        IStoreDataSource storeDataSource)
    {
        _storePlatform = storePlatform;
        _playerDataService = playerDataService;
        _backendValidator = backendValidator;
        _storeDataSource = storeDataSource;
    }

    public string GetPrice(string productId) => _storePlatform.GetLocalizedPrice(productId);

    // THE MASTER PIPELINE: Platform -> Validator -> Economy
    public async UniTask<bool> ProcessFullPurchaseFlowAsync(string productId, int coinRewardAmount)
    {
        // Self-initialize if needed
        if (!_isInitialized)
        {
            UnityEngine.Debug.Log("[StoreOrchestrator] Store not initialized yet. Initializing on-demand...");
            await InitializeStoreAsync();
        }

        OnPurchaseStarted?.Invoke(productId);

        try
        {
            // 1. Process the real-money transaction
            PurchaseReceipt? receiptData = await _storePlatform.ProcessRealMoneyPurchaseAsync(productId);

            if (!receiptData.HasValue || string.IsNullOrEmpty(receiptData.Value.ReceiptData))
            {
                OnPurchaseFailed?.Invoke("Transaction cancelled or failed.");
                return false;
            }

            // 2. Validate with PlayFab/CBS
            bool isLegit = await _backendValidator.ValidateReceiptAndGrantItemsAsync(productId, receiptData.Value);

            if (isLegit)
            {
                // 3. Update local economy
                _playerDataService.AddCoins(coinRewardAmount);
                await _playerDataService.ForceSyncAsync();

                OnPurchaseSuccessful?.Invoke(new StoreProduct { Id = productId });
                return true;
            }

            OnPurchaseFailed?.Invoke("Validation failed.");
            return false;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[StoreOrchestrator] Exception in ProcessFullPurchaseFlowAsync: {ex}");
            OnPurchaseFailed?.Invoke($"Store Error: {ex.Message}");
            return false;
        }


    }

    public async UniTask InitializeStoreAsync()
    {
        if (_isInitialized) return;

        try 
        {
            UnityEngine.Debug.Log("[StoreOrchestrator] Beginning IAP Initialization...");

            // 0. Ensure CBS has finished fetching the catalog from PlayFab
            await _storeDataSource.ForceRefreshAsync();
            UnityEngine.Debug.Log("[StoreOrchestrator] CBS ForceRefresh complete.");

            // 1. Pull the perfectly cached RM product IDs from CBS
            var productIds = await _storeDataSource.GetAllRealMoneyProductIdsAsync();
            UnityEngine.Debug.Log($"[StoreOrchestrator] Found {productIds.Count} RM product IDs.");

            // Give the engine a frame to breathe and decouple from the CBS HTTP Response Coroutine
            await UniTask.NextFrame();

            // 2. Tell Unity IAP to start booting up in the background with the specific IDs
            await _storePlatform.InitializePlatformAsync(productIds.ToArray());

            _isInitialized = true;
            UnityEngine.Debug.Log($"[StoreOrchestrator] Unity IAP Successfully Initialized with {productIds.Count} RM products.");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[StoreOrchestrator] CRITICAL FAILURE initializing Unity IAP: {ex}");
        }
    }
}
