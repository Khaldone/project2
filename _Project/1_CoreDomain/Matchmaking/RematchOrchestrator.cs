// Assets/_Project/1_CoreDomain/Matchmaking/RematchOrchestrator.cs
using Cysharp.Threading.Tasks;


public enum RematchState { None, RequestedByMe, RequestedByOpponent, Accepted, Declined }


public class RematchOrchestrator
{
    private readonly ILocalWalletCache _wallet;
    //private readonly IMatchmakingCloudService _cloudService; // Talks to Azure
    private readonly IMessageBroker_New _broker;

    private RematchState _currentState = RematchState.None;


    //public RematchOrchestrator(ILocalWalletCache wallet, IMatchmakingCloudService cloudService, IMessageBroker_New broker)
    //{
    //    _wallet = wallet;
    //    _cloudService = cloudService;
    //    _broker = broker;
    //}

    public RematchOrchestrator(ILocalWalletCache wallet, IMessageBroker_New broker)
    {
        _wallet = wallet;
        _broker = broker;
    }

    // Called when local player clicks "Rematch" on the win/loss screen
    public async UniTask RequestRematchAsync(string currentTierName, int entryFee)
    {
        // 1. FAST FAIL: Do they even have enough coins for round 2?
        if (_wallet.GetWallet().Coins < entryFee)
        {
            //_broker.Publish(new SystemNotificationMessage("Insufficient Funds", "You cannot afford a rematch."));
            return;
        }


        _currentState = RematchState.RequestedByMe;
        //_broker.Publish(new RematchStateChangedMessage(_currentState));


        // Tell Photon to ask the opponent
        //_cloudService.SendRematchRequestToOpponent();
    }


    // Called via Photon when opponent clicks Rematch
    public async UniTask OnOpponentRequestedRematchAsync(string currentTierName, int entryFee)
    {
        if (_currentState == RematchState.RequestedByMe)
        {
            // WE BOTH AGREED! Handshake complete.
            _currentState = RematchState.Accepted;
            //_broker.Publish(new RematchStateChangedMessage(_currentState));


            // CRITICAL: We do NOT restart the physics yet.
            // We must ask Azure to deduct the coins for the new match first.
            //bool escrowSuccess = await _cloudService.PayEntryFeeAsync(currentTierName);

            //if (escrowSuccess)
            //{
            //    // Coins deducted securely. Restart the arena!
            //    _broker.Publish(new RestartMatchCommand());
            //}
            //else
            //{
            //    //_broker.Publish(new SystemNotificationMessage("Error", "Escrow failed. Returning to lobby."));
            //    _broker.Publish(new ReturnToLobbyCommand());
            //}
        }
        else
        {
            // They asked us. We show the UI prompt: "Opponent wants a rematch! [Accept] [Decline]"
            _currentState = RematchState.RequestedByOpponent;
            //_broker.Publish(new RematchStateChangedMessage(_currentState));
        }
    }
}