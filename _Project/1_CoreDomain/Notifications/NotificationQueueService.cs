// 3. Assets/_Project/CoreDomain/Notifications/NotificationQueueService.cs
using System;
using System.Collections.Generic;

namespace Billiards.CoreDomain.Notifications
{
    public class NotificationQueueService : INotificationQueue
    {
        private readonly Queue<NotificationData> _queue = new Queue<NotificationData>();
        private bool _isCurrentlyShowing = false;
        
        public bool HasPendingNotifications => _queue.Count > 0;

        public event Action<NotificationData> OnShowNotification;
        public event Action<NotificationClassification> OnClassificationUpdated;
        public event Action<string, string> OnMessageUpdated;
        public event Action OnDismissRequested;

        /// <summary>
        /// Must be called by the View after it subscribes to OnShowNotification.
        /// Signals that the notification system is ready to display popups,
        /// and flushes any notifications that were enqueued before the View existed.
        /// </summary>
        public void NotifyViewReady()
        {
            UnityEngine.Debug.Log("[NotificationQueueService] View is ready. Flushing any pending notifications...");
            TryShowNext();
        }

        public void Enqueue(NotificationData data)
        {
            UnityEngine.Debug.Log($"[NotificationQueueService] Enqueued notification: {data.Title}. Queue count is now {_queue.Count + 1}");
            _queue.Enqueue(data);
            TryShowNext();
        }

        private void TryShowNext()
        {
            // If the screen is busy, or the queue is empty, do nothing.
            if (_isCurrentlyShowing)
            {
                UnityEngine.Debug.Log("[NotificationQueueService] Cannot show next: A notification is already showing.");
                return;
            }
            if (_queue.Count == 0)
            {
                UnityEngine.Debug.Log("[NotificationQueueService] Cannot show next: Queue is empty.");
                return;
            }
            // If no View is listening yet, keep the notification queued until NotifyViewReady() is called.
            if (OnShowNotification == null)
            {
                UnityEngine.Debug.Log("[NotificationQueueService] Cannot show next: No View is subscribed to OnShowNotification yet.");
                return;
            }

            // Otherwise, lock the screen and pop the next item
            _isCurrentlyShowing = true;
            NotificationData nextNotification = _queue.Dequeue();
            
            UnityEngine.Debug.Log($"[NotificationQueueService] Dequeued notification: {nextNotification.Title}. Firing OnShowNotification event!");

            OnShowNotification.Invoke(nextNotification);
        }

        public void NotifyDisplayFinished()
        {
            UnityEngine.Debug.Log("[NotificationQueueService] Display finished. Unlocking screen.");
            // The UI tells us it finished animating away. Unlock the screen.
            _isCurrentlyShowing = false;

            // Check if anyone else is waiting in line
            TryShowNext();
        }

        public void UpdateActiveClassification(NotificationClassification newClassification)
        {
            if (!_isCurrentlyShowing) return;
            OnClassificationUpdated?.Invoke(newClassification);
        }

        public void UpdateActiveMessage(string title, string message)
        {
            if (!_isCurrentlyShowing) return;
            OnMessageUpdated?.Invoke(title, message);
        }

        public void DismissActive()
        {
            if (!_isCurrentlyShowing) return;
            OnDismissRequested?.Invoke();
        }
    }
}