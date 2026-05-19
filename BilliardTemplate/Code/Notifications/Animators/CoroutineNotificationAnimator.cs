using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BilliardGame.UI.Notifications
{
    public class CoroutineNotificationAnimator : NotificationAnimatorBase
    {
        [Header("Timing (prefab-configurable)")]
        [SerializeField] private float slideInDuration = 0.5f;
        [SerializeField] private float slideOutDuration = 0.5f;
        [SerializeField] private float fadeInDuration = 0.25f;
        [SerializeField] private float fadeOutDuration = 0.25f;
        [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Coroutine running;
        private float displayDuration = 2f;

        public override void Show(NotificationData data, Action onComplete)
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(ShowRoutine(onComplete));
            displayDuration = data.displayDuration;
        }

        public override void Dismiss()
        {
            if (running != null)
            {
                StopCoroutine(running);
                running = null;
            }
        }

        private IEnumerator ShowRoutine(Action onComplete)
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            float t = 0f;
            while (t < fadeInDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            yield return new WaitForSeconds(displayDuration);

            t = 0f;
            while (t < fadeOutDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeOutDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;

            running = null;
            onComplete?.Invoke();
        }
    }
}