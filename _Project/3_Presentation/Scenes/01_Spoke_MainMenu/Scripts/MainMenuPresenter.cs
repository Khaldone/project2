// Assets/_Project/Scenes/01_Spoke_MainMenu/Scripts/MainMenuPresenter.cs
using PlayFab.EconomyModels;
using System;
using VContainer.Unity;


public class MainMenuPresenter : IStartable, IDisposable
{
    private readonly IDeepLinkOrchestrator _deepLink;
    private readonly IUIRouter _uiRouter;
    //private readonly ILootboxOrchestrator _lootboxOrchestrator;

    public MainMenuPresenter(IDeepLinkOrchestrator deepLink, IUIRouter uiRouter)
    {
        _deepLink = deepLink;
        _uiRouter = uiRouter;
    }

    public void Start()
    {
        // 1. Play the camera fly-in animation, render the 3D table, etc.

        // 2. CHECK THE VAULT
        if (_deepLink.TryConsumePendingLink(out string actionToExecute))
        {
            ExecuteDeepLink(actionToExecute);
        }
    }

    private void ExecuteDeepLink(string actionId)
    {
        switch (actionId)
        {
            case "open_shop":
                // Instantly slide the Shop UI over the Main Menu
                _uiRouter.OpenMenuAsync("UI_IAPShop");
                break;

            case "open_tournament":
                // Route them to the bracket screen
                _uiRouter.OpenMenuAsync("UI_TournamentBracket");
                break;

            case "claim_daily":
                // Perhaps trigger a specific reward popup
                _uiRouter.OpenMenuAsync("UI_DailyRewards");
                break;

            default:
                UnityEngine.Debug.LogWarning($"Unknown deep link action: {actionId}");
                break;
        }
    }

    private async void OnLootboxSlotTapped(LootboxInstance clickedBox)
    {
        if (clickedBox.IsReadyToOpen)
        {
            // 1. Call Backend
            //_view.ShowLoadingSpinner(true);
            //var rewards = await _lootboxOrchestrator.TryOpenLootboxAsync(clickedBox);
            //_view.ShowLoadingSpinner(false);


            //if (rewards != null)
            //{
            //    // 2. Launch the theatrical scene and pass the data!
            //    //_uiRouter.OpenMenuWithPayloadAsync("UI_LootboxReveal", rewards);
            //}
        }
        else
        {
            // Calculate the gem cost based on time remaining (e.g., 1 gem per 10 minutes)
            //int gemCost = CalculateSpeedUpCost(clickedBox.TimeRemaining);


            // Show a UI Popup: "Open instantly for 18 Gems?"
            //_view.ShowSpeedUpPrompt(gemCost, async onConfirm: () =>
            //{
            //    _view.ShowLoadingSpinner(true);
            //    bool success = await _lootboxOrchestrator.SpeedUpLootboxAsync(clickedBox, gemCost);
            //    _view.ShowLoadingSpinner(false);

            //    if (success)
            //    {
            //        // Recursive call: Now the box is ready, so this will trigger the open sequence!
            //        clickedBox.UnlockTime = DateTime.UtcNow;
            //        OnLootboxSlotTapped(clickedBox);
            //    }
            //});
        }
    }


    public void Dispose() { /* ... */ }
}