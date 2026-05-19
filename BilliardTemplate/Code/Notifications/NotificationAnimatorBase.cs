using UnityEngine;

namespace BilliardGame.UI.Notifications
{
    public abstract class NotificationAnimatorBase : MonoBehaviour, INotificationAnimator
    {
        protected NotificationView view;
        protected CanvasGroup canvasGroup;
        protected RectTransform rect;

        public virtual void Initialize(NotificationView v)
        {
            view = v;
            canvasGroup = GetComponent<CanvasGroup>();
            rect = GetComponent<RectTransform>();
        }

        public abstract void Show(NotificationData data, System.Action onComplete);
        public abstract void Dismiss();
    }
}