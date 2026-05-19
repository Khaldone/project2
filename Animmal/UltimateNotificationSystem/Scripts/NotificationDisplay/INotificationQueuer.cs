using System.Collections.Generic;

namespace Animmal.NotificationSystem
{
    public interface INotificationQueuer
    {
        UnityNotificationStatusEvent OnNotificationReady { get; }
        List<NotificationStatus> ActiveNotificationItems { get; }
        void Init(NotificationDisplay _NotificationDisplay);
        NotificationStatus AddToQueue(NotificationData _NotificationData);
        void OnDisplayEnabled();
        void OnDisplayDisabled();
        void ItemHidingFinished(NotificationStatus _NotificationStatus);
        void ClearQueues(int variationID);
    }
}