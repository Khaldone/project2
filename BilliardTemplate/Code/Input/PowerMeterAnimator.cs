using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ibc.game
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class PowerMeterAnimator : MonoBehaviour
    {
        [Header("Animation")] [SerializeField] private float _duration = 0.35f;
        [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float _offscreenOffsetX = 400f;
        [SerializeField] private bool _useUnscaledTime = true;

        [Header("Optional")] [SerializeField] private CanvasGroup _canvasGroup;

        RectTransform _rt;
        Vector2 _shownPos;
        Vector2 _hiddenPos;
        Coroutine _anim;

        void Awake()
        {
            _rt = GetComponent<RectTransform>();
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _shownPos = _rt.anchoredPosition;
            _hiddenPos = _shownPos + new Vector2(-Mathf.Abs(_offscreenOffsetX), 0f);
        }

        public void Show(bool immediate = false)
        {
            if (immediate)
            {
                Snap(_shownPos, 1f);
                return;
            }

            Play(_hiddenPos, _shownPos, 0f, 1f);
        }

        public void Hide(bool immediate = false)
        {
            if (immediate)
            {
                Snap(_hiddenPos, 0f);
                return;
            }

            Play(_shownPos, _hiddenPos, 1f, 0f);
        }

        void Play(Vector2 fromPos, Vector2 toPos, float fromA, float toA)
        {
            if (_anim != null) StopCoroutine(_anim);
            _anim = StartCoroutine(Anim(fromPos, toPos, fromA, toA));
        }

        IEnumerator Anim(Vector2 fromPos, Vector2 toPos, float fromA, float toA)
        {
            float t = 0f;
            _rt.anchoredPosition = fromPos;
            _canvasGroup.alpha = fromA;
            while (t < 1f)
            {
                t += (_useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) / Mathf.Max(0.0001f, _duration);
                float k = _curve.Evaluate(Mathf.Clamp01(t));
                _rt.anchoredPosition = Vector2.LerpUnclamped(fromPos, toPos, k);
                _canvasGroup.alpha = Mathf.LerpUnclamped(fromA, toA, k);
                yield return null;
            }

            _rt.anchoredPosition = toPos;
            _canvasGroup.alpha = toA;
            _anim = null;
        }

        void Snap(Vector2 pos, float alpha)
        {
            if (_anim != null) StopCoroutine(_anim);
            _rt.anchoredPosition = pos;
            _canvasGroup.alpha = alpha;
            _anim = null;
        }

#if UNITY_EDITOR
        [ContextMenu("Editor/Show (snap)")]
        void CtxShowSnap() => Show(true);

        [ContextMenu("Editor/Hide (snap)")]
        void CtxHideSnap() => Hide(true);
#endif
    }
}