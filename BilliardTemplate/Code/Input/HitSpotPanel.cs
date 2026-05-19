using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ibc.game
{
    [DisallowMultipleComponent]
    public class HitSpotPanel : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler
    {
        [Header("Events")]
        public UnityEvent<float2> OffsetSetEvent;
        public UnityEvent OnShown;
        public UnityEvent OnHidden;
        public UnityEvent OnBeginDragEvt;
        public UnityEvent OnEndDragEvt;

        [Header("Dependencies")]
        [SerializeField] private CueStickController _cueStickController;

        [Header("UI")]
        [SerializeField] private RectTransform _ballRect;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Settings")]
        [SerializeField] private float _visualRadialOffset = 0f;
        [SerializeField] private float _fadeInDuration = 0.15f;
        [SerializeField] private float _fadeOutDuration = 0.12f;
        [SerializeField] private bool _blockRaycastsWhenVisible = true;
        [SerializeField] private bool _interactableWhenVisible = true;

        private float2 _offset;
        private Vector2 _startLocalPos;
        private float2 _startOffset;
        private bool _isDragging;
        private bool _isVisible;
        private Coroutine _fadeRoutine;
        private bool _dragEndedThisFrame;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponentInChildren<CanvasGroup>(true);
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void Start()
        {
            if (_cueStickController != null)
            {
                _offset = _cueStickController.GetOffset();
            }

            var shouldStartVisible = _canvasGroup.alpha > 0.9f;
            if (shouldStartVisible)
            {
                ApplyVisibleState(true);
            }
            else
            {
                ApplyVisibleState(false);
            }
        }

        private void LateUpdate()
        {
            _dragEndedThisFrame = false;
        }

        public bool IsVisible => _isVisible;

        public void Show(bool instant = false)
        {
            if (_isVisible && !instant) return;
            StartFade(1f, instant ? 0f : _fadeInDuration, true);
        }

        public void Hide(bool instant = false)
        {
            if (!_isVisible && !instant) return;
            StartFade(0f, instant ? 0f : _fadeOutDuration, false);
        }

        public void SetOffset(float2 value, bool notify = true)
        {
            var clamped = ClampToUnitDisk(value);
            _offset = clamped;

            if (_cueStickController != null)
                _cueStickController.SetOffset((Vector2)clamped);

            if (notify)
                OffsetSetEvent?.Invoke(clamped);
        }

        public float2 GetOffset() => _offset;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!TryLocalPoint(eventData, _ballRect, out var localPoint)) return;

            _startLocalPos = localPoint;
            _startOffset = _offset;
            _isDragging = true;
            OnBeginDragEvt?.Invoke();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            _dragEndedThisFrame = true;
            OnEndDragEvt?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            if (!TryLocalPoint(eventData, _ballRect, out var localPoint)) return;

            var radialSize = GetRadialSize();
            var delta = (localPoint - _startLocalPos) / radialSize;
            var proposed = (Vector2)_startOffset + delta;
            SetOffset(proposed);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging) return;
            if (_dragEndedThisFrame) return;

            if (eventData.pointerEnter == _ballRect.gameObject)
            {
                if (!TryLocalPoint(eventData, _ballRect, out var localPoint)) return;
                var radialSize = GetRadialSize();
                var normalized = localPoint / radialSize;
                SetOffset(normalized);
            }
            else
            {
                Hide();
            }
        }

        private float GetRadialSize()
        {
            return (_ballRect.rect.width - _visualRadialOffset) * 0.5f;
        }

        private static float2 ClampToUnitDisk(float2 v)
        {
            float magSq = math.lengthsq(v);
            if (magSq <= 1f) return v;
            var m = math.sqrt(magSq);
            return v / m;
        }

        private static bool TryLocalPoint(PointerEventData e, RectTransform rect, out Vector2 localPoint)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rect, e.position, e.pressEventCamera, out localPoint);
        }

        private void StartFade(float targetAlpha, float duration, bool becomingVisible)
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, duration, becomingVisible));
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration, bool becomingVisible)
        {
            if (becomingVisible)
                ApplyVisibleState(true);

            float start = _canvasGroup.alpha;
            float t = 0f;
            if (duration <= 0f)
            {
                _canvasGroup.alpha = targetAlpha;
            }
            else
            {
                while (t < duration)
                {
                    t += Time.unscaledDeltaTime;
                    float u = Mathf.Clamp01(t / duration);
                    float eased = u * u * (3f - 2f * u);
                    _canvasGroup.alpha = Mathf.Lerp(start, targetAlpha, eased);
                    yield return null;
                }
            }

            _canvasGroup.alpha = targetAlpha;

            if (!becomingVisible)
                ApplyVisibleState(false);

            _fadeRoutine = null;
        }

        private void ApplyVisibleState(bool visibleNow)
        {
            _isVisible = visibleNow;
            _canvasGroup.blocksRaycasts = visibleNow && _blockRaycastsWhenVisible;
            _canvasGroup.interactable = visibleNow && _interactableWhenVisible;
            _canvasGroup.alpha = visibleNow ? Mathf.Max(_canvasGroup.alpha, 1f) : Mathf.Min(_canvasGroup.alpha, 0f);

            if (visibleNow) OnShown?.Invoke();
            else OnHidden?.Invoke();
        }
    }
}