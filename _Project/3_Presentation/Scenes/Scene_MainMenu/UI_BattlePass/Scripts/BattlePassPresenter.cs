// Assets/_Project/Scenes/UI_BattlePass/Scripts/BattlePassPresenter.cs
using PlayFab.EconomyModels;
using System;
using VContainer.Unity;
using Billiards.CoreDomain.Services;

public class BattlePassPresenter : IStartable, IDisposable
{
    private readonly IBattlePassOrchestrator _orchestrator;
    //private readonly IBattlePassView _view;
    private readonly IInventoryOrchestrator _inventoryOrchestrator;
    private readonly IAssetDeliveryService _assetDelivery;

    //public BattlePassPresenter(IBattlePassOrchestrator orchestrator, IBattlePassView view)
    //{
    //    _orchestrator = orchestrator;
    //    _view = view;
    //}

    public BattlePassPresenter(IBattlePassOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async void Start()
    {
        // 1. Listen for data changes
        _orchestrator.OnStateUpdated += UpdateUIView;

        // 2. Listen to UI Button clicks
        //_view.OnClaimButtonClicked += HandleClaimRequest;
        //_view.OnBuyPremiumClicked += HandlePremiumRequest;


        // 3. Command the fetch
        //_view.ShowLoading(true);
        await _orchestrator.FetchLatestDataAsync();
    }


    private void UpdateUIView(BattlePassState state)
    {
        //_view.ShowLoading(false);
        //_view.UpdateProgressBar(state.CurrentLevel, state.CurrentExp, state.ExpToNextLevel);
        //_view.SetPremiumTheme(state.IsPremiumUnlocked);

        // _view.RenderRewardTrack(_orchestrator.AvailableRewards);
    }


    private async void HandleClaimRequest(string itemId)
    {
        //_view.ShowLoadingBlocker(true);
        bool success = await _orchestrator.ClaimRewardAsync(itemId);
        //_view.ShowLoadingBlocker(false);


        if (success)
        {
            // 2. Tell our local inventory it needs to sync with PlayFab
            await _inventoryOrchestrator.RefreshInventoryAsync();


            // 3. Optional: Ask Addressables to pre-download the heavy 3D asset
            // into RAM right now so it's ready when they go to their loadout screen.
            // The 'itemId' string MUST match the Addressables Key perfectly!
            await _assetDelivery.LoadAssetAsync<UnityEngine.GameObject>(itemId);


            // 4. Play the shiny unlock animation
            //_view.PlayUnlockAnimation(itemId);
        }

        //_view.ShowLoadingBlocker(false);
    }
    private void HandlePremiumRequest() => _orchestrator.PurchasePremiumPassAsync();
    public void Dispose()
    {
        _orchestrator.OnStateUpdated -= UpdateUIView;
        //_view.OnClaimButtonClicked -= HandleClaimRequest;
        //_view.OnBuyPremiumClicked -= HandlePremiumRequest;
    }

}