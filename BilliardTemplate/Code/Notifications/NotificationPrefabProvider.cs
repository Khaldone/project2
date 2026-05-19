using System;
using System.Linq;
using UnityEngine;

namespace BilliardGame.UI.Notifications
{
    [CreateAssetMenu(menuName = "Notifications/Prefab Provider")]
    public class NotificationPrefabProvider : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public NotificationType type;
            public NotificationView prefab;
        }

        public Entry[] entries;

        public Entry GetEntry(NotificationType type)
        {
            if (entries == null || entries.Length == 0) return null;
            return entries.FirstOrDefault(e => e.type == type);
        }
    }
}