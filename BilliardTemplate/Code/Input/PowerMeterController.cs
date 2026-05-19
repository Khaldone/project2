using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ibc.game
{
    public class PowerMeterController : MonoBehaviour
    {
        public float Value => _slider.value;
        public UnityEvent<float> OnStrikeEvent;
        public UnityEvent OnSliderValueChanged; 

        [Header("Settings")] 
        [SerializeField] private Slider _slider;
        [SerializeField] private RectTransform _handleRt;
        [SerializeField] private float _springStiffness = 50f;
        [SerializeField] private float _cueStickDistanceMax = 0.25f;
        [SerializeField] private float _minimumStrikeVelocity = 0.05f;

        [Header("Material Properties")] 
        [SerializeField] private AnimationCurve _scrollTextureSpeedFactor;
        [SerializeField] private Gradient _fillGradient;
        [SerializeField] private Image _fillImage;
        [SerializeField] private RawImage _arrowsImage;
        [SerializeField] private float _scrollTextureSpeed = 4f;

        private float _timer;
        private bool _performStrike;
        private bool _isDragging;
        private float _velocity;
        private float _terminalVelocity;
        private Vector2 _offset;
        private float _previousSliderValue;

        private bool _isInteractable = true;

        private void Awake()
        {
            if (OnSliderValueChanged == null)
                OnSliderValueChanged = new UnityEvent();
            if (OnStrikeEvent == null)
                OnStrikeEvent = new UnityEvent<float>();
        }

        private void Start()
        {
            _previousSliderValue = _slider.value;
        }

        private void Update()
        {
            if (!_isDragging)
            {
                UpdateSpring();
            }

            UpdateSliderUI();
            UpdateMaterialProperties();
            
            if (Mathf.Abs(_slider.value - _previousSliderValue) > 0.001f)
            {
                _previousSliderValue = _slider.value;
                OnSliderValueChanged?.Invoke();
            }
        }

        private void UpdateSpring()
        {
            _velocity += Time.deltaTime * _springStiffness * _slider.value;
            _slider.value -= _velocity * Time.deltaTime;
            
            if (_performStrike)
            {
                if (_slider.value <= float.Epsilon)
                {
                    _performStrike = false;
                    OnStrikeEvent?.Invoke(_terminalVelocity);
                    _terminalVelocity = 0;
                }
            }
        }

        private void UpdateMaterialProperties()
        {
            var sliderValue = Value;
            _timer += Time.deltaTime * _scrollTextureSpeedFactor.Evaluate(sliderValue) * _scrollTextureSpeed;
            var rect = _arrowsImage.uvRect;
            rect.y = _timer;
            _arrowsImage.uvRect = rect;
            _fillImage.color = _fillGradient.Evaluate(sliderValue);
        }
        
        public float CalculateSpringTerminalVelocity(float springDistance)
        {
            return springDistance * math.sqrt(_springStiffness) * _cueStickDistanceMax;
        }

        public void OnPointerDown(BaseEventData eventData)
        {
            if (!_isInteractable)
                return;

            _isDragging = true;
            _terminalVelocity = 0f;
        }

        public void OnPointerUp(BaseEventData eventData)
        {
            if (!_isInteractable)
                return;

            if (_terminalVelocity > _minimumStrikeVelocity)
                _performStrike = true;

            _isDragging = false;
            _velocity = 0;
        }
        
        public void OnDrag(BaseEventData eventData)
        {
            if (!_isInteractable)
                return;

            if (_isDragging)
            {
                _terminalVelocity = CalculateSpringTerminalVelocity(Value);
            }
        }

        private void UpdateSliderUI()
        {
            RectTransform.Axis axis = RectTransform.Axis.Vertical;
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.one;
            anchorMin[(int) axis] = anchorMax[(int) axis] = 1 - Value;
            _handleRt.anchorMin = anchorMin;
            _handleRt.anchorMax = anchorMax;
        }
        
        public float GetValue()
        {
            return Value;
        }

        public float GetTerminalVelocity()
        {
            return _terminalVelocity;
        }
        
        public void SetInteractable(bool interactable)
        {
            _isInteractable = interactable;
            if (_slider != null)
                _slider.interactable = interactable;

            if (!interactable)
            {
                _isDragging = false;
                _performStrike = false;
                _velocity = 0;
                _terminalVelocity = 0;
            }
        }

        public void ResetSlider()
        {
            _slider.value = 0f;
            _velocity = 0f;
            _terminalVelocity = 0f;
            _performStrike = false;
            _isDragging = false;
            _previousSliderValue = 0f;
        }
    }
}