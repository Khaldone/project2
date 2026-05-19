// Assets/_Project/3_Presentation/TrophyRoad/Widgets/TrophyNodeWidget.cs
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // <-- REQUIRED FOR EVENT SYSTEM INTERFACES
using Billiards.Core.Progression;
using Cysharp.Threading.Tasks;
using TMPro;

namespace Billiards.Presentation.TrophyRoad.Widgets
{
    // Implement drag interfaces to act as an event-forwarding bridge
    public sealed class TrophyNodeWidget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private TextMeshProUGUI _titleTxt;
        [SerializeField] private TextMeshProUGUI _cupsRequiredTxt;
        [SerializeField] private Button _claimButton;
        [SerializeField] private Image _rewardIconImg;

        [Header("State Layout Ensembles")]
        [SerializeField] private GameObject _lockedVisual;
        [SerializeField] private GameObject _claimableVisual;
        [SerializeField] private GameObject _claimedVisual;

        [Header("Performance Optimization Components")]
        [SerializeField] private Canvas _targetCanvas;
        [SerializeField] private CanvasGroup _targetCanvasGroup;

        private Action _onClickedCallback;
        private string _currentLoadedKey;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private ScrollRect _mainScrollRect; // Cached reference to the parent scroller

        public RectTransform CachedRectTransform
        {
            get
            {
                if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        // Updated signature to accept the parent scroller reference
        public void Setup(TrophyMilestone milestone, Func<string, UniTask<Sprite>> iconProvider, ScrollRect parentScrollRect, Action onClicked)
        {
            _mainScrollRect = parentScrollRect; // Cache the scroller link
            _titleTxt.text = milestone.Title;
            _cupsRequiredTxt.text = $"{milestone.RequiredCups}";
            _onClickedCallback = onClicked;

            _claimButton.onClick.RemoveAllListeners();
            _claimButton.onClick.AddListener(() => _onClickedCallback?.Invoke());

            UpdateVisualState(milestone.State);
            LoadRewardIconSequence(milestone.IconAssetKey, iconProvider).Forget();
        }

        // =========================================================================
        // ZERO-ALLOCATION EVENT BUBBLING PASSES
        // =========================================================================
        public void OnBeginDrag(PointerEventData eventData) => _mainScrollRect?.OnBeginDrag(eventData);
        public void OnDrag(PointerEventData eventData) => _mainScrollRect?.OnDrag(eventData);
        public void OnEndDrag(PointerEventData eventData) => _mainScrollRect?.OnEndDrag(eventData);

        public void SetCullingState(bool isCulled)
        {
            if (_targetCanvas == null || _targetCanvasGroup == null) return;

            bool shouldRender = !isCulled;
            if (_targetCanvas.enabled != shouldRender)
            {
                _targetCanvas.enabled = shouldRender;
                _targetCanvasGroup.blocksRaycasts = shouldRender;
                _targetCanvasGroup.interactable = shouldRender;
            }
        }

        public void UpdateVisualState(MilestoneState state)
        {
            _lockedVisual.SetActive(state == MilestoneState.Locked);
            _claimableVisual.SetActive(state == MilestoneState.Claimable);
            _claimedVisual.SetActive(state == MilestoneState.Claimed);
            _claimButton.interactable = (state == MilestoneState.Claimable);
        }

        private async UniTaskVoid LoadRewardIconSequence(string assetKey, Func<string, UniTask<Sprite>> iconProvider)
        {
            if (_currentLoadedKey == assetKey) return;
            _currentLoadedKey = assetKey;
            if (_rewardIconImg == null || iconProvider == null) return;

            _rewardIconImg.enabled = false;
            try
            {
                Sprite sprite = await iconProvider.Invoke(assetKey);
                if (_currentLoadedKey == assetKey)
                {
                    _rewardIconImg.sprite = sprite;
                    _rewardIconImg.enabled = true;
                }
            }
            catch (Exception)
            {
                _rewardIconImg.sprite = null;
                _rewardIconImg.enabled = false;
            }
        }
    }
}