// Assets/_Project/1_CoreDomain/Tests/EconomyOrchestratorTests.cs
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;
using NSubstitute.ExceptionExtensions;


[TestFixture]
public class EconomyOrchestratorTests
{
    private ILocalWalletCache _subLocalCache;
    private IEconomyCloudService _subCloudService;
    private IMessageBroker_New _subBroker;
    //private EconomyOrchestrator _orchestrator;


    [SetUp]
    public void Setup()
    {
        // ARRANGE: Set up our pure C# substitutes
        _subLocalCache = Substitute.For<ILocalWalletCache>();
        _subCloudService = Substitute.For<IEconomyCloudService>();
        _subBroker = Substitute.For<IMessageBroker_New>();


        // Inject them into the Orchestrator
        //_orchestrator = new EconomyOrchestrator(_subLocalCache, _subCloudService, _subBroker);
    }


    [Test]
    public async Task SpendCurrency_WithSufficientFunds_DeductsAndSyncs()
    {
        // ARRANGE: Player has 1000 Coins and 50 Gems
        var startingWallet = new PlayerWallet { Coins = 1000, Gems = 50, HasPendingCloudSync = false };
        _subLocalCache.GetWallet().Returns(startingWallet);


        // ACT: Player attempts to buy a Cue Stick for 400 Coins
        //bool purchaseSuccess = await _orchestrator.TrySpendCurrencyAsync(CurrencyType.Coins, 400);


        // ASSERT
        //Assert.IsTrue(purchaseSuccess, "The purchase should have been approved.");


        // 1. Verify the local cache was updated to 600 Coins
        _subLocalCache.Received(1).SaveWallet(Arg.Is<PlayerWallet>(w =>
            w.Coins == 600 &&
            w.Gems == 50
        ));


        // 2. Verify the Message Broker told the Top Bar UI to update its text
        _subBroker.Received(1).Publish(Arg.Is<CurrencyUpdatedMessage_New>(msg =>
            msg.NewCoins == 600
        ));


        // 3. Verify it commanded the Infrastructure layer to tell PlayFab about the purchase
        await _subCloudService.Received(1).SyncWalletToServerAsync(Arg.Any<PlayerWallet>());
    }


    [Test]
    public async Task SpendCurrency_WithInsufficientFunds_RejectsAndDoesNotSync()
    {
        // ARRANGE: Player only has 10 Gems
        var startingWallet = new PlayerWallet { Coins = 1000, Gems = 10 };
        _subLocalCache.GetWallet().Returns(startingWallet);


        // ACT: Player attempts to buy a Premium Lootbox for 500 Gems
        //bool purchaseSuccess = await _orchestrator.TrySpendCurrencyAsync(CurrencyType.Gems, 500);


        // ASSERT
        //Assert.IsFalse(purchaseSuccess, "The Orchestrator should have blocked the transaction.");


        // 1. Verify the wallet was NEVER saved with negative gems
        _subLocalCache.DidNotReceive().SaveWallet(Arg.Any<PlayerWallet>());


        // 2. Verify the UI was NEVER told to update
        _subBroker.DidNotReceive().Publish(Arg.Any<CurrencyUpdatedMessage>());


        // 3. CRITICAL: Verify PlayFab was NEVER contacted to prevent useless API calls
        await _subCloudService.DidNotReceive().SyncWalletToServerAsync(Arg.Any<PlayerWallet>());
    }


    [Test]
    public async Task GrantCurrency_WhenCloudFails_SetsLocalDirtyBit()
    {
        // ARRANGE: Player has 0 Coins.
        var startingWallet = new PlayerWallet { Coins = 0, HasPendingCloudSync = false };
        _subLocalCache.GetWallet().Returns(startingWallet);


        // Simulate a network outage: PlayFab throws an exception when the Orchestrator tries to sync
        _subCloudService.SyncWalletToServerAsync(Arg.Any<PlayerWallet>())
                        .Throws(new Exception("Network Timeout"));


        // ACT: Player opens a daily reward box and gets 100 Coins
        // We use Assert.DoesNotThrow to ensure the Orchestrator catches the PlayFab crash securely
        Assert.DoesNotThrowAsync(async () =>
        {
            //await _orchestrator.GrantCurrencyAsync(CurrencyType.Coins, 100);
        });


        // ASSERT
        // Verify the Orchestrator successfully saved the 100 coins locally, but FLAGGED it as dirty
        _subLocalCache.Received(1).SaveWallet(Arg.Is<PlayerWallet>(w =>
            w.Coins == 100 &&
            w.HasPendingCloudSync == true
        ));


        // The UI should STILL update so the player sees their reward, even if they are offline!
        _subBroker.Received(1).Publish(Arg.Is<CurrencyUpdatedMessage_New>(msg =>
            msg.NewCoins == 100
        ));
    }
}
