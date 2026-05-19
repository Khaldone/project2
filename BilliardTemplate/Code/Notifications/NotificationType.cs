using UnityEngine;

namespace BilliardGame.UI.Notifications
{
    /// <summary>
    /// Defines the type and behavior of notifications
    /// </summary>
    public enum NotificationType
    {
        Center,
        Corner,
    }

    /// <summary>
    /// Defines the priority of notifications (higher priority can interrupt lower)
    /// </summary>
    public enum NotificationPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }
}