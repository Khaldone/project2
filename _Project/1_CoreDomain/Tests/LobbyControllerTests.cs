//using NUnit.Framework;
//using NSubstitute;
//using System;
//using System.Threading;
//using System.Threading.Tasks;


//public class LobbyControllerTests
//{
//    [Test]
//    public async Task StartSearchAsync_WhenMatchFound_UpdatesUIAndLoadsScene()
//    {
//        // ARRANGE
//        var mockService = Substitute.For<IMatchmakingService>();
//        var mockUI = Substitute.For<ILobbyUIView>();
//        var mockLoader = Substitute.For<ISceneLoader>();


//        // Simulate a successful match returned by the server
//        var successResult = new MatchResult { Success = true, SessionName = "Room_123" };
//        mockService.RequestMatchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
//                   .Returns(Task.FromResult(successResult));


//        var controller = new LobbyController(mockService, mockUI, mockLoader);


//        // ACT
//        await controller.StartSearchAsync("Standard_8Ball");


//        // ASSERT
//        Received.InOrder(() => {
//            mockUI.EnableInteractability(false);
//            mockUI.ShowSearchingState("Calculating...");
//            mockUI.ShowMatchFoundState();
//            // Note: In a real test, you'd abstract Task.Delay to a time-provider,
//            // but for simplicity, we verify the scene load was requested.
//            mockLoader.LoadScene("Game_Arena");
//        });
//    }


//    [Test]
//    public async Task StartSearchAsync_WhenCancelled_ResetsUIGracefully()
//    {
//        // ARRANGE
//        var mockService = Substitute.For<IMatchmakingService>();
//        var mockUI = Substitute.For<ILobbyUIView>();
//        var mockLoader = Substitute.For<ISceneLoader>();


//        // Simulate a long-running task that gets cancelled
//        mockService.RequestMatchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
//            .Returns(async callInfo =>
//            {
//                var token = callInfo.Arg<CancellationToken>();
//                // Wait until the token is cancelled, then throw the standard exception
//                while (!token.IsCancellationRequested) { await Task.Yield(); }
//                token.ThrowIfCancellationRequested();
//                return new MatchResult();
//            });


//        var controller = new LobbyController(mockService, mockUI, mockLoader);


//        // ACT
//        // Start the search without awaiting it yet
//        var searchTask = controller.StartSearchAsync("Standard_8Ball");

//        // Immediately cancel it
//        controller.CancelSearch();


//        await searchTask;


//        // ASSERT
//        // Verify the cancellation block caught the exception and reset the UI
//        mockUI.Received().ShowDefaultState();
//        mockUI.Received().EnableInteractability(true);
//        mockLoader.DidNotReceiveWithAnyArgs().LoadScene(default);
//    }
//}
