using System.Collections;

namespace Animmal.NotificationSystem
{
    public interface INotificationSpawner
    {
        void Init(NotificationDisplay _NotificationDisplay);
        IEnumerator SpawnNotificationItem(NotificationStatus _NotificationStatus);
    }
}