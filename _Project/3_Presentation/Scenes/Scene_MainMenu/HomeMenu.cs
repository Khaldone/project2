// Assets/_Project/3_Presentation/Scene_MainMenu/Scripts/HomeMenu.cs
using TMPro;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Billiards.Presentation
{
    public class HomeMenu : SsBaseMenu
    {
        [Header("PlayFab UI")]
        [SerializeField] private TextMeshProUGUI _displayNameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _countryText;

        [Header("Animation Panels (RectTransforms)")]
        [SerializeField] private RectTransform _topPanel;
        [SerializeField] private RectTransform _bottomPanel;
        [SerializeField] private RectTransform _leftPanel;
        [SerializeField] private RectTransform _rightPanel;

        [Header("Center Panel (The Pink Area)")]
        [SerializeField] private CanvasGroup _centerPanelGroup;
        [SerializeField] private RectTransform _centerPanel;
        [SerializeField] private float _centerDropOffset = 150f; // How many pixels high it starts before dropping

        [Header("Animation Settings")]
        [SerializeField] private float _animationDuration = 0.6f;
        [SerializeField] private Ease _slideEase = Ease.OutBack;
        [SerializeField] private Ease _centerDropEase = Ease.OutBack;

        [SerializeField] private float _staggerDelay = 0.15f;
        private float _shadowBuffer = 50f;

        private Vector2 _topOriginalPos;
        private Vector2 _bottomOriginalPos;
        private Vector2 _leftOriginalPos;
        private Vector2 _rightOriginalPos;
        private Vector2 _centerOriginalPos; // NEW: Cache the center position

        public override void Awake()
        {
            base.Awake();

            if (_topPanel != null) _topOriginalPos = _topPanel.anchoredPosition;
            if (_bottomPanel != null) _bottomOriginalPos = _bottomPanel.anchoredPosition;
            if (_leftPanel != null) _leftOriginalPos = _leftPanel.anchoredPosition;
            if (_rightPanel != null) _rightOriginalPos = _rightPanel.anchoredPosition;
            if (_centerPanel != null) _centerOriginalPos = _centerPanel.anchoredPosition;

            SnapPanelsOffscreen();
        }

        private void SnapPanelsOffscreen()
        {
            // 1. Hide the edge panels
            if (_topPanel != null)
                _topPanel.anchoredPosition = _topOriginalPos + new Vector2(0, _topPanel.rect.height + _shadowBuffer);

            if (_bottomPanel != null)
                _bottomPanel.anchoredPosition = _bottomOriginalPos + new Vector2(0, -(_bottomPanel.rect.height + _shadowBuffer));

            if (_leftPanel != null)
                _leftPanel.anchoredPosition = _leftOriginalPos + new Vector2(-(_leftPanel.rect.width + _shadowBuffer), 0);

            if (_rightPanel != null)
                _rightPanel.anchoredPosition = _rightOriginalPos + new Vector2(_rightPanel.rect.width + _shadowBuffer, 0);

            // 2. Hide the center panel (Move it UP by the offset and turn transparent)
            if (_centerPanelGroup != null && _centerPanel != null)
            {
                _centerPanelGroup.alpha = 0f;
                _centerPanel.anchoredPosition = _centerOriginalPos + new Vector2(0, _centerDropOffset);
            }
        }

        protected override void Show(enFromDirections fromDirection, bool snap)
        {
            SnapPanelsOffscreen();

            base.Show(fromDirection, snap);

            AnimatePanelsInAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTask AnimatePanelsInAsync(CancellationToken token)
        {
            // Center Panel: Fades in and slides DOWN to its original position
            UniTask centerTask = UniTask.CompletedTask;
            if (_centerPanelGroup != null && _centerPanel != null)
            {
                // We multiply by 3 to make it the very last thing that animates!
                float centerWaitTime = _staggerDelay * 3;

                var tAlpha = _centerPanelGroup.DOFade(1f, _animationDuration)
                    .SetDelay(centerWaitTime) // <-- Delay added here
                    .SetEase(Ease.OutQuad)
                    .ToUniTask(cancellationToken: token);

                var tMove = _centerPanel.DOAnchorPos(_centerOriginalPos, _animationDuration)
                    .SetDelay(centerWaitTime) // <-- Delay added here
                    .SetEase(_centerDropEase)
                    .ToUniTask(cancellationToken: token);

                centerTask = UniTask.WhenAll(tAlpha, tMove);
            }

            // Edge Panels: Cascading slide in
            var t1 = _topPanel.DOAnchorPos(_topOriginalPos, _animationDuration)
                .SetEase(_slideEase)
                .ToUniTask(cancellationToken: token);

            var t2 = _leftPanel.DOAnchorPos(_leftOriginalPos, _animationDuration)
                .SetDelay(_staggerDelay * 1)
                .SetEase(_slideEase)
                .ToUniTask(cancellationToken: token);

            var t3 = _rightPanel.DOAnchorPos(_rightOriginalPos, _animationDuration)
                .SetDelay(_staggerDelay * 1)
                .SetEase(_slideEase)
                .ToUniTask(cancellationToken: token);

            var t4 = _bottomPanel.DOAnchorPos(_bottomOriginalPos, _animationDuration)
                .SetDelay(_staggerDelay * 2)
                .SetEase(_slideEase)
                .ToUniTask(cancellationToken: token);

            // Wait for everything to finish
            await UniTask.WhenAll(t1, t2, t3, t4, centerTask);

            Debug.Log("[HomeMenu] Full Animation Sequence Complete!");
        }

        public void UpdateDisplay(string playerName, int level, string country)
        {
            _displayNameText.text = playerName;
            _levelText.text = $"Level: {level}";
            _countryText.text = country;
        }

#if UNITY_EDITOR
        private INotificationQueue _testQueue;

        private void TryResolveQueue()
        {
            if (_testQueue != null) return;

            var scopes = FindObjectsByType<VContainer.Unity.LifetimeScope>(FindObjectsSortMode.None);
            foreach (var scope in scopes)
            {
                if (scope.Container == null) continue;
                try
                {
                    _testQueue = (INotificationQueue)scope.Container.Resolve(typeof(INotificationQueue));
                    break;
                }
                catch { /* Ignore */ }
            }
        }

        private void OnGUI()
        {
            TryResolveQueue();

            if (GUI.Button(new Rect(10, 10, 250, 40), "TEST INFO NOTIFICATION"))
            {
                if (_testQueue != null)
                {
                    _testQueue.Enqueue(new NotificationData
                    {
                        Type = enNotificationType.SystemWarning,
                        Classification = NotificationClassification.Info,
                        Layout = NotificationLayout.Standard,
                        SlideIn = NotificationSlideDirection.Top,
                        SlideOut = NotificationSlideDirection.Top,
                        Title = "System Info",
                        Message = "This is a standard info notification.",
                        DisplayDurationSeconds = 5
                    });
                }
            }

            if (GUI.Button(new Rect(10, 60, 250, 40), "TEST ERROR NOTIFICATION"))
            {
                if (_testQueue != null)
                {
                    _testQueue.Enqueue(new NotificationData
                    {
                        Type = enNotificationType.SystemWarning,
                        Classification = NotificationClassification.Error,
                        Layout = NotificationLayout.Standard,
                        SlideIn = NotificationSlideDirection.Bottom,
                        SlideOut = NotificationSlideDirection.Bottom,
                        Title = "Connection Failed",
                        Message = "Unable to connect to the server. Please try again.",
                        DisplayDurationSeconds = 5
                    });
                }
            }

            if (GUI.Button(new Rect(10, 110, 250, 40), "TEST ACTION NOTIFICATION"))
            {
                if (_testQueue != null)
                {
                    _testQueue.Enqueue(new NotificationData
                    {
                        Type = enNotificationType.SystemWarning,
                        Classification = NotificationClassification.Success,
                        Layout = NotificationLayout.Actionable,
                        SlideIn = NotificationSlideDirection.Right,
                        SlideOut = NotificationSlideDirection.Left,
                        Title = "Match Found!",
                        Message = "A new challenger has appeared. Do you accept?",
                        DisplayDurationSeconds = 10,
                        OnInteractionResolved = (accepted) => 
                        {
                            if (accepted)
                            {
                                // The user pressed the 'Accept' or 'Ok' button
                                Debug.Log("Button 1 pressed");
                            }
                            else
                            {
                                // The user pressed the 'Decline' or 'Close' button
                                Debug.Log("Button 2 pressed");
                            }
                        }
                    });
                }
            }
        }
#endif
    }
}