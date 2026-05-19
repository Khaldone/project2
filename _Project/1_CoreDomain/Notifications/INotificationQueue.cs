// 2. Assets/_Project/CoreDomain/Notifications/INotificationQueue.cs
using System;
public interface INotificationQueue
{
    // The UI View will listen to this to know when to animate a popup
    event Action<NotificationData> OnShowNotification;
    
    // Fired when the active notification's color/classification changes mid-flight
    event Action<NotificationClassification> OnClassificationUpdated;

    // Fired when the active notification's message/title changes mid-flight
    event Action<string, string> OnMessageUpdated;

    // Fired when the caller explicitly requests the active notification to close (e.g. for StatusOverlay)
    event Action OnDismissRequested;

    // The UI View calls this when its exit animation finishes
    void NotifyDisplayFinished();

    /// <summary>
    /// Returns true if there are notifications waiting in the queue.
    /// </summary>
    bool HasPendingNotifications { get; }

    /// <summary>
    /// Called by the View after it subscribes to OnShowNotification.
    /// Signals that the notification system is ready to display popups,
    /// and flushes any notifications that were enqueued before the View existed.
    /// </summary>
    void NotifyViewReady();

    void Enqueue(NotificationData data);

    /// <summary>
    /// Updates the classification (color stripe) of the currently showing notification.
    /// Does nothing if no notification is currently displayed.
    /// </summary>
    void UpdateActiveClassification(NotificationClassification newClassification);

    /// <summary>
    /// Updates the text of the currently showing notification.
    /// Does nothing if no notification is currently displayed.
    /// </summary>
    void UpdateActiveMessage(string title, string message);

    /// <summary>
    /// Programmatically requests the currently showing notification to dismiss itself.
    /// Used primarily for StatusOverlay layouts that lack their own close button.
    /// </summary>
    void DismissActive();
}