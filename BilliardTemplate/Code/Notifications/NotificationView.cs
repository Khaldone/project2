using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

namespace BilliardGame.UI.Notifications
{
    [RequireComponent(typeof(CanvasGroup))]
    public class NotificationView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform containerRect;

        private INotificationAnimator animator;
        private NotificationData currentData;

        public event Action<NotificationView> OnNotificationComplete;
        public event Action<NotificationData> OnShown;
        public event Action<NotificationData> OnDismissed;
        public event Action<NotificationData> OnCompleted;

        public bool IsAnimating { get; private set; }
        public NotificationPriority Priority => currentData?.priority ?? NotificationPriority.Low;
        public NotificationType Type => currentData?.type ?? NotificationType.Center;
        public NotificationData CurrentData => currentData;

        private void Awake()
        {
            if (containerRect == null) containerRect = GetComponent<RectTransform>();
            animator = GetComponent<INotificationAnimator>();
            animator?.Initialize(this);
        }

        public void Show(NotificationData data)
        {
            currentData = data;
            if (messageText != null) messageText.text = data.message;
            OnShown?.Invoke(currentData);
            IsAnimating = true;

            if (animator != null)
            {
                animator.Show(data, () =>
                {
                    IsAnimating = false;
                    OnCompleted?.Invoke(currentData);
                    OnNotificationComplete?.Invoke(this);
                });
            }
            else
            {
                // fallback immediate complete
                IsAnimating = false;
                OnCompleted?.Invoke(currentData);
                OnNotificationComplete?.Invoke(this);
            }
        }

        public void Dismiss()
        {
            animator?.Dismiss();
            IsAnimating = false;
            OnDismissed?.Invoke(currentData);
            OnNotificationComplete?.Invoke(this);
        }
    }
}
