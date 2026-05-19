//using NUnit.Framework;
//using NSubstitute;
//using System.Threading.Tasks;
//public class PlayerDataServiceTests
//{
//    [Test]
//    public async Task ForceSyncAsync_WhenProfileIsDirty_CallsCloudBackend()
//    {
//        // ARRANGE
//        var mockBackend = Substitute.For<ICloudSaveBackend>();
//        mockBackend.SaveProfileAsync(Arg.Any<PlayerProfileSave>()).Returns(Task.FromResult(true));

//        var dataService = new PlayerDataService(mockBackend);


//        // ACT
//        dataService.AddCoins(500); // This should flag _isDirty = true
//        await dataService.ForceSyncAsync();


//        // ASSERT
//        // Verify PlayFab was commanded to save exactly once
//        await mockBackend.Received(1).SaveProfileAsync(dataService.CurrentProfile);
//    }


//    [Test]
//    public async Task ForceSyncAsync_WhenProfileIsClean_IgnoresCloudBackend()
//    {
//        // ARRANGE
//        var mockBackend = Substitute.For<ICloudSaveBackend>();
//        var dataService = new PlayerDataService(mockBackend);


//        // ACT
//        // We do NOT add coins. The cache is clean.
//        await dataService.ForceSyncAsync();


//        // ASSERT
//        // Verify we saved API calls by not talking to PlayFab
//        await mockBackend.DidNotReceiveWithAnyArgs().SaveProfileAsync(default);
//    }
//}