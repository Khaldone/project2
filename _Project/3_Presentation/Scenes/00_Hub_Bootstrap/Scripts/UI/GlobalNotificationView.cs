// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/UI/GlobalNotificationView.cs
using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class GlobalNotificationView : SsBaseMenu
{
    [Header("Standard Notification UI")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private Image _topStripColor;
    
    [Header("Overlay (Click Blocker)")]
    [SerializeField] private GameObject _overlayPanel;
    
    [Header("Colors")]
    [SerializeField] private Color _successColor = Color.green;
    [SerializeField] private Color _errorColor = Color.red;
    [SerializeField] private Color _timeoutColor = new Color(0.5f, 0.3f, 0.1f); // Brown
    [SerializeField] private Color _infoColor = Color.cyan;

    [Header("Interactive UI")]
    [SerializeField] private GameObject _actionButtonContainer; // Holds Accept/Decline buttons
    [SerializeField] private Button _acceptButton;
    [SerializeField] private Button _declineButton;
    
    [SerializeField] private GameObject _standardButtonContainer; // Holds Ok button
    [SerializeField] private Button _okButton;
    
    [SerializeField] private Button _closeButton;

    private INotificationQueue _queueService;
    private UniTaskCompletionSource<bool> _userInputTcs;
    private UniTaskCompletionSource _dismissTcs;
    private CancellationTokenSource _cts;

    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }

    protected override void Show(enFromDirections fromDirection, bool snap)
    {
        base.Show(fromDirection, snap);
    }

    [Inject]
    public void Construct(INotificationQueue queueService)
    {
        _queueService = queueService;
        _queueService.OnShowNotification += AnimateNotification;
        _queueService.OnClassificationUpdated += HandleClassificationUpdated;
        _queueService.OnMessageUpdated += HandleMessageUpdated;
        _queueService.OnDismissRequested += HandleDismissRequested;

        // Wire up the Unity buttons to resolve the UniTask
        if (_acceptButton != null) _acceptButton.onClick.AddListener(() => _userInputTcs?.TrySetResult(true));
        if (_declineButton != null) _declineButton.onClick.AddListener(() => _userInputTcs?.TrySetResult(false));
        if (_okButton != null) _okButton.onClick.AddListener(() => _userInputTcs?.TrySetResult(true));
        
        // Close button acts as a decline or ignore
        if (_closeButton != null) _closeButton.onClick.AddListener(() => _userInputTcs?.TrySetResult(false));

        // Signal that we're ready — flush any notifications enqueued before this View existed.
        _queueService.NotifyViewReady();
    }

    private void HandleClassificationUpdated(NotificationClassification newClassification)
    {
        if (_topStripColor != null)
        {
            _topStripColor.color = GetColorForClassification(newClassification);
        }
    }

    private void HandleMessageUpdated(string title, string message)
    {
        if (_titleText != null) _titleText.text = title;
        if (_messageText != null) _messageText.text = message;
    }

    private void HandleDismissRequested()
    {
        _dismissTcs?.TrySetResult();
    }

    private Color GetColorForClassification(NotificationClassification classification)
    {
        switch (classification)
        {
            case NotificationClassification.Success: return _successColor;
            case NotificationClassification.Error: return _errorColor;
            case NotificationClassification.Timeout: return _timeoutColor;
            case NotificationClassification.Info: 
            default: return _infoColor;
        }
    }

    private async void AnimateNotification(NotificationData data)
    {
        // Activate Overlay
        if (_overlayPanel != null)
        {
            _overlayPanel.SetActive(true);

            // Ensure the overlay is dark grey
            var overlayImage = _overlayPanel.GetComponentInChildren<Image>(true);
            if (overlayImage != null)
            {
                overlayImage.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);
            }
        }

        // 1. Setup UI
        if (_titleText != null) _titleText.text = data.Title;
        if (_messageText != null) _messageText.text = data.Message;

        // Set Top Strip Color
        if (_topStripColor != null)
        {
            _topStripColor.color = GetColorForClassification(data.Classification);
        }

        // Toggle Buttons Based on Layout
        bool isActionable = data.Layout == NotificationLayout.Actionable;
        bool isStatusOverlay = data.Layout == NotificationLayout.StatusOverlay;

        if (_actionButtonContainer != null) _actionButtonContainer.SetActive(isActionable);
        if (_standardButtonContainer != null) _standardButtonContainer.SetActive(!isActionable && !isStatusOverlay);
        if (_closeButton != null) _closeButton.gameObject.SetActive(!isStatusOverlay);

        // 2. Animate In (via SsBaseMenu)
        if (data.SlideIn == NotificationSlideDirection.Immediate)
        {
            JustShow();
        }
        else
        {
            Show(MapSlideIn(data.SlideIn), snap: false);
            // Wait for SsBaseMenu state machine to finish animating
            await UniTask.WaitUntil(() => State == enStates.visibleAndIdle);
        }

        // 3. The Waiting Logic
        bool playerAccepted = false;

        if (isStatusOverlay)
        {
            // StatusOverlay: No timeout, no buttons — wait for DismissActive() from the caller
            _dismissTcs = new UniTaskCompletionSource();
            await _dismissTcs.Task;
            _dismissTcs = null;
        }
        else
        {
            // Standard/Actionable: Race condition between Button Click and Timeout
            _userInputTcs = new UniTaskCompletionSource<bool>();
            _cts = new CancellationTokenSource();

            try
            {
                // Delay task throws OperationCanceledException if cancelled before timeout
                var timeoutTask = UniTask.Delay(data.DisplayDurationSeconds * 1000, cancellationToken: _cts.Token);
                
                // Race them
                var winner = await UniTask.WhenAny(_userInputTcs.Task, timeoutTask);

                if (winner.hasResultLeft) // userInputTcs won
                {
                    playerAccepted = winner.result;
                    _cts.Cancel(); // Stop the timer
                }
                else // timeoutTask won
                {
                    playerAccepted = false; // Auto-decline/ignore
                }
            }
            catch (System.OperationCanceledException)
            {
                // If the delay is cancelled, it means the user clicked a button
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
            }
        }

        // 4. Fire callback to CoreDomain
        data.OnInteractionResolved?.Invoke(playerAccepted);

        // 5. Animate Out (via SsBaseMenu)
        if (data.SlideOut == NotificationSlideDirection.Immediate)
        {
            HideImmediate();
        }
        else
        {
            Hide(false, MapSlideOut(data.SlideOut));
            // Wait for SsBaseMenu state machine to finish animating
            await UniTask.WaitUntil(() => State == enStates.hidden);
        }

        // 6. Manage Overlay State
        // Only turn off the overlay if there are no more notifications waiting
        if (!_queueService.HasPendingNotifications && _overlayPanel != null)
        {
            _overlayPanel.SetActive(false);
        }

        // Tell the queue to bring in the next one
        _queueService.NotifyDisplayFinished();
    }

    private enFromDirections MapSlideIn(NotificationSlideDirection dir)
    {
        switch (dir)
        {
            case NotificationSlideDirection.Top: return enFromDirections.fromTop;
            case NotificationSlideDirection.Bottom: return enFromDirections.fromBottom;
            case NotificationSlideDirection.Left: return enFromDirections.fromLeft;
            case NotificationSlideDirection.Right: return enFromDirections.fromRight;
            default: return enFromDirections.invalid;
        }
    }

    private enToDirections MapSlideOut(NotificationSlideDirection dir)
    {
        switch (dir)
        {
            case NotificationSlideDirection.Top: return enToDirections.toTop;
            case NotificationSlideDirection.Bottom: return enToDirections.toBottom;
            case NotificationSlideDirection.Left: return enToDirections.toLeft;
            case NotificationSlideDirection.Right: return enToDirections.toRight;
            default: return enToDirections.invalid;
        }
    }

    private void OnDestroy()
    {
        if (_queueService != null)
        {
            _queueService.OnShowNotification -= AnimateNotification;
            _queueService.OnClassificationUpdated -= HandleClassificationUpdated;
            _queueService.OnMessageUpdated -= HandleMessageUpdated;
            _queueService.OnDismissRequested -= HandleDismissRequested;
        }
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
