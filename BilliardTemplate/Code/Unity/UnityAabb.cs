using System;
using System.Collections;
using ibc.objects;
using UnityEngine;

namespace ibc.unity
{
    public class UnityAabb : MonoBehaviour, IIdentifiable
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [Header("Identity & Bounds")] 
        public int Identifier;
        public Bounds Bounds;

        [Header("Gizmos")] 
        public Color WireColor = Color.yellow;
        public Color FillColor = new Color(1f, 1f, 0f, 0.08f);

        [Header("Quad")]
        public bool ShowQuad = true;
        public Material QuadMaterial;
        public Color QuadBaseColor = new Color(1f, 1f, 0f, 1f);

        [Range(0f, 1f)] public float DefaultShowOpacity = 0.20f;
        [Min(0f)] public float DefaultFadeDuration = 0.25f;
        public AnimationCurve Ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public bool UseUnscaledTime = false;

        private MeshRenderer _quadRenderer;
        private Transform _quadTransform;
        private Coroutine _fadeRoutine;
        private float CurrentOpacity => _quadRenderer.sharedMaterial.GetColor(BaseColorId).a;

        public int GetIdentifier() => Identifier;

        public Bounds GetWorldBounds()
        {
            var worldCenter = transform.TransformPoint(Bounds.center);
            var worldExtents = TransformExtentsToWorld(Bounds.extents);
            return new Bounds(worldCenter, worldExtents * 2f);
        }

        private Vector3 TransformExtentsToWorld(Vector3 localExtents)
        {
            var m = transform.localToWorldMatrix;
            var wx = m.MultiplyVector(Vector3.right) * localExtents.x;
            var wy = m.MultiplyVector(Vector3.up) * localExtents.y;
            var wz = m.MultiplyVector(Vector3.forward) * localExtents.z;
            return new Vector3(
                Mathf.Abs(wx.x) + Mathf.Abs(wy.x) + Mathf.Abs(wz.x),
                Mathf.Abs(wx.y) + Mathf.Abs(wy.y) + Mathf.Abs(wz.y),
                Mathf.Abs(wx.z) + Mathf.Abs(wy.z) + Mathf.Abs(wz.z)
            );
        }

        public bool ContainsPoint(Vector3 point, float radius = 0f)
        {
            var b = GetWorldBounds();
            b.Expand(radius * 2f);
            return b.Contains(point);
        }

        public Vector3 ClampPoint(Vector3 point, float radius = 0f)
        {
            var b = GetWorldBounds();
            b.Expand(-radius * 2f);
            return b.ClosestPoint(point);
        }

        public Vector3 GetClampOffset(Vector3 point, float radius = 0f)
        {
            var c = ClampPoint(point, radius);
            return c - point;
        }

