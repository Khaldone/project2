using System;

namespace BilliardGame.UI.Notifications
{
    [Serializable]
    public class NotificationData
    {
        public string id;
        public string message;
        public NotificationType type;
        public NotificationPriority priority;
        public float displayDuration = 3;

        public NotificationData(string msg, NotificationType notificationType)
        {
            id = Guid.NewGuid().ToString();
            message = msg;
            type = notificationType;
            priority = NotificationPriority.Normal;
        }

        public NotificationData Clone()
        {
            var clone = new NotificationData(message, type)
            {
                priority = priority,
                displayDuration = displayDuration
            };

            return clone;
        }
    }
}
