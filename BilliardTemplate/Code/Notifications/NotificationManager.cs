using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BilliardGame.UI.Notifications
{
    public class NotificationManager : MonoBehaviour
    {
        private static NotificationManager instance;
        public static NotificationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<NotificationManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("NotificationManager");
                        instance = go.AddComponent<NotificationManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Prefabs (fallback)")]
        [SerializeField] private NotificationView notificationPrefab;

        [Header("Provider")]
        [SerializeField] private NotificationPrefabProvider prefabProvider;

        [Header("Containers")]
        [SerializeField] private Transform centerContainer;
        [SerializeField] private Transform cornerContainer;

        [Header("Settings")]
        [SerializeField] private int maxActiveNotifications = 3;

        [Header("Queue Settings")]
        [SerializeField] private bool allowInterruption = true;
        [SerializeField] private NotificationPriority minimumInterruptPriority = NotificationPriority.High;

        private Queue<NotificationData> notificationQueue = new Queue<NotificationData>();
        private List<NotificationView> activeNotifications = new List<NotificationView>();


        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePool();
        }

        private void InitializePool()
        {
            
        }

        private NotificationView CreateNotification(NotificationType type)
        {
            NotificationPrefabProvider.Entry entry = prefabProvider?.GetEntry(type);
            NotificationView prefabToUse = entry?.prefab ?? notificationPrefab;
            Transform parent = (type == NotificationType.Center)
                ? centerContainer
                : cornerContainer;

            NotificationView view = Instantiate(prefabToUse, parent, false);
            view.gameObject.SetActive(false);
            view.OnNotificationComplete += OnNotificationComplete;
            return view;
        }


        public void ShowNotification(NotificationData data)
        {
            if (data == null || string.IsNullOrEmpty(data.message))
            {
                throw new Exception("NotificationManager: Cannot show notification with empty message");
            }

            // Check if we should interrupt current notifications
            if (allowInterruption && data.priority >= minimumInterruptPriority)
            {
                var interruptible = activeNotifications
                    .Where(n => n.Priority < data.priority)
                    .ToList();

                foreach (var notification in interruptible)
                {
                    notification.Dismiss();
                }
            }

            // Try to show immediately if we have capacity
            if (activeNotifications.Count < maxActiveNotifications)
            {
                DisplayNotification(data);
            }
            else
            {
                notificationQueue.Enqueue(data);
            }
        }

        /// <summary>
        /// Build NotificationData from provider defaults (if available) and override message/duration/corner
        /// </summary>
        private NotificationData BuildDataFromPreset(NotificationType type, string message, float? displayDuration = null)
        {
            NotificationPrefabProvider.Entry entry = prefabProvider?.GetEntry(type);
            NotificationData data = new NotificationData(message, type);
            data.type = type;
            data.message = message;
            if (displayDuration.HasValue) data.displayDuration = displayDuration.Value;
            return data;
        }

        /// <summary>
        /// Convenience API: show by type and message; provider supplies prefab and defaults
        /// </summary>
        public void ShowByType(NotificationType type, string message, float? displayDuration = null, NotificationPriority? priority = null)
        {
            var data = BuildDataFromPreset(type, message, displayDuration);
            if (priority.HasValue) data.priority = priority.Value;
            ShowNotification(data);
        }
        
        public void ShowCorner(string message, float displayDuration = 3f)
        {
            ShowByType(NotificationType.Corner, message, displayDuration);
        }

        public void ShowCenter(string message, float displayDuration = 2f)
        {
            ShowByType(NotificationType.Center, message, displayDuration);
        }

        public void DismissAll()
        {
            var toRemove = activeNotifications.ToList();
            foreach (var notification in toRemove)
            {
                notification.Dismiss();
            }
        }

        public void ClearQueue()
        {
            notificationQueue.Clear();
        }

        private void DisplayNotification(NotificationData data)
        {
            NotificationView view = CreateNotification(data.type);

            if (data.type == NotificationType.Center)
            {
                view.transform.SetParent(centerContainer, false);
            }
            else
            {
                view.transform.SetParent(cornerContainer, false);
            }

            view.gameObject.SetActive(true);
            activeNotifications.Add(view);
            view.Show(data);
        }

        private void OnNotificationComplete(NotificationView view)
        {
            activeNotifications.Remove(view);
            Destroy(view.gameObject);
            
            // Show next queued notification if any
            if (notificationQueue.Count > 0)
            {
                var nextData = notificationQueue.Dequeue();
                DisplayNotification(nextData);
            }
        }

        public int GetActiveNotificationCount() => activeNotifications.Count;
        public int GetQueuedNotificationCount() => notificationQueue.Count;
    }
}
