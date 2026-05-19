using System;

namespace BilliardGame.UI.Notifications
{
    public interface INotificationAnimator
    {
        void Initialize(NotificationView view);
        void Show(NotificationData data, Action onComplete);
        void Dismiss();
    }
}