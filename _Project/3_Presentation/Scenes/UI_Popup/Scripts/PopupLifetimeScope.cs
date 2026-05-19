using UnityEngine;
using VContainer;
using VContainer.Unity;
using Billiards.CoreDomain.Notifications;
public class PopupLifetimeScope : LifetimeScope
{
    [Header("Global UI")]
    [SerializeField] private GlobalNotificationView _notificationView;

    protected override void Configure(IContainerBuilder builder)
    {
        // Notification Queue + Router (pure C# services)
        builder.Register<NotificationQueueService>(Lifetime.Singleton).As<INotificationQueue>();
        builder.RegisterEntryPoint<NotificationRouter>();

        // Notification View (MonoBehaviour living in this scene)
        if (_notificationView != null)
        {
            builder.RegisterComponent(_notificationView);
        }
        else
        {
            Debug.LogWarning("[PopupLifetimeScope] GlobalNotificationView is not assigned in the Inspector!");
        }
    }
}
