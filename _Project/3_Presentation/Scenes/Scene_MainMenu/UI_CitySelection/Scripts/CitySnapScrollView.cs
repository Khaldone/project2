// Assets/_Project/3_Presentation/Shared/UI/CitySnapScrollView.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Billiards.CoreDomain.Progression;

namespace Billiards.Presentation.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public class CitySnapScrollView : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [Header("Scroll References")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;
        [SerializeField] private string _citySlotAddressableKey = "CitySlot";
        private List<RectTransform> _panels = new();
        private List<GameObject> _spawnedInstances = new();

        [Header("Scale & Zoom Settings")]
        [SerializeField, Range(0f, 2f)] private float _activeScale = 1.15f;
        [SerializeField, Range(0f, 1f)] private float _inactiveScale = 0.85f;

        [Header("Transition Settings")]
        [SerializeField] private float _snapDuration = 0.35f;
        [SerializeField] private Ease _snapEase = Ease.OutCirc;
        [SerializeField] private float _edgeBounceForce = 0.03f;
        [SerializeField] private float _swipeVelocityThreshold = 400f;

        private float[] _normalizedPositions;
        private Canvas[] _panelCanvases;
        private float _dynamicScaleThreshold;
        private int _currentIndex = 0;
        private Tween _snapTween;

        public event Action<int> OnCityFocused;

        private void Awake()
        {
            if (_scrollRect == null) _scrollRect = GetComponent<ScrollRect>();

            _scrollRect.inertia = true;
            _scrollRect.decelerationRate = 0.135f;

            // Do NOT calculate padding or positions here anymore. The Canvas isn't ready!
        }

        private void Start()
        {
            // Layout logic moved to PopulateCitiesAsync
            _scrollRect.onValueChanged.AddListener(UpdatePanelScales);
        }

        public async UniTask PopulateCitiesAsync(List<ArenaConfig> configs)
        {
            // Clean up existing instances if any
            ClearPanels();

            if (configs == null || configs.Count == 0) return;

            // Spawn panels dynamically
            foreach (var config in configs)
            {
                var instance = await Addressables.InstantiateAsync(_citySlotAddressableKey, _content).Task.AsUniTask();
                _spawnedInstances.Add(instance);
                
                var rect = instance.GetComponent<RectTransform>();
                _panels.Add(rect);

                var slotView = instance.GetComponentInChildren<CitySelection.CitySlotView>();
                if (slotView != null)
                {
                    slotView.RenderCityData(config.DisplayName, config.PrizePool, config.EntryFee);

                    Debug.Log($"[CitySnapScrollView] Spawning {config.ItemId} | IsPlayable struct property evaluates to: {config.IsPlayable}");
                    
                    slotView.SetLockState(!config.IsPlayable, config.RequiredCups, config.RequiredLevel);
                }
            }

            // 1. Force the Canvas Scaler to finish its job before we do any math
            Canvas.ForceUpdateCanvases();

            // 2. Now it is safe to calculate the exact center of the screen
            ApplyDynamicPadding();

            // 3. Map the positions (0.0 to 1.0) based on the new, correct width
            CalculatePositionsAndCaches();

            // 4. Force the initial visual scale
            UpdatePanelScales(new Vector2(_scrollRect.horizontalNormalizedPosition, 0));
        }

        private void ClearPanels()
        {
            foreach (var instance in _spawnedInstances)
            {
                if (instance != null) Addressables.ReleaseInstance(instance);
            }
            _spawnedInstances.Clear();
            _panels.Clear();
        }

        /// <summary>
        /// Mathematically guarantees the first and last city are dead-center, regardless of iPad or iPhone screen size.
        /// </summary>
        private void ApplyDynamicPadding()
        {
            if (_content == null || _scrollRect.viewport == null) return;

            var grid = _content.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                // Math: (Screen Center) - (Half of a City's Width) = The exact padding needed
                float viewportWidth = _scrollRect.viewport.rect.width;
                float cellWidth = grid.cellSize.x;

                int perfectPadding = Mathf.RoundToInt((viewportWidth / 2f) - (cellWidth / 2f));

                grid.padding.left = perfectPadding;
                grid.padding.right = perfectPadding;

                // Force Unity to build the layout right now so our position math below is flawless
                LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
            }
        }

        private void CalculatePositionsAndCaches()
        {
            int count = _panels.Count;
            _normalizedPositions = new float[count];
            _panelCanvases = new Canvas[count];

            for (int i = 0; i < count; i++)
            {
                _normalizedPositions[i] = count > 1 ? (float)i / (count - 1) : 0f;
                // Canvas lives on the child CityVisual_Panel, not on the spawned root.
                _panelCanvases[i] = _panels[i].GetComponentInChildren<Canvas>(true);
            }

            _dynamicScaleThreshold = count > 1 ? 1f / (count - 1) : 1f;
        }

        private void UpdatePanelScales(Vector2 scrollPos)
        {
            float currentX = scrollPos.x;

            for (int i = 0; i < _panels.Count; i++)
            {
                float distance = Mathf.Abs(currentX - _normalizedPositions[i]);
                float scaleLerp = Mathf.Clamp01(1f - (distance / _dynamicScaleThreshold));
                scaleLerp = Mathf.SmoothStep(0f, 1f, scaleLerp);

                float targetScale = Mathf.Lerp(_inactiveScale, _activeScale, scaleLerp);
                _panels[i].localScale = new Vector3(targetScale, targetScale, 1f);

                if (_panelCanvases[i] != null)
                {
                    // MasterCanvas is at sortingOrder 10; cards must sit above it or the
                    // Viewport's raycast-target Image swallows every click on the card.
                    _panelCanvases[i].sortingOrder = 20 + Mathf.RoundToInt(scaleLerp * 100);
                }
            }
        }

        // --- DRAG PHYSICS ---

        public void OnBeginDrag(PointerEventData eventData)
        {
            _snapTween?.Kill();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            float currentX = _scrollRect.horizontalNormalizedPosition;
            float velocityX = _scrollRect.velocity.x;

            _scrollRect.velocity = Vector2.zero;

            int targetIndex = _currentIndex;

            if (Mathf.Abs(velocityX) > _swipeVelocityThreshold)
            {
                if (velocityX < 0) targetIndex = Mathf.Min(_panels.Count - 1, _currentIndex + 1);
                else targetIndex = Mathf.Max(0, _currentIndex - 1);
            }
            else
            {
                float minDistance = float.MaxValue;
                for (int i = 0; i < _normalizedPositions.Length; i++)
                {
                    float distance = Mathf.Abs(currentX - _normalizedPositions[i]);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        targetIndex = i;
                    }
                }
            }

            SnapToIndex(targetIndex);
        }

        public void GoToNext()
        {
            if (_currentIndex >= _panels.Count - 1) PlayEdgeBounce(1f + _edgeBounceForce);
            else SnapToIndex(_currentIndex + 1);
        }

        public void GoToPrevious()
        {
            if (_currentIndex <= 0) PlayEdgeBounce(0f - _edgeBounceForce);
            else SnapToIndex(_currentIndex - 1);
        }

        private void SnapToIndex(int index)
        {
            _currentIndex = index;
            float targetNormPos = _normalizedPositions[index];

            _snapTween?.Kill();
            _snapTween = DOTween.To(
                () => _scrollRect.horizontalNormalizedPosition,
                x => _scrollRect.horizontalNormalizedPosition = x,
                targetNormPos,
                _snapDuration)
                .SetEase(_snapEase);

            OnCityFocused?.Invoke(_currentIndex);
        }

        private void PlayEdgeBounce(float targetStretch)
        {
            float originalPos = _normalizedPositions[_currentIndex];

            _snapTween?.Kill();
            Sequence bounceSeq = DOTween.Sequence();
            bounceSeq.Append(DOTween.To(() => _scrollRect.horizontalNormalizedPosition, x => _scrollRect.horizontalNormalizedPosition = x, targetStretch, _snapDuration * 0.4f).SetEase(Ease.OutQuad));
            bounceSeq.Append(DOTween.To(() => _scrollRect.horizontalNormalizedPosition, x => _scrollRect.horizontalNormalizedPosition = x, originalPos, _snapDuration * 0.8f).SetEase(Ease.OutElastic));
            _snapTween = bounceSeq;
        }

        public CitySelection.CitySlotView GetCitySlotView(int index)
        {
            if (index >= 0 && index < _panels.Count)
                return _panels[index].GetComponent<CitySelection.CitySlotView>();
            return null;
        }

        private void OnDestroy()
        {
            _scrollRect.onValueChanged.RemoveListener(UpdatePanelScales);
            _snapTween?.Kill();
            ClearPanels();
        }
    }
}