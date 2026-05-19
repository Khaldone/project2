// Assets/_Project/3_Presentation/TrophyRoad/Components/TrophyTrackSnapper.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace Billiards.Presentation.TrophyRoad.Components
{
    /// <summary>
    /// Performance-isolated UI utility capturing drag momentum deltas to smoothly 
    /// anchor timeline tracks down onto precise item boundaries via DOTween projection logic.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public sealed class TrophyTrackSnapper : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [Header("Snapping Mechanics Settings")]
        [SerializeField] private float _snapDuration = 0.4f;
        [SerializeField] private Ease _snapEase = Ease.OutCubic;

        [Tooltip("Higher numbers project the glide further down the track timeline.")]
        [Range(0.01f, 0.4f)]
        [SerializeField] private float _flingInertiaFactor = 0.15f;

        private ScrollRect _scrollRect;
        private RectTransform _contentRect;

        private float _stepSize;
        private int _maxIndex;
        private Tween _snapTweener;
        private bool _isInitialized;

        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            if (_scrollRect != null)
            {
                _contentRect = _scrollRect.content;
            }
        }

        /// <summary>
        /// Updates sizing constraints sent down from the parent View layout controller.
        /// </summary>
        public void SetupSnappingDimensions(int totalMilestones, float nodeWidth, float spacing, int desiredVisibleCount)
        {
            _stepSize = nodeWidth + spacing;

            // Limit snapping bounds to prevent the track from scrolling past the final entry element footprint
            _maxIndex = Mathf.Max(0, totalMilestones - desiredVisibleCount);
            _isInitialized = _stepSize > 0;

            if (_scrollRect != null) _scrollRect.inertia = true;
        }

        /// <summary>
        /// NATIVE EVENT HANDLER: Triggered the exact frame contact begins.
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            // Interrupt any active snap animation instantly if a player catches the list mid-glide
            KillActiveSnap();
            if (_scrollRect != null) _scrollRect.inertia = true;
        }

        /// <summary>
        /// NATIVE EVENT HANDLER: Intercepts the touch release to calculate inertial landings.
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isInitialized || _scrollRect == null || _contentRect == null) return;

            // 1. Capture spatial position parameters in local anchoring coordinates
            float currentX = _contentRect.anchoredPosition.x;
            float velocityX = _scrollRect.velocity.x;

            // 2. Project landing trajectory based on momentum inputs
            float projectedLandingX = currentX + (velocityX * _flingInertiaFactor);

            // 3. Round to the nearest milestone node step index matching layout values
            int targetIndex = Mathf.RoundToInt(-projectedLandingX / _stepSize);
            targetIndex = Mathf.Clamp(targetIndex, 0, _maxIndex);

            // 4. Transform back into target pixel coordinates
            float targetAnchoredX = -targetIndex * _stepSize;

            // 5. Freeze native physics deceleration to hand control over to DOTween
            _scrollRect.inertia = false;
            _scrollRect.velocity = Vector2.zero;

            // 6. Execute smooth elastic glide animation sequence
            _snapTweener = _contentRect.DOAnchorPosX(targetAnchoredX, _snapDuration)
                .SetEase(_snapEase)
                .OnComplete(() =>
                {
                    // Restore manual inertial tracking functionality once alignment completes
                    _scrollRect.inertia = true;
                });
        }

        public void KillActiveSnap()
        {
            if (_snapTweener != null && _snapTweener.IsActive())
            {
                _snapTweener.Kill();
            }
        }

        private void OnDestroy()
        {
            KillActiveSnap();
        }
    }
}