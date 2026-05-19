// Assets/_Project/1_CoreDomain/Hardware/IPushNotificationService.cs
using System;
using System.Threading.Tasks;


public struct RemoteNotificationPayload
{
    public string Title;
    public string Body;
    public string DeepLinkAction; // e.g., "open_shop" or "join_tournament"
}


public interface IPushNotificationService_New
{
    Task<bool> InitializeAsync();
    string CurrentDeviceToken { get; }

    // An event for when the OS hands the app a notification while the game is actively running
    event Action<RemoteNotificationPayload> OnNotificationReceivedForeground;
}