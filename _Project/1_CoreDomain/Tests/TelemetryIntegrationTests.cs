//using NUnit.Framework;
//using NSubstitute;
//using System;
//using System.Threading.Tasks;


//public class TelemetryIntegrationTests
//{
//    [Test]
//    public async Task ForceSyncAsync_WhenBackendThrowsException_CapturesTelemetryAndRecovers()
//    {
//        // ARRANGE
//        var mockBackend = Substitute.For<ICloudSaveBackend>();
//        var mockTelemetry = Substitute.For<ITelemetryService>();

//        var simulatedNetworkCrash = new Exception("PlayFab Timeout Connection Terminated.");
//        //mockBackend.SaveProfileAsync(Arg.Any<PlayerProfileSave>()).Returns(Task.FromException<bool>(simulatedNetworkCrash));

//        // Assume we updated PlayerDataService to take ITelemetryService in its constructor
//        //var dataService = new PlayerDataService(mockBackend, mockTelemetry);
//        //dataService.AddCoins(100); // Make cache dirty


//        // ACT
//        // We await the sync. If the service doesn't try/catch properly, the test runner will fail.
//        //await dataService.ForceSyncAsync();


//        // ASSERT
//        // Verify the game didn't crash, and the telemetry service was alerted
//        mockTelemetry.Received(1).CaptureException(simulatedNetworkCrash);

//        // Verify the cache is still dirty because the save failed, preventing data loss
//        //Assert.IsTrue(dataService.IsDirty);
//    }
//}
