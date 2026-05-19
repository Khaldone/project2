// Assets/_Project/1_CoreDomain/Matchmaking/MatchmakingOrchestrator.cs
using Cysharp.Threading.Tasks;


public class MatchmakingOrchestrator_New
{
    private readonly ILocalWalletCache _walletCache;
    //private readonly IMatchmakingCloudService _cloudService;
    private readonly IMessageBroker_New _broker;
    private readonly IUIRouter _uiRouter;


    //public MatchmakingOrchestrator_New(
    //    ILocalWalletCache walletCache,
    //    IMatchmakingCloudService cloudService,
    //    IMessageBroker_New broker,
    //    IUIRouter uiRouter)
    //{
    //    _walletCache = walletCache;
    //    _cloudService = cloudService;
    //    _broker = broker;
    //    _uiRouter = uiRouter;
    //}

    public MatchmakingOrchestrator_New(
    ILocalWalletCache walletCache,
    IMessageBroker_New broker,
    IUIRouter uiRouter)
    {
        _walletCache = walletCache;
        _broker = broker;
        _uiRouter = uiRouter;
    }


    public async UniTask RequestJoinTierAsync(string tierName, int requiredFee)
    {
        // 1. FAST FAIL: Check local wallet to prevent unnecessary server calls
        var wallet = _walletCache.GetWallet();
        if (wallet.Coins < requiredFee)
        {
            //_broker.Publish(new SystemNotificationMessage("Insufficient Funds", "You need more coins to enter this tier."));
            //_uiRouter.OpenStorePopup();
            return;
        }


        // 2. UI LOCK: Tell the UI to show the spinning "Deducting Entry Fee..." blocker
        //_broker.Publish(new MatchmakingStateMessage(isSearching: true, statusText: "Securing Entry Fee..."));


        // 3. SERVER ESCROW: Ask the Azure Function to take the money
        //bool escrowSuccess = await _cloudService.PayEntryFeeAsync(tierName);


        //if (escrowSuccess)
        //{
        //    // 4. UPDATE LOCAL CACHE: The server took the money, so we manually update our local UI to match.
        //    wallet.Coins -= requiredFee;
        //    _walletCache.SaveWallet(wallet);
        //    _broker.Publish(new CurrencyUpdatedMessage(wallet.Coins, wallet.Gems));


        //    // 5. PROCEED TO MATCHMAKER: Now we actually look for an opponent in Photon
        //    _broker.Publish(new MatchmakingStateMessage(isSearching: true, statusText: "Looking for Opponent..."));
        //    await _cloudService.ConnectToPhotonMatchmakerAsync(tierName);
        //}
        else
        {
            // Server rejected it (e.g., they tried to hack their local wallet to say they had 1M coins)
            //_broker.Publish(new MatchmakingStateMessage(isSearching: false, statusText: ""));
            //_broker.Publish(new SystemNotificationMessage("Error", "Transaction failed."));
        }
    }
}
