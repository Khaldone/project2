namespace Animmal.NotificationSystem
{
    public interface INotificationDisplayer
    {
        void Init(NotificationDisplay _NotificationDisplay);
        void DisplayNotification(NotificationStatus _NotificationStatus);

    }
}