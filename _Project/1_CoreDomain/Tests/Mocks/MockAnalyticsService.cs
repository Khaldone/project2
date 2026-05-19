// Assets/_Project/1_CoreDomain/Tests/Mocks/MockAnalyticsService.cs
using System.Collections.Generic;


public class MockAnalyticsService : IAnalyticsService
{
    // Public properties we can inspect during our tests
    public int TrackEventCallCount { get; private set; } = 0;
    public string LastEventTracked { get; private set; }
    public Dictionary<string, object> LastProperties { get; private set; }


    // We leave these blank because we don't care about testing them right now
    public void Initialize() { }
    public void SetUserIdentity(string playFabId) { }
    public void SetUserProperty(string propertyName, object value) { }


    // This is the method we want to wiretap
    public void TrackEvent(string eventName, Dictionary<string, object> properties = null)
    {
        TrackEventCallCount++;
        LastEventTracked = eventName;
        LastProperties = properties;
    }
}
