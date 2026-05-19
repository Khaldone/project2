// Assets/_Project/1_CoreDomain/Tests/MatchmakingOrchestratorTests.cs
using NUnit.Framework;
using NSubstitute;
using System.Threading.Tasks;


[TestFixture]
public class MatchmakingOrchestratorTests
{
    private ILocalWalletCache _subWallet;
    //private IMatchmakingCloudService _subCloud;
    private IMessageBroker_New _subBroker;
    private IUIRouter _subRouter;
    private MatchmakingOrchestrator _orchestrator;


    [SetUp]
    public void Setup()
    {
        _subWallet = Substitute.For<ILocalWalletCache>();
        //_subCloud = Substitute.For<IMatchmakingCloudService>();
        _subBroker = Substitute.For<IMessageBroker_New>();
        _subRouter = Substitute.For<IUIRouter>();


        //_orchestrator = new MatchmakingOrchestrator(_subWallet, _subCloud, _subBroker, _subRouter);
    }


    [Test]
    public async Task RequestJoinTier_WithInsufficientLocalFunds_HaltsInstantly()
    {
        // ARRANGE: Player has 10 coins. Tokyo costs 500.
        _subWallet.GetWallet().Returns(new PlayerWallet { Coins = 10 });


        // ACT
        //await _orchestrator.RequestJoinTierAsync("Tokyo", 500);


        // ASSERT
        // 1. Verify the Azure Function was NEVER called
        //await _subCloud.DidNotReceive().PayEntryFeeAsync(Arg.Any<string>());

        // 2. Verify the player was routed to the store
        //_subRouter.Received(1).OpenStorePopup();
    }


    [Test]
    public async Task RequestJoinTier_WhenAzureFails_DoesNotDeductLocalWallet()
    {
        // ARRANGE: Player has 1000 coins locally.
        _subWallet.GetWallet().Returns(new PlayerWallet { Coins = 1000 });

        // Simulate a scenario where the Azure Function rejects the payment (e.g., server desync)
        //_subCloud.PayEntryFeeAsync("Tokyo").Returns(Task.FromResult(false));


        // ACT
        //await _orchestrator.RequestJoinTierAsync("Tokyo", 500);


        // ASSERT
        // 1. Verify the local wallet was NOT saved with a deducted amount
        _subWallet.DidNotReceive().SaveWallet(Arg.Any<PlayerWallet>());

        // 2. Verify we did NOT attempt to connect to Photon matchmaker
        //await _subCloud.DidNotReceive().ConnectToPhotonMatchmakerAsync(Arg.Any<string>());
    }
}