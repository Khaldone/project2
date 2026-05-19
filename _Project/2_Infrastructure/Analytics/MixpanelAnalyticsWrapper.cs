// Assets/_Project/2_Infrastructure/Analytics/MixpanelAnalyticsWrapper.cs
using mixpanel; // The quarantined third-party namespace
using System.Collections.Generic;
using UnityEngine;


public class MixpanelAnalyticsWrapper : MonoBehaviour, IAnalyticsService
{
    public void Initialize()
    {
        // Mixpanel usually auto-initializes via a Unity Settings asset,
        // but we can put programmatic configurations here.
        Mixpanel.Identify(SystemInfo.deviceUniqueIdentifier);
        Debug.Log("AAA Pipeline: Mixpanel Analytics Initialized.");
    }


    public void SetUserIdentity(string playFabId)
    {
        // Alias the anonymous device ID to the authenticated PlayFab ID
        Mixpanel.Alias(playFabId);
        Mixpanel.Identify(playFabId);
    }


    public void TrackEvent(string eventName, Dictionary<string, object> properties = null)
    {
        var props = new Value();

        if (properties != null)
        {
            foreach (var kvp in properties)
            {
                // Box the C# objects into Mixpanel's proprietary 'Value' type
                props[kvp.Key] = kvp.Value.ToString();
            }
        }


        Mixpanel.Track(eventName, props);
    }


    public void SetUserProperty(string propertyName, object value)
    {
        Mixpanel.People.Set(propertyName, value.ToString());
    }
}
