using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace ibc.game
{
    public class CueStickController : MonoBehaviour
    {
        public UnityEvent OnShow;
        public UnityEvent OnHide;
        public UnityEvent<float2> OnOffsetSet;
        
        [Header("References")]
        [Tooltip("Cue stick transform")]
        [SerializeField] private Transform _cueStick;

        [Header("Jaw Rotation")]
        [SerializeField] private float _jawInterpolateSpeed = 5f;

        [Header("Distance Positioning")]
        [SerializeField] private float _minDistance = 0.15f;
        [SerializeField] private float _maxDistance = 0.5f;
        [SerializeField] private float _distanceInterpolateSpeed = 8f;

        [Header("Strike Animation")]
        [SerializeField] private float _strikeForwardDuration = 0.1f; 
        [SerializeField] private float _strikeForwardDistance = 0.3f;

        [Header("Impact")]
        [Tooltip("Distance threshold used to detect cue tip impact with cue ball. Tune to your model.")]
        [SerializeField] private float _impactDistance = 0.02f;

        [Header("Post-Strike")]
        [SerializeField] private float _postStrikeDistance = 0.8f; 
        [SerializeField] private float _postStrikeFadeDuration = 0.3f;

        [Header("Turn Start")]
        [SerializeField] private float _turnStartFadeDuration = 0.3f;
        [SerializeField] private float _turnStartMoveDuration = 0.4f;

        [Header("Renderers")]
        [SerializeField] private Renderer[] _renderers;

        public event Action<float> OnStrikeImpact;

        private Vector3 _cueBallWorldPosition;
        private Material[] _materials;
        private Color[] _initialColors;
        private Coroutine _animCoroutine;

        // Jaw interpolation state
        private bool _interpolateJaw;
        private float _jawInterpTimer;
        private Quaternion _srcJaw;
        private Quaternion _destJaw;

        // Distance state
        private float _currentDistance;
        private float _targetDistance;
        private bool _isStriking;

        // Pending strike velocity (set when ExecuteStrike is called)
        private float _pendingStrikeVelocity;
        private bool _impactFired;

        private float2 _offset;

        private void Awake()
        {
            if (_cueStick == null)
            {
                Debug.LogWarning("CueStickAnimator: cue stick transform not assigned.");
                enabled = false;
                return;
            }

            _currentDistance = _minDistance;
            _targetDistance = _minDistance;
            
            if (_renderers == null || _renderers.Length == 0)
                _renderers = _cueStick.GetComponentsInChildren<Renderer>();

            _materials = new Material[_renderers.Length];
            _initialColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; ++i)
            {
                var r = _renderers[i];
                if (r == null) continue;
                var mat = new Material(r.material);
                r.material = mat;
                _materials[i] = mat;
                _initialColors[i] = mat.HasProperty("_Color") ? mat.color : Color.white;
            }
        }

        private void Update()
        {
            if (_interpolateJaw)
            {
                _jawInterpTimer += _jawInterpolateSpeed * Time.deltaTime;
                var t = Mathf.Clamp01(_jawInterpTimer);
                var curRot = Quaternion.Slerp(_srcJaw, _destJaw, t);
                SetJaw(curRot.eulerAngles.y);

                if (_jawInterpTimer >= 1f)
                {
                    _interpolateJaw = false;
                }
            }
            
            // Handle distance interpolation (only when not striking)
            if (!_isStriking && Mathf.Abs(_currentDistance - _targetDistance) > 0.001f)
            {
                _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance,
                    Time.deltaTime * _distanceInterpolateSpeed);
            }
            
            UpdateCuePosition();

        }

        /// <summary>
        /// Updates cue ball world position from physics state
        /// </summary>
        public void SetCueBallPosition(Vector3 pos)
        {
            _cueBallWorldPosition = pos;
            UpdateCuePosition();
        }

        /// <summary>
        /// Updates cue stick position based on cue ball position and current distance
        /// Distance is applied along local Z axis (forward/back from cue ball)
        /// </summary>
        private void UpdateCuePosition()
        {
            if (_cueStick == null) return;
            
            _cueStick.position = _cueBallWorldPosition;
            _cueStick.position -= _cueStick.forward * _currentDistance;
        }


        public void OrientTowardsDirection(Unity.Mathematics.float2 dir)
        {
            var angle = Unity.Mathematics.math.degrees(Unity.Mathematics.math.atan2(dir.y, dir.x));
            _srcJaw = Quaternion.Euler(0, GetJaw(), 0);
            _destJaw = Quaternion.AngleAxis(90 - angle, Vector3.up);
            _interpolateJaw = true;
            _jawInterpTimer = 0f;

        }

        public void RotateBy(float deltaYaw)
        {
            SetJaw(GetJaw() + deltaYaw);
        }

        public float GetJaw()
        {
            return _cueStick != null ? _cueStick.eulerAngles.y : 0f;
        }

        private void SetJaw(float jaw)
        {
            if (_cueStick == null) return;
            _cueStick.rotation = Quaternion.Euler(0, jaw, 0);
        }

        /// <summary>
        /// Update cue distance based on power (0-1).
        /// Called continuously while slider is being dragged.
        /// </summary>
        public void UpdatePowerDistance(float normalizedPower)
        {
            _targetDistance = Mathf.Lerp(_minDistance, _maxDistance, normalizedPower);
        }

        /// <summary>
        /// Execute strike animation: forward hit -> return -> move back -> fade out
        /// Now accepts strike velocity and raises OnStrikeImpact when cue reaches impact distance.
        /// </summary>
        public void ExecuteStrike(float strikeVelocity)
        {
            _pendingStrikeVelocity = strikeVelocity;
            if (_animCoroutine != null)
                StopCoroutine(_animCoroutine);
            _animCoroutine = StartCoroutine(StrikeAnimationSequence());
        }

        /// <summary>
        /// Fade in and move to ready position at turn start
        /// </summary>
        public void PrepareForTurn(Action onShow)
        {
            if (_animCoroutine != null)
                StopCoroutine(_animCoroutine);
            _animCoroutine = StartCoroutine(TurnStartAnimation(onShow));
        }

        /// <summary>
        /// Immediately hide the cue (for ball-in-hand or game end)
        /// </summary>
        public void Hide()
        {
            if (_animCoroutine != null)
            {
                StopCoroutine(_animCoroutine);
                _animCoroutine = null;
            }
            
            _animCoroutine = StartCoroutine(HideAnimationSequence());
        }
        
        private IEnumerator HideAnimationSequence()
        {
            _isStriking = true;
            _impactFired = false;
            float startDistance = _currentDistance;
            
            var elapsed = 0f;
            float moveDuration = 0.3f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);
                _currentDistance = Mathf.Lerp(startDistance, _postStrikeDistance, t);
                SetAlpha(Mathf.Lerp(1f, 0f, t));
                UpdateCuePosition();
                yield return null;
            }


            SetAlpha(0f);
            _isStriking = false;
            _animCoroutine = null;
            OnHide?.Invoke();
        }
        
        
        private IEnumerator StrikeAnimationSequence()
        {
            _isStriking = true;
            _impactFired = false;
            float startDistance = _currentDistance;

            // Phase 1: Strike forward (fast)
            float elapsed = 0f;
            float prevDistance = startDistance;
            while (elapsed < _strikeForwardDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _strikeForwardDuration);
                float newDistance = Mathf.Lerp(startDistance, _strikeForwardDistance, t);
                _currentDistance = newDistance;
                UpdateCuePosition();

                // Detect crossing of impact distance (handles both increasing and decreasing distance)
                if (!_impactFired)
                {
                    float a = prevDistance - _impactDistance;
                    float b = newDistance - _impactDistance;
                    if (a == 0f || b == 0f || (a > 0f && b < 0f) || (a < 0f && b > 0f))
                    {
                        _impactFired = true;
                        try
                        {
                            OnStrikeImpact?.Invoke(_pendingStrikeVelocity);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }

                prevDistance = newDistance;
                yield return null;
            }
            
            // Ensure impact fired if it wasn't during forward phase (fallback)
            if (!_impactFired)
            {
                _impactFired = true;
                try
                {
                    OnStrikeImpact?.Invoke(_pendingStrikeVelocity);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            elapsed = 0f;
            float moveDuration = 0.3f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);
                _currentDistance = Mathf.Lerp(startDistance, _postStrikeDistance, t);
                UpdateCuePosition();
                SetAlpha(Mathf.Lerp(1f, 0f, t));
                yield return null;
            }

            SetAlpha(0f);
            _isStriking = false;
            _animCoroutine = null;
            OnHide?.Invoke();
        }
        
        private IEnumerator TurnStartAnimation(Action onShow)
        {
            // Start from far back position
            _currentDistance = _postStrikeDistance;
            UpdateCuePosition();

            // Fade in and move to ready position simultaneously
            float elapsed = 0f;
            float maxDuration = Mathf.Max(_turnStartFadeDuration, _turnStartMoveDuration);
            
            while (elapsed < maxDuration)
            {
                elapsed += Time.deltaTime;
                
                // Fade in
                if (elapsed < _turnStartFadeDuration)
                {
                    float fadeT = elapsed / _turnStartFadeDuration;
                    SetAlpha(Mathf.SmoothStep(0f, 1f, fadeT));
                }
                else
                {
                    SetAlpha(1f);
                }

                // Move forward
                if (elapsed < _turnStartMoveDuration)
                {
                    float moveT = elapsed / _turnStartMoveDuration;
                    _currentDistance = Mathf.SmoothStep(_postStrikeDistance, _minDistance, moveT);
                    UpdateCuePosition();
                }
                else
                {
                    _currentDistance = _minDistance;
                    _targetDistance = _minDistance;
                    UpdateCuePosition();
                }

                yield return null;
            }

            SetAlpha(1f);
            _currentDistance = _minDistance;
            _targetDistance = _minDistance;
            UpdateCuePosition();
            _animCoroutine = null;
            OnShow?.Invoke();
            onShow?.Invoke();
        }
        
        private void SetAlpha(float alpha)
        {
            if (_materials == null) return;

            for (int i = 0; i < _materials.Length; ++i)
            {
                if (_materials[i] == null) continue;
                var c = _initialColors[i];
                c.a = alpha;
                _materials[i].color = c;
            }
        }
        
        public float2 GetOffset()
        {
            return _offset;
        }

        public void SetOffset(float2 offset)
        {
            if (math.lengthsq(_offset - offset) > 1E-4f)
            {
                _offset = offset;
                if (math.lengthsq(_offset) > 1)
                    _offset = math.normalizesafe(offset);
                OnOffsetSet?.Invoke(_offset);
            }

        }

        public void ResetOffset()
        {
            SetOffset(0);
        }
    }
}
