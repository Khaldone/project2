using NUnit.Framework;
using NSubstitute;
using System.Threading.Tasks;
using System;


public class NetworkOrchestratorTests
{
    [Test]
    public async Task JoinMatch_WhenConnectionFails_FiresFailureEvent()
    {
        // ARRANGE
        var mockMatchmaking = Substitute.For<INetMatchmakingService>();
        mockMatchmaking.JoinOrCreateMatchAsync(Arg.Any<string>()).Returns(Task.FromResult(false));


        //var orchestrator = new NetworkOrchestrator(mockMatchmaking);
        bool failureEventFired = false;
        //orchestrator.OnNetworkFailure += () => failureEventFired = true;


        // ACT
       // await orchestrator.AttemptJoinAsync("Room_123");


        // ASSERT
        Assert.IsTrue(failureEventFired);
    }
}
