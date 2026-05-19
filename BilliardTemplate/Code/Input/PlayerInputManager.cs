using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ibc.game
{
    public class PlayerInputManager : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private Transform _cueBall;
        [SerializeField] private CueStickController cueStickController;
        [SerializeField] private PowerMeterController powerMeter;

        [Header("Settings")] 
        [SerializeField] private float _tapTimeThreshold = 0.1f;
        [SerializeField] private float _tapDistanceThresholdPixels = 30;
        [SerializeField] private float _followPointerFactor = 0.1f;
        [SerializeField] private int _targetFrameRate = 90;

        private float2 _lastOrientFingerPos, _startOrientFingerPos;
        private int _orientCueFinger;
        private float _orientCueFingerTapTimer;
        private bool _enabled = true;

        private void Start()
        {
            Application.targetFrameRate = _targetFrameRate;
            _orientCueFinger = -1;

            // Subscribe to slider value changes to update cue distance
            if (powerMeter != null)
            {
                powerMeter.OnSliderValueChanged.AddListener(OnSliderValueChanged);
            }
        }

        private void OnDestroy()
        {
            if (powerMeter != null)
            {
                powerMeter.OnSliderValueChanged.RemoveListener(OnSliderValueChanged);
            }
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            if (!enabled)
                _orientCueFinger = -1;
        }

        private bool IsOrientFingerDown()
        {
            return _orientCueFinger >= 0;
        }

        private void OnFingerDown(Touch touch)
        {
            if (!_enabled) return;

#if !UNITY_EDITOR
            if (Input.touchCount != 1)
                return;
#endif

            if (!IsOrientFingerDown())
            {
                var mouseOverUI = IsPointerOverAnyUI(touch);

                if (!mouseOverUI)
                {
                    _lastOrientFingerPos = _startOrientFingerPos = touch.position;
                    _orientCueFinger = touch.fingerId;
                    _orientCueFingerTapTimer = Time.time;
                }
            }
        }

        private void OnFingerUp(Touch touch)
        {
            if (!_enabled) return;

            if (IsOrientFingerDown() && touch.fingerId == _orientCueFinger)
            {
                if (Time.time - _orientCueFingerTapTimer < _tapTimeThreshold &&
                    math.lengthsq((float2)touch.position - _startOrientFingerPos) <
                    _tapDistanceThresholdPixels * _tapDistanceThresholdPixels)
                {
                    OrientCueStickTowardsTouchPoint(touch.position);
                }

                _orientCueFinger = -1;
            }
        }

        private void OrientCueStickTowardsTouchPoint(float2 pointerPosition)
        {
            var screenPosV3 = new float3(pointerPosition.x, pointerPosition.y, 0);
            var mouseWorldPos = (float3)Camera.main.ScreenToWorldPoint(screenPosV3);

            var cueBallPosition = (float3)_cueBall.transform.position;
            var dir = mouseWorldPos.xz - cueBallPosition.xz;

            cueStickController?.OrientTowardsDirection(dir);
        }

        /// <summary>
        /// Called when slider value changes - updates cue stick distance
        /// </summary>
        private void OnSliderValueChanged()
        {
            if (cueStickController != null && powerMeter != null)
            {
                cueStickController.UpdatePowerDistance(powerMeter.Value);
            }
        }

        public void Update()
        {
            if (!_enabled) return;

#if UNITY_EDITOR
            for (int i = 0; i < 3; ++i)
            {
                var touch = new Touch();
                touch.position = ((float3)Input.mousePosition).xy;
                touch.fingerId = i;

                if (Input.GetMouseButtonDown(i))
                    OnFingerDown(touch);
                if (Input.GetMouseButtonUp(i))
                    OnFingerUp(touch);
            }
#else
            for (int i = 0; i < Input.touchCount; ++i)
            {
                var touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                    OnFingerDown(touch);
                if (touch.phase == TouchPhase.Ended)
                    OnFingerUp(touch);
            }
#endif

            if (_orientCueFinger >= 0)
            {
                Touch touch = default;
                for (int i = 0; i < Input.touchCount; ++i)
                {
                    touch = Input.GetTouch(i);
                    if (touch.fingerId == _orientCueFinger)
                        break;
                }

                var pointerPosition = (float2)touch.position;
#if UNITY_EDITOR
                pointerPosition = ((float3)Input.mousePosition).xy;
                touch.phase = TouchPhase.Moved;
#endif

                if (touch.fingerId == _orientCueFinger && touch.phase == TouchPhase.Moved)
                {
                    var cueBallPos = (float3)_cueBall.position;
                    var cueBallScreenPos = (float3)Camera.main.WorldToScreenPoint(cueBallPos);

                    var pointerToCueBallScreenDir = math.normalizesafe(cueBallScreenPos.xy - pointerPosition);
                    var lastPointerToCueBallScreenDir = math.normalizesafe(cueBallScreenPos.xy - _lastOrientFingerPos);

                    var rotateAmount = Vector2.SignedAngle(pointerToCueBallScreenDir, lastPointerToCueBallScreenDir);
                    var deltaYaw = rotateAmount * _followPointerFactor;

                    _lastOrientFingerPos = pointerPosition;

                    cueStickController?.RotateBy(deltaYaw);
                }
            }
        }

        public float GetCueStickJaw()
        {
            return cueStickController != null ? cueStickController.GetJaw() : 0f;
        }

        private static bool IsPointerOverAnyUI(Touch touch)
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = touch.position;
            List<RaycastResult> raycastResultsList = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResultsList);
            bool mouseOverUI = false;
            for (int i = 0; i < raycastResultsList.Count; i++)
            {
                if (raycastResultsList[i].gameObject != null)
                {
                    mouseOverUI = true;
                    break;
                }
            }

            return mouseOverUI;
        }
    }
}