//using NUnit.Framework;
//using NSubstitute;
//using System.Threading.Tasks;


//public class StoreOrchestratorTests
//{
//    [Test]
//    public async Task BuyCoinPackAsync_WhenTransactionSucceeds_GrantsCoinsAndSyncs()
//    {
//        // ARRANGE
//        var mockPlatform = Substitute.For<IStorePlatform>();
//        var mockDataService = Substitute.For<IPlayerDataService>();


//        // Simulate a successful Apple/Google App Store charge
//        mockPlatform.ProcessRealMoneyPurchaseAsync("coin_pack_large")
//                    .Returns(Task.FromResult(true));


//        // orchestrator = new StoreOrchestrator(mockPlatform, mockDataService);


//        // ACT
//        //bool result = await orchestrator.BuyCoinPackAsync("coin_pack_large", 1000);


//        // ASSERT
//        //Assert.IsTrue(result);

//        // Verify the secure economy pipeline was followed exactly
//        Received.InOrder(() => {
//            mockDataService.AddCoins(1000);
//            mockDataService.ForceSyncAsync();
//        });
//    }


//    [Test]
//    public async Task BuyCoinPackAsync_WhenTransactionFails_BlocksReward()
//    {
//        // ARRANGE
//        var mockPlatform = Substitute.For<IStorePlatform>();
//        var mockDataService = Substitute.For<IPlayerDataService>();


//        // Simulate a declined card or user cancellation
//        mockPlatform.ProcessRealMoneyPurchaseAsync("coin_pack_large")
//                    .Returns(Task.FromResult(false));


//        //var orchestrator = new StoreOrchestrator(mockPlatform, mockDataService);


//        // ACT
//        //bool result = await orchestrator.BuyCoinPackAsync("coin_pack_large", 1000);


//        // ASSERT
//        //Assert.IsFalse(result);

//        // Mathematically prove the player did not receive free items
//        mockDataService.DidNotReceiveWithAnyArgs().AddCoins(default);
//        await mockDataService.DidNotReceiveWithAnyArgs().ForceSyncAsync(); //Added await.
//    }
//}
