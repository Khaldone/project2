// Assets/_Project/Scenes/UI_LootboxReveal/Scripts/LootboxRevealPresenter.cs
using PlayFab.EconomyModels;
using System;
using System.Collections.Generic;
using VContainer.Unity;
using Billiards.CoreDomain.Services;

public class LootboxRevealPresenter : IStartable, IDisposable
{
    //private readonly ILootboxRevealView _view;
    private readonly IAssetDeliveryService _assetDelivery; // For loading the 3D items
    private readonly IUIRouter _router;


    private List<LootboxReward> _rewardsToShow;
    private int _currentRewardIndex = -1;

    // States: Intro -> Idle -> PoppingItem -> WaitingForTap -> Summary -> Closed
    private enum RevealState { Intro, WaitingForTap, PoppingItem, Summary }
    private RevealState _currentState = RevealState.Intro;


    //public LootboxRevealPresenter(ILootboxRevealView view, IAssetDeliveryService assetDelivery, IUIRouter router)
    //{
    //    _view = view;
    //    _assetDelivery = assetDelivery;
    //    _router = router;
    //}

    public LootboxRevealPresenter(IAssetDeliveryService assetDelivery, IUIRouter router)
    {
        _assetDelivery = assetDelivery;
        _router = router;
    }


    // A setup method called by the router right after VContainer resolves this class
    public void InjectPayload(List<LootboxReward> rewards)
    {
        _rewardsToShow = rewards;
    }


    public void Start()
    {
        //_view.OnScreenTapped += HandleScreenTap;


        // SEQUENCE 1: The Setup
        //_view.DimBackground(duration: 0.5f);
        //_view.PlayBoxSpawnAnimation();

        // Allow the player to tap to start the fireworks
        //_currentState = RevealState.WaitingForTap;
    }


    private async void HandleScreenTap()
    {
        if (_currentState != RevealState.WaitingForTap) return; // Prevent spam-clicking breaking the animation


        _currentRewardIndex++;


        // Have we shown all the items?
        if (_currentRewardIndex >= _rewardsToShow.Count)
        {
            ShowSummaryScreen();
            return;
        }


        // SEQUENCE 2: The Pop
        _currentState = RevealState.PoppingItem;
        var currentReward = _rewardsToShow[_currentRewardIndex];


        // 1. Play the box shaking/bursting particle effect
        //_view.PlayBoxEruptionVFX(currentReward.Rarity);


        // 2. Load the specific 3D icon for the reward (e.g., the Dragon Cue piece)
        var itemPrefab = await _assetDelivery.LoadAssetAsync<UnityEngine.GameObject>(currentReward.ItemId);


        // 3. Command the view to animate the item flying out of the box and hanging in the air
        //await _view.PlayItemRevealAnimationAsync(itemPrefab, currentReward.Quantity);


        // 4. Finished animating. Wait for the player to tap again to see the next item.
        _currentState = RevealState.WaitingForTap;
    }


    private void ShowSummaryScreen()
    {
        _currentState = RevealState.Summary;
        //_view.HideBox();
        //_view.ShowFinalSummaryGrid(_rewardsToShow);
        //_view.ChangeTapPromptTo("Tap to Continue");

        // The next tap will call a method to close the additive scene
        //_view.OnScreenTapped -= HandleScreenTap;
        //_view.OnScreenTapped += CloseRevealScene;
    }


    private void CloseRevealScene()
    {
        //_assetDelivery.ReleaseAllUnusedAssets(); // Clear the 3D cue pieces from RAM
        _router.CloseMenuAsync("UI_LootboxReveal");
    }


    public void Dispose()
    {
        //_view.OnScreenTapped -= HandleScreenTap;
        //_view.OnScreenTapped -= CloseRevealScene;
    }
}
