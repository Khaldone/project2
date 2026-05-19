namespace Animmal.NotificationSystem
{
    public interface INotificationPool
    {
        void Init(NotificationDisplay notificationDisplay);
        NotificationItem RemoveFromPool(int _VariationID, NotificationDisplay _NotificationDisplay);
        void AddToPool(int _VariationID, NotificationItem _NotificationItem);
        bool HasPooledItem(int _VariationID);
        void ItemHidingFinished(NotificationStatus _NotificationStatus, NotificationDisplay _NotificationDisplay);
    }
}