// Assets/_Project/1_CoreDomain/Tests/AnalyticsOrchestratorTests.cs
using NUnit.Framework;
using System.Collections.Generic;


[TestFixture]
public class AnalyticsOrchestratorTests
{
    private MessageBroker _realMessageBroker;
    private MockAnalyticsService _mockAnalytics;
    private AnalyticsOrchestrator _orchestrator;


    [SetUp]
    public void Setup()
    {
        // ARRANGE: Set up the environment using pure C# objects (No Unity!)
        _realMessageBroker = new MessageBroker();
        _mockAnalytics = new MockAnalyticsService();

        // Inject our real broker and our fake analytics service into the Orchestrator
        //_orchestrator = new AnalyticsOrchestrator(_mockAnalytics, _realMessageBroker);


        // Simulate VContainer calling Start()
        _orchestrator.Start();
    }


    [TearDown]
    public void Teardown()
    {
        // Clean up our memory subscriptions to avoid test bleed
        _orchestrator.Dispose();
    }


    [Test]
    public void OnMatchFinished_WhenPublished_TracksMatchCompletedEvent()
    {
        //// ACT: We simulate the MatchCoordinator announcing a finished match
        //var message = new MatchFinishedMessage(
        //    DidLocalPlayerWin: true,
        //    WasBotMatch: false,
        //    MatchDurationSeconds: 120,
        //    ArenaTierName: "Tokyo",
        //    NewTotalLifetimeMatches: 42
        //);


        // Fire the message into the void
        //_realMessageBroker.Publish(message);


        // ASSERT: Did the Orchestrator translate the message into an analytics call?
        Assert.AreEqual(1, _mockAnalytics.TrackEventCallCount, "TrackEvent should be called exactly once.");
        Assert.AreEqual("Match_Completed", _mockAnalytics.LastEventTracked);


        // Verify the specific payload properties were parsed correctly
        Assert.IsTrue(_mockAnalytics.LastProperties.ContainsKey("Result"));
        Assert.AreEqual("Win", _mockAnalytics.LastProperties["Result"]);

        Assert.IsTrue(_mockAnalytics.LastProperties.ContainsKey("Tier"));
        Assert.AreEqual("Tokyo", _mockAnalytics.LastProperties["Tier"]);
    }
}
