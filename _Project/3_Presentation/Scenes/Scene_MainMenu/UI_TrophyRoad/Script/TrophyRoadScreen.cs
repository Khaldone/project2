// Assets/_Project/3_Presentation/TrophyRoad/TrophyRoadScreen.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Billiards.Core.Progression;
using Billiards.Presentation.TrophyRoad.Widgets;
using Billiards.Presentation.TrophyRoad.Pools;
using Billiards.Presentation.TrophyRoad.Components;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Billiards.Presentation.TrophyRoad
{
    /// <summary>
    /// Highly optimized, zero-allocation presentation view layer managing the Trophy Road UI layout panel.
    /// Features frame-accurate progress bar tracking, inertial snapping metrics, and bidirectional shortcuts.
    /// </summary>
    public sealed class TrophyRoadScreen : SsBaseMenu, ITrophyRoadView
    {
        public event Action<string> OnClaimNodeClicked;

        [Header("Track Viewport Layout Containers")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private TrophyTrackSnapper _trackSnapper;
        [SerializeField] private RectTransform _contentContainer;
        [SerializeField] private RectTransform _viewportRect;
        [SerializeField] private HorizontalLayoutGroup _layoutGroup;
        [SerializeField] private Slider _trackProgressBar;
        [SerializeField] private CanvasGroup _interactionBlockerGroup;
        [SerializeField] private GameObject _nodePrefab;

        [Header("Navigation Shortcut Components (Left / Return Back)")]
        [SerializeField] private Button _returnToStartButton;
        [SerializeField] private CanvasGroup _returnToStartCanvasGroup;

        [Header("Navigation Shortcut Components (Right / Jump Forward)")]
        [SerializeField] private Button _jumpForwardButton;
        [SerializeField] private CanvasGroup _jumpForwardCanvasGroup;

        [Tooltip("Pixel distance scrolled past your progress position before a shortcut button fades in.")]
        [SerializeField] private float _scrollThresholdToSubmenu = 300f;

        [Header("Sizing Configuration Control")]
        [Range(1, 6)]
        [SerializeField] private int _desiredVisibleNodesCount = 3;

        [Header("Juice: Slider Animation Settings")]
        [SerializeField] private float _fillAnimationDuration = 0.8f;
        [SerializeField] private Ease _fillAnimationEase = Ease.OutCubic;

        private readonly Dictionary<string, TrophyNodeWidget> _activeNodesMap = new();
        private IReadOnlyList<TrophyMilestone> _cachedMilestonesList;
        private TrophyNodePool _nodeRecyclerPool;
        private bool _isFirstLayoutRender = true;

        private Tween _leftButtonFadeTweener;
        private Tween _rightButtonFadeTweener;

        private float _lastClaimedAnchoredX = 0f;
        private int _cachedActiveCups = 0;

        // Allocation Protection: Pre-allocated spatial arrays to bypass runtime Garbage Collection spikes
        private readonly Vector3[] _viewportWorldCorners = new Vector3[4];
        private readonly Vector3[] _nodeWorldCorners = new Vector3[4];

        [Header("Zero-Allocation Effects Engine")]
        [SerializeField] private ParticleSystem _burstParticleSystem;
        [SerializeField] private int _burstParticleCount = 35;

        public override void Awake()
        {
            base.Awake();
            if (_nodePrefab != null && _contentContainer != null)
            {
                _nodeRecyclerPool = new TrophyNodePool(_nodePrefab, _contentContainer, initialPrewarmCapacity: 12);
            }

            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.AddListener(HandleScrollUpdate);
            }

            if (_returnToStartButton != null)
            {
                _returnToStartButton.onClick.AddListener(HandleFocusOnActiveProgressClicked);
            }
            if (_jumpForwardButton != null)
            {
                _jumpForwardButton.onClick.AddListener(HandleFocusOnActiveProgressClicked);
            }

            InitializeShortcutCanvasGroup(_returnToStartCanvasGroup);
            InitializeShortcutCanvasGroup(_jumpForwardCanvasGroup);
        }

        private void InitializeShortcutCanvasGroup(CanvasGroup group)
        {
            if (group == null) return;
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        private void OnDestroy()
        {
            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.RemoveListener(HandleScrollUpdate);
            }
            if (_returnToStartButton != null)
            {
                _returnToStartButton.onClick.RemoveListener(HandleFocusOnActiveProgressClicked);
            }
            if (_jumpForwardButton != null)
            {
                _jumpForwardButton.onClick.RemoveListener(HandleFocusOnActiveProgressClicked);
            }
        }

        private void OnDisable()
        {
            _isFirstLayoutRender = true;
        }

        private void OnValidate()
        {
            RefreshNodeLayout();
        }

        private void HandleScrollUpdate(Vector2 normalPos)
        {
            EvaluateViewportCullingPass();
            EvaluateNavigationButtonVisibility();
        }

        private void EvaluateNavigationButtonVisibility()
        {
            if (_contentContainer == null) return;

            float currentX = _contentContainer.anchoredPosition.x;

            bool shouldShowLeft = currentX < (_lastClaimedAnchoredX - _scrollThresholdToSubmenu);
            UpdateShortcutTweenState(_returnToStartCanvasGroup, shouldShowLeft, ref _leftButtonFadeTweener);

            bool shouldShowRight = currentX > (_lastClaimedAnchoredX + _scrollThresholdToSubmenu);
            UpdateShortcutTweenState(_jumpForwardCanvasGroup, shouldShowRight, ref _rightButtonFadeTweener);
        }

        private void UpdateShortcutTweenState(CanvasGroup group, bool shouldBeVisible, ref Tween cachedTweener)
        {
            if (group == null) return;

            float targetAlpha = shouldBeVisible ? 1f : 0f;
            if (Mathf.Approximately(group.alpha, targetAlpha)) return;

            cachedTweener?.Kill();

            group.blocksRaycasts = shouldBeVisible;
            group.interactable = shouldBeVisible;

            cachedTweener = group.DOFade(targetAlpha, 0.25f).SetEase(Ease.OutCubic);
        }

        private void HandleFocusOnActiveProgressClicked()
        {
            if (_contentContainer == null) return;

            if (_trackSnapper != null) _trackSnapper.KillActiveSnap();
            if (_scrollRect != null) _scrollRect.velocity = Vector2.zero;

            _contentContainer.DOKill();
            _contentContainer.DOAnchorPosX(_lastClaimedAnchoredX, 0.5f)
                .SetEase(_fillAnimationEase)
                .OnUpdate(HandleScrollUpdateDependentRoutines);
        }

        private void EvaluateViewportCullingPass()
        {
            if (_viewportRect == null || _nodeRecyclerPool == null || _nodeRecyclerPool.ActiveNodes.Count == 0) return;

            _viewportRect.GetWorldCorners(_viewportWorldCorners);
            float viewLeftLimitX = _viewportWorldCorners[0].x;
            float viewRightLimitX = _viewportWorldCorners[2].x;

            var activeNodesList = _nodeRecyclerPool.ActiveNodes;
            for (int i = 0; i < activeNodesList.Count; i++)
            {
                var widget = activeNodesList[i];
                if (widget == null) continue;

                widget.CachedRectTransform.GetWorldCorners(_nodeWorldCorners);
                float nodeLeftLimitX = _nodeWorldCorners[0].x;
                float nodeRightLimitX = _nodeWorldCorners[2].x;

                bool isVisible = (nodeRightLimitX >= viewLeftLimitX) && (nodeLeftLimitX <= viewRightLimitX);
                widget.SetCullingState(!isVisible);
            }
        }

        public async UniTask RenderTrack(IReadOnlyList<TrophyMilestone> milestones, int activeCups, Func<string, UniTask<Sprite>> iconProvider)
        {
            _cachedMilestonesList = milestones;
            _cachedActiveCups = activeCups;
            _activeNodesMap.Clear();

            if (_nodeRecyclerPool != null) _nodeRecyclerPool.RecycleAllActiveNodes();
            if (milestones == null || milestones.Count == 0) return;

            if (_trackProgressBar != null)
            {
                _trackProgressBar.minValue = 0f;
                _trackProgressBar.maxValue = milestones.Count;
            }

            float calculatedNodeWidth = CalculateOptimalNodeWidth();

            for (int i = 0; i < milestones.Count; i++)
            {
                var m = milestones[i];
                var widget = _nodeRecyclerPool.SpawnNode();

                if (widget != null)
                {
                    if (widget.TryGetComponent<LayoutElement>(out var layoutElement))
                    {
                        layoutElement.preferredWidth = calculatedNodeWidth;
                    }

                    widget.Setup(m, iconProvider, _scrollRect, () => OnClaimNodeClicked?.Invoke(m.TaskId));
                    _activeNodesMap.Add(m.TaskId, widget);
                }
            }

            RefreshNodeLayout();

            bool shouldAnimate = !_isFirstLayoutRender;
            _isFirstLayoutRender = false;

            await UpdateProgressFill(activeCups, shouldAnimate);
            EvaluateViewportCullingPass();
            EvaluateNavigationButtonVisibility();
        }

        /// <summary>
        /// Animates the progress fill line and continuously positions the camera view over the fill tip on every frame.
        /// </summary>
        public async UniTask UpdateProgressFill(int activeCups, bool animate)
        {
            if (_trackProgressBar == null || _cachedMilestonesList == null || _cachedMilestonesList.Count == 0) return;

            float normalizedValue = 0f;
            if (activeCups < _cachedMilestonesList[0].RequiredCups)
            {
                float t = (float)activeCups / _cachedMilestonesList[0].RequiredCups;
                normalizedValue = t * 0.5f;
            }
            else
            {
                int currentSegmentIndex = 0;
                for (int i = 0; i < _cachedMilestonesList.Count - 1; i++)
                {
                    if (activeCups >= _cachedMilestonesList[i].RequiredCups && activeCups < _cachedMilestonesList[i + 1].RequiredCups)
                    {
                        currentSegmentIndex = i;
                        break;
                    }
                    currentSegmentIndex = i;
                }

                if (currentSegmentIndex < _cachedMilestonesList.Count - 1)
                {
                    int lowerCups = _cachedMilestonesList[currentSegmentIndex].RequiredCups;
                    int upperCups = _cachedMilestonesList[currentSegmentIndex + 1].RequiredCups;
                    float segmentProgress = (float)(activeCups - lowerCups) / (upperCups - lowerCups);
                    normalizedValue = (currentSegmentIndex + 0.5f) + segmentProgress;
                }
                else
                {
                    normalizedValue = _cachedMilestonesList.Count;
                }
            }

            _trackProgressBar.DOKill();
            _contentContainer.DOKill();

            if (animate)
            {
                // Hook into OnUpdate to keep the camera glued to the filling bar tip in real-time
                await _trackProgressBar.DOValue(normalizedValue, _fillAnimationDuration)
                    .SetEase(_fillAnimationEase)
                    .OnUpdate(UpdateCameraToCenterSliderValue)
                    .ToUniTask(TweenCancelBehaviour.Kill, this.GetCancellationTokenOnDestroy());
            }
            else
            {
                _trackProgressBar.value = normalizedValue;
                UpdateCameraToCenterSliderValue();
                await UniTask.CompletedTask;
            }
        }

        /// <summary>
        /// Mathematically calculates the exact horizontal position required to center the progress tip on screen.
        /// </summary>
        private void UpdateCameraToCenterSliderValue()
        {
            if (_contentContainer == null || _viewportRect == null || _layoutGroup == null || _trackProgressBar == null) return;

            float targetWidth = CalculateOptimalNodeWidth();
            float nodeSpacing = _layoutGroup.spacing;
            float paddingLeft = _layoutGroup.padding.left;
            float viewportWidth = _viewportRect.rect.width;

            // Compute exact local X pixel coordinate of the progress fill tip
            float currentProgressLocalX = paddingLeft + (_trackProgressBar.value * (targetWidth + nodeSpacing));

            // Shift the anchor to keep this exact progress point perfectly framed in the center of the viewport box
            float targetAnchoredX = -(currentProgressLocalX - (viewportWidth * 0.5f));

            // Clamp positions to protect the layout boundaries against over-scrolling
            float contentWidth = _contentContainer.rect.width;
            if (contentWidth > viewportWidth)
            {
                targetAnchoredX = Mathf.Clamp(targetAnchoredX, -(contentWidth - viewportWidth), 0f);
            }
            else
            {
                targetAnchoredX = 0f;
            }

            _contentContainer.anchoredPosition = new Vector2(targetAnchoredX, _contentContainer.anchoredPosition.y);

            HandleScrollUpdateDependentRoutines();
        }

        private void HandleScrollUpdateDependentRoutines()
        {
            EvaluateViewportCullingPass();
            EvaluateNavigationButtonVisibility();
        }

        private void RefreshNodeLayout()
        {
            if (_contentContainer == null || _viewportRect == null || _layoutGroup == null) return;
            float targetWidth = CalculateOptimalNodeWidth();
            float nodeSpacing = _layoutGroup.spacing;

            if (_nodeRecyclerPool != null && _nodeRecyclerPool.ActiveNodes.Count > 0)
            {
                foreach (var widget in _nodeRecyclerPool.ActiveNodes)
                {
                    if (widget.TryGetComponent<LayoutElement>(out var layoutElement))
                    {
                        layoutElement.preferredWidth = targetWidth;
                    }
                }
            }
            else
            {
                foreach (Transform child in _contentContainer)
                {
                    if (child.TryGetComponent<LayoutElement>(out var layoutElement) && !layoutElement.ignoreLayout)
                    {
                        layoutElement.preferredWidth = targetWidth;
                    }
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_contentContainer);

            if (_cachedMilestonesList != null && _cachedMilestonesList.Count > 0)
            {
                int targetProgressIndex = 0;

                for (int i = 0; i < _cachedMilestonesList.Count; i++)
                {
                    if (_cachedActiveCups >= _cachedMilestonesList[i].RequiredCups ||
                        _cachedMilestonesList[i].State == MilestoneState.Claimed ||
                        _cachedMilestonesList[i].State == MilestoneState.Claimable)
                    {
                        targetProgressIndex = i;
                    }
                }

                float viewportWidth = _viewportRect.rect.width;
                float paddingLeft = _layoutGroup.padding.left;

                float nodeCenterLocalX = paddingLeft + (targetProgressIndex * (targetWidth + nodeSpacing)) + (targetWidth * 0.5f);
                _lastClaimedAnchoredX = -(nodeCenterLocalX - (viewportWidth * 0.5f));

                float contentWidth = _contentContainer.rect.width;
                if (contentWidth > viewportWidth)
                {
                    _lastClaimedAnchoredX = Mathf.Clamp(_lastClaimedAnchoredX, -(contentWidth - viewportWidth), 0f);
                }
                else
                {
                    _lastClaimedAnchoredX = 0f;
                }
            }

            if (_trackSnapper != null && _cachedMilestonesList != null)
            {
                _trackSnapper.SetupSnappingDimensions(_cachedMilestonesList.Count, targetWidth, _layoutGroup.spacing, _desiredVisibleNodesCount);
            }

            EvaluateViewportCullingPass();
        }

        private float CalculateOptimalNodeWidth()
        {
            if (_viewportRect == null || _layoutGroup == null) return 200f;
            float viewportWidth = _viewportRect.rect.width;
            if (viewportWidth <= 0f) viewportWidth = 1920f;

            float totalPadding = _layoutGroup.padding.left + _layoutGroup.padding.right;
            float totalSpacing = (_desiredVisibleNodesCount - 1) * _layoutGroup.spacing;

            float targetWidth = (viewportWidth - totalPadding - totalSpacing) / _desiredVisibleNodesCount;
            return Mathf.Max(50f, targetWidth);
        }

        private void OnRectTransformDimensionsChange()
        {
            RefreshNodeLayout();
        }

        public void UpdateNodeState(string taskId, MilestoneState newState)
        {
            if (_activeNodesMap.TryGetValue(taskId, out var widget))
            {
                widget.UpdateVisualState(newState);
            }
        }

        public void SetInteractionLock(bool isLocked)
        {
            if (_interactionBlockerGroup != null)
            {
                _interactionBlockerGroup.blocksRaycasts = true;
                _interactionBlockerGroup.interactable = !isLocked;
                _interactionBlockerGroup.alpha = isLocked ? 0.6f : 1.0f;
            }
        }

        public void PlayBurstEffect(string taskId)
        {
            if (_burstParticleSystem == null) return;

            if (_activeNodesMap.TryGetValue(taskId, out var widget))
            {
                Vector3 targetWorldPos = widget.transform.position;
                _burstParticleSystem.transform.position = targetWorldPos;
                _burstParticleSystem.Emit(_burstParticleCount);
            }
        }
    }
}