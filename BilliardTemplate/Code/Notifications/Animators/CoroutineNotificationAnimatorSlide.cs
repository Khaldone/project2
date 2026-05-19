using System;
using System.Collections;
using UnityEngine;

namespace BilliardGame.UI.Notifications
{
    public class CoroutineNotificationAnimatorSlide : NotificationAnimatorBase
    {
        [Header("Timing (prefab-configurable)")]
        [SerializeField] private float slideInDuration = 0.5f;
        [SerializeField] private float slideOutDuration = 0.5f;
        [SerializeField] private float fadeInDuration = 0.25f;
        [SerializeField] private float fadeOutDuration = 0.25f;
        [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Coroutine running;
        private float displayDuration = 2f;
        private bool _dismissRequested;

        public override void Show(NotificationData data, Action onComplete)
        {
            // stop any running animation and reset dismiss flag
            _dismissRequested = false;
            if (running != null) StopCoroutine(running);
            displayDuration = Mathf.Max(0.01f, data.displayDuration);
            running = StartCoroutine(ShowRoutine(onComplete));
        }

        public override void Dismiss()
        {
            // request early dismissal; the running coroutine will handle the slide-out
            _dismissRequested = true;
        }

        private IEnumerator ShowRoutine(Action onComplete)
        {
            {
                if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
                if (rect == null) rect = GetComponent<RectTransform>();

                // Try to find the root canvas for a reliable full-canvas width
                Canvas canvas = view?.transform?.GetComponentInParent<Canvas>();
                float canvasWidth = 0f;
                if (canvas != null && canvas.rootCanvas != null)
                {
                    canvasWidth = canvas.rootCanvas.pixelRect.width;
                }
                else
                {
                    // fallback to screen width if no canvas found
                    canvasWidth = Screen.width;
                }

                // compute center anchored position and offscreen positions using canvas width
                Vector2 centerPos = rect.anchoredPosition;
                float offscreenX = (canvasWidth * 0.5f) + rect.rect.width;
                Vector2 leftPos = centerPos + Vector2.left * offscreenX;
                Vector2 rightPos = centerPos + Vector2.right * offscreenX;

                // start offscreen left and invisible
                rect.anchoredPosition = leftPos;
                canvasGroup.alpha = 0f;

                // slide in + fade in
                float t = 0f;
                while (t < slideInDuration && !_dismissRequested)
                {
                    t += Time.deltaTime;
                    float nt = Mathf.Clamp01(t / slideInDuration);
                    float e = ease.Evaluate(nt);
                    rect.anchoredPosition = Vector2.Lerp(leftPos, centerPos, e);

                    float fadeT = Mathf.Clamp01(t / Mathf.Max(0.0001f, fadeInDuration));
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, fadeT);

                    yield return null;
                }

                rect.anchoredPosition = centerPos;
                canvasGroup.alpha = 1f;

                // hold in center unless dismissal requested
                float elapsed = 0f;
                while (elapsed < displayDuration && !_dismissRequested)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // slide out to right + fade out
                t = 0f;
                Vector2 startPos = rect.anchoredPosition;
                while (t < slideOutDuration)
                {
                    t += Time.deltaTime;
                    float nt = Mathf.Clamp01(t / slideOutDuration);
                    float e = ease.Evaluate(nt);
                    rect.anchoredPosition = Vector2.Lerp(startPos, rightPos, e);

                    float fadeT = Mathf.Clamp01(t / Mathf.Max(0.0001f, fadeOutDuration));
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeT);

                    yield return null;
                }

                rect.anchoredPosition = rightPos;
                canvasGroup.alpha = 0f;

                running = null;
                _dismissRequested = false;
                onComplete?.Invoke();
            }
        }

        private IEnumerator FadeInHoldFadeOutFallback(Action onComplete)
        {
            // fallback simple fade if layout info missing
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