        private void Awake()
        {
            if (!Application.isPlaying) return;

            EnsureQuad();
            UpdateQuadTransform();
            SetOpacityImmediate(0f);
            if (!ShowQuad) Hide(0f);
        }

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            EnsureQuad();
            UpdateQuadTransform();
            if (!ShowQuad) Hide(0f);
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying) return;
            if (_quadTransform != null)
                UpdateQuadTransform();
        }

        public void Show(float opacity = -1f, float duration = -1f)
        {
            if (!ShowQuad)
            {
                Hide(0f);
                return;
            }

            EnsureQuad();
            if (_quadRenderer == null) return;

            var target = (opacity >= 0f) ? Mathf.Clamp01(opacity) : Mathf.Clamp01(DefaultShowOpacity);
            var time = (duration >= 0f) ? Mathf.Max(0f, duration) : Mathf.Max(0f, DefaultFadeDuration);

            _quadRenderer.enabled = true;
            StartFade(CurrentOpacity, target, time, disableAfter: false);
        }

        public void Hide(float duration = -1f)
        {
            if (_quadRenderer == null) return;

            var time = (duration >= 0f) ? Mathf.Max(0f, duration) : Mathf.Max(0f, DefaultFadeDuration);
            StartFade(CurrentOpacity, 0f, time, disableAfter: true);
        }

        public void ShowImmediate(float opacity = -1f)
        {
            var target = (opacity >= 0f) ? Mathf.Clamp01(opacity) : Mathf.Clamp01(DefaultShowOpacity);
            EnsureQuad();
            if (_quadRenderer == null) return;
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _quadRenderer.enabled = true;
            SetOpacityImmediate(target);
        }

        public void HideImmediate()
        {
            if (_quadRenderer == null) return;
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            SetOpacityImmediate(0f);
            _quadRenderer.enabled = false;
        }

        private void StartFade(float from, float to, float duration, bool disableAfter)
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeRoutine(from, to, duration, disableAfter));
        }

        private IEnumerator FadeRoutine(float from, float to, float duration, bool disableAfter)
        {
            if (duration <= 0f)
            {
                SetOpacityImmediate(to);
                if (disableAfter && to <= 0f) _quadRenderer.enabled = false;
                _fadeRoutine = null;
                yield break;
            }

            float t = 0f;
            while (t < 1f)
            {
                var dt = UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                t = Mathf.Min(1f, t + (dt / duration));
                var eased = Ease != null ? Mathf.Clamp01(Ease.Evaluate(t)) : t;
                var val = Mathf.LerpUnclamped(from, to, eased);
                SetOpacityImmediate(val);
                yield return null;
            }

            if (disableAfter && to <= 0f)
                _quadRenderer.enabled = false;

            _fadeRoutine = null;
        }

        private void EnsureQuad()
        {
            if (_quadTransform == null)
            {
                _quadRenderer = transform.GetComponentInChildren<MeshRenderer>();
                if (_quadRenderer) _quadTransform = _quadRenderer.transform;
            }

            if (_quadTransform == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.name = "Quad";
                _quadTransform = go.transform;
                _quadTransform.SetParent(transform, false);

                _quadRenderer = go.GetComponent<MeshRenderer>();

                if (QuadMaterial != null)
                {
                    _quadRenderer.sharedMaterial = new Material(QuadMaterial);
                }
                else
                {
                    var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                    var mat = new Material(shader);
                    _quadRenderer.sharedMaterial = mat;
                }

#if UNITY_EDITOR
                // Remove collider added by CreatePrimitive in editor to avoid accidental physics
                var col = go.GetComponent<Collider>();
                if (col) DestroyImmediate(col);
#else
                var col = go.GetComponent<Collider>();
                if (col) Destroy(col);
#endif
                _quadRenderer.enabled = false;
            }
        }

        private void DestroyQuadIfAny()
        {
            if (_quadTransform == null) return;
            if (Application.isPlaying) Destroy(_quadTransform.gameObject);
            else DestroyImmediate(_quadTransform.gameObject);
            _quadTransform = null;
            _quadRenderer = null;
        }

        private void UpdateQuadTransform()
        {
            if (_quadTransform == null) return;

            var b = GetWorldBounds();
            var center = b.center;
            center.y = b.max.y;

            var size = b.size;
            var rot = Quaternion.Euler(90f, 0f, 0f);
            var scale = new Vector3(Mathf.Max(size.x, 0.001f), Mathf.Max(size.z, 0.001f), 1f);

            _quadTransform.position = center;
            _quadTransform.rotation = rot;
            _quadTransform.localScale = scale;
        }

        private void SetOpacityImmediate(float opacity01)
        {
            if (_quadRenderer == null) return;
            var c = QuadBaseColor;
            c.a = opacity01;
            _quadRenderer.sharedMaterial.SetColor(BaseColorId, c);
        }

        private void OnDrawGizmos()
        {
            var prevColor = Gizmos.color;
            var prevMatrix = Gizmos.matrix;

            Gizmos.matrix = Matrix4x4.identity;
            var b = GetWorldBounds();
            Gizmos.color = FillColor;
            Gizmos.DrawCube(b.center, b.size);
            Gizmos.color = WireColor;
            Gizmos.DrawWireCube(b.center, b.size);

            Gizmos.color = prevColor;
            Gizmos.matrix = prevMatrix;
        }

        private void OnDestroy()
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }

            if (_quadRenderer != null && _quadRenderer.sharedMaterial != null)
            {
                Destroy(_quadRenderer.sharedMaterial);
            }
        }

        [ContextMenu("Show (Default)")]
        private void CtxShowDefault() => Show();

        [ContextMenu("Hide (Default)")]
        private void CtxHideDefault() => Hide();

        [ContextMenu("Rebuild Quad")]
        private void CtxRebuild()
        {
            DestroyQuadIfAny();
            EnsureQuad();
            UpdateQuadTransform();
            SetOpacityImmediate(0f);
        }
    }
}