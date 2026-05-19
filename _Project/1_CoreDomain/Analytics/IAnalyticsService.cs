// Assets/_Project/1_CoreDomain/Analytics/IAnalyticsService.cs
using System.Collections.Generic;

public interface IAnalyticsService
{
    // Initialize the SDK (called by AppBootstrapper)
    void Initialize();


    // Identify the user (called after PlayFab login)
    void SetUserIdentity(string playFabId);


    // Track a generic event with optional parameters
    void TrackEvent(string eventName, Dictionary<string, object> properties = null);


    // Track a change in user state (e.g., "Total_Trophies" = 1500)
    void SetUserProperty(string propertyName, object value);
}