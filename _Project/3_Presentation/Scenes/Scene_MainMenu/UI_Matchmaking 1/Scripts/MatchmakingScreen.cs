// Assets/_Project/3_Presentation/Scenes/Scene_MainMenu/UI_Matchmaking 1/Scripts/MatchmakingScreen.cs
// The View. It is "dumb" — it holds zero logic, only exposes SerializeFields and animation methods.
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Billiards.Presentation
{
    public class MatchmakingScreen : SsBaseMenu
    {

        [Header("Navigation")]
        [SerializeField] private Button _cancelBtn;

        [Header("Player 1 UI")]
        [SerializeField] private RectTransform _player1Panel;
        [SerializeField] private RectTransform _player1LevelContainer;
        [SerializeField] private RectTransform _player1CoinsContribution;
        [SerializeField] private Animator _player1CoinsAnimator;
        [SerializeField] private TextMeshProUGUI _playerDisplayNameTxt;
        [SerializeField] private TextMeshProUGUI _playerLevelTxt;
        [SerializeField] private Slider _playerLevelSlider;
        [SerializeField] private TextMeshProUGUI _playerCoinsTxt;

        [Header("Opponent UI")]
        [SerializeField] private RectTransform _opponentPanel;
        [SerializeField] private RectTransform _opponentLevelContainer;
        [SerializeField] private RectTransform _opponentCoinsContribution;
        [SerializeField] private Animator _opponentCoinsAnimator;
        [SerializeField] private TextMeshProUGUI _opponentDisplayNameTxt;
        [SerializeField] private TextMeshProUGUI _opponentLevelTxt;
        [SerializeField] private Slider _opponentLevelSlider;
        [SerializeField] private TextMeshProUGUI _opponentCoinsTxt;

        [Header("Opponent Search")]
        [SerializeField] private RawImage _opponentSearchRawImage;
        [SerializeField] private Image _opponentFoundEffectImg;
        [SerializeField] private Image _opponentFoundImg;

        [Header("Match Info")]
        [SerializeField] private TextMeshProUGUI _betAmountTxt;

        [Header("Animation Assets")]
        [Tooltip("Texture containing a vertical strip of avatars for the scrolling search animation.")]
        [SerializeField] private Texture2D _avatarScrollTexture;

        [Header("Animation Settings")]
        [SerializeField] private float _panelSlideInDuration = 0.5f;
        [SerializeField] private float _panelBounceDuration = 0.5f;
        [SerializeField] private float _scrollSpeed = 0.5f;
        [SerializeField] private float _coinAnimationDelay = 0.33f;

        // Internal animation state
        private Tween _scrollTween;

        // --- MVP Events (for Presenter to subscribe) ---
        public event Action OnCancelClicked;
        public event Action OnMenuShown;
        public event Action OnMenuHidden;

        private float _p1OriginalX;
        private float _p2OriginalX;

        public override void Awake()
        {
            base.Awake();

            if (_cancelBtn != null)
                _cancelBtn.onClick.AddListener(() => OnCancelClicked?.Invoke());

            if (_player1Panel != null) _p1OriginalX = _player1Panel.anchoredPosition.x;
            if (_opponentPanel != null) _p2OriginalX = _opponentPanel.anchoredPosition.x;
        }

        protected override void Show(enFromDirections fromDirection, bool snap)
        {
            base.Show(fromDirection, snap);
            OnMenuShown?.Invoke();
        }

        protected override void Hide(bool snap, enToDirections outDirection)
        {
            base.Hide(snap, outDirection);
            OnMenuHidden?.Invoke();
        }

        public override void HideImmediate()
        {
            base.HideImmediate();
            OnMenuHidden?.Invoke();
        }

        // =====================================================
        // PUBLIC METHODS — Called by the Presenter
        // =====================================================

        /// <summary>
        /// Resets all animated elements to their pre-animation state.
        /// Must be called before PlayIntroAnimationAsync to set the stage.
        /// </summary>
        public void ResetToDefault()
        {
            // CRITICAL: Kill all stale DOTween animations on our elements first.
            // Without this, returning to the screen causes old tweens to conflict
            // with new ones, resulting in slower/shorter animations.
            StopSearchScrollAnimation();
            if (_player1Panel != null) _player1Panel.DOKill();
            if (_opponentPanel != null) _opponentPanel.DOKill();
            if (_player1LevelContainer != null) _player1LevelContainer.DOKill();
            if (_opponentLevelContainer != null) _opponentLevelContainer.DOKill();
            if (_player1CoinsContribution != null) _player1CoinsContribution.DOKill();
            if (_opponentCoinsContribution != null) _opponentCoinsContribution.DOKill();
            if (_opponentFoundEffectImg != null) _opponentFoundEffectImg.DOKill();
            if (_opponentSearchRawImage != null) _opponentSearchRawImage.DOKill();

            // Hide level bars and coin contributions at scale 0
            if (_player1LevelContainer != null)
            {
                _player1LevelContainer.localScale = Vector3.one * 0.1f;
                _player1LevelContainer.gameObject.SetActive(false);
            }
            if (_opponentLevelContainer != null)
            {
                _opponentLevelContainer.localScale = Vector3.one * 0.1f;
                _opponentLevelContainer.gameObject.SetActive(false);
            }
            if (_player1CoinsContribution != null)
            {
                _player1CoinsContribution.localScale = Vector3.one * 0.1f;
                _player1CoinsContribution.gameObject.SetActive(false);
            }
            if (_opponentCoinsContribution != null)
            {
                _opponentCoinsContribution.localScale = Vector3.one * 0.1f;
                _opponentCoinsContribution.gameObject.SetActive(false);
            }

            // Also hide the separate animator panels so they reset
            if (_player1CoinsAnimator != null) _player1CoinsAnimator.gameObject.SetActive(false);
            if (_opponentCoinsAnimator != null) _opponentCoinsAnimator.gameObject.SetActive(false);

            // Hide the found effect
            if (_opponentFoundEffectImg != null)
            {
                _opponentFoundEffectImg.gameObject.SetActive(false);
                _opponentFoundEffectImg.transform.localScale = Vector3.one * 0.75f;
                var c = _opponentFoundEffectImg.color;
                _opponentFoundEffectImg.color = new Color(c.r, c.g, c.b, 0f);
            }

            // Set opponent search texture and reset transparency
            if (_opponentSearchRawImage != null && _avatarScrollTexture != null)
            {
                _opponentSearchRawImage.texture = _avatarScrollTexture;
                _opponentSearchRawImage.uvRect = new Rect(0, 0, 1, 0.125f);
                var color = _opponentSearchRawImage.color;
                _opponentSearchRawImage.color = new Color(color.r, color.g, color.b, 1f);
            }

            // Hide the actual opponent found image
            if (_opponentFoundImg != null)
            {
                var color = _opponentFoundImg.color;
                _opponentFoundImg.color = new Color(color.r, color.g, color.b, 0f);
            }

            // Snap panels off-screen based on their original positions
            if (_player1Panel != null) _player1Panel.DOAnchorPosX(_p1OriginalX - 800f, 0f);
            if (_opponentPanel != null) _opponentPanel.DOAnchorPosX(_p2OriginalX + 800f, 0f);

            // Hide name labels until match is found
            if (_playerDisplayNameTxt != null) _playerDisplayNameTxt.gameObject.SetActive(false);
            if (_opponentDisplayNameTxt != null) _opponentDisplayNameTxt.gameObject.SetActive(false);
        }

        /// <summary>
        /// Populates player 1's info panel.
        /// </summary>
        public void SetPlayerInfo(string displayName, int level, string coins)
        {
            if (_playerDisplayNameTxt != null) _playerDisplayNameTxt.SetText(displayName);
            if (_playerLevelTxt != null) _playerLevelTxt.SetText(level.ToString());
            if (_playerCoinsTxt != null) _playerCoinsTxt.SetText(coins);
        }

        /// <summary>
        /// Populates the opponent's info panel.
        /// </summary>
        public void SetOpponentInfo(string displayName, int level, string coins)
        {
            if (_opponentDisplayNameTxt != null) _opponentDisplayNameTxt.SetText(displayName);
            if (_opponentLevelTxt != null) _opponentLevelTxt.SetText(level.ToString());
            if (_opponentCoinsTxt != null) _opponentCoinsTxt.SetText(coins);
        }

        /// <summary>
        /// Sets the bet amount text in the center.
        /// </summary>
        public void SetBetAmount(string amount)
        {
            if (_betAmountTxt != null) _betAmountTxt.SetText(amount);
        }

        /// <summary>
        /// Phase 1: Intro — slides both player panels in from opposite sides,
        /// then starts the opponent search UV scroll loop.
        /// </summary>
        public async UniTask PlayIntroAnimationAsync(CancellationToken token)
        {
            // --- Start the UV scroll loop on the opponent search image IMMEDIATELY ---
            StartSearchScrollAnimation();

            // --- Player 1 Panel: slide from left ---
            // Start off-screen left
            await _player1Panel.DOAnchorPosX(_p1OriginalX - 800f, 0f);

            var p1Slide = _player1Panel.DOAnchorPosX(_p1OriginalX + 130f, _panelSlideInDuration)
                .SetEase(Ease.Linear);

            // --- Opponent Panel: slide from right (mirror) ---
            await _opponentPanel.DOAnchorPosX(_p2OriginalX + 800f, 0f);

            var p2Slide = _opponentPanel.DOAnchorPosX(_p2OriginalX - 130f, _panelSlideInDuration)
                .SetEase(Ease.Linear);

            // Wait for both to finish the initial fast slide
            await UniTask.WhenAll(
                p1Slide.ToUniTask(cancellationToken: token),
                p2Slide.ToUniTask(cancellationToken: token)
            );

            // --- Overshoot + Bounce ---
            var p1Overshoot = _player1Panel.DOAnchorPosX(_p1OriginalX - 100f, 0.1f).SetEase(Ease.Linear);
            var p2Overshoot = _opponentPanel.DOAnchorPosX(_p2OriginalX + 100f, 0.1f).SetEase(Ease.Linear);

            await UniTask.WhenAll(
                p1Overshoot.ToUniTask(cancellationToken: token),
                p2Overshoot.ToUniTask(cancellationToken: token)
            );

            var p1Bounce = _player1Panel.DOAnchorPosX(_p1OriginalX, _panelBounceDuration).SetEase(Ease.OutBounce);
            var p2Bounce = _opponentPanel.DOAnchorPosX(_p2OriginalX, _panelBounceDuration).SetEase(Ease.OutBounce);

            await UniTask.WhenAll(
                p1Bounce.ToUniTask(cancellationToken: token),
                p2Bounce.ToUniTask(cancellationToken: token)
            );

            Debug.Log("[MatchmakingScreen] Intro animation complete. Search scroll started.");
        }

        /// <summary>
        /// Phase 2: Match Found — stops the scroll, plays the flash effect,
        /// reveals opponent info, pops in level bars + coin animations.
        /// </summary>
        public async UniTask PlayMatchFoundAnimationAsync(CancellationToken token)
        {
            // 1. Kill the scroll loop
            StopSearchScrollAnimation();

            // Set OpponentSearch_RawImage transparency to 0
            if (_opponentSearchRawImage != null)
            {
                _opponentSearchRawImage.DOFade(0f, 0.25f).ToUniTask(cancellationToken: token).Forget();
            }

            // 2. Show player names
            if (_playerDisplayNameTxt != null) _playerDisplayNameTxt.gameObject.SetActive(true);
            if (_opponentDisplayNameTxt != null) _opponentDisplayNameTxt.gameObject.SetActive(true);

            // 3. Pop in level bars with OutBounce (concurrent)
            if (_player1LevelContainer != null)
            {
                _player1LevelContainer.gameObject.SetActive(true);
                _player1LevelContainer.DOScale(1f, 0.75f).SetEase(Ease.OutBounce).ToUniTask(cancellationToken: token).Forget();
            }
            if (_opponentLevelContainer != null)
            {
                _opponentLevelContainer.gameObject.SetActive(true);
                _opponentLevelContainer.DOScale(1f, 0.75f).SetEase(Ease.OutBounce).ToUniTask(cancellationToken: token).Forget();
            }

            // 4. Pop in coin contributions + trigger coin animation (concurrent)
            if (_player1CoinsContribution != null)
            {
                PopInAndPlayCoinsAsync(_player1CoinsContribution, _player1CoinsAnimator, false, token).Forget();
            }

            if (_opponentCoinsContribution != null)
            {
                PopInAndPlayCoinsAsync(_opponentCoinsContribution, _opponentCoinsAnimator, true, token).Forget();
            }

            // 5. White flash effect on the opponent frame
            if (_opponentFoundEffectImg != null)
            {
                _opponentFoundEffectImg.gameObject.SetActive(true);
                
                // Start as a dot at the center
                _opponentFoundEffectImg.transform.localScale = Vector3.zero;
                
                // Set color to full opacity white (or original color)
                var c = _opponentFoundEffectImg.color;
                _opponentFoundEffectImg.color = new Color(c.r, c.g, c.b, 1f);

                // Reset opponent search to full image (stop showing the strip)
                if (_opponentSearchRawImage != null)
                    _opponentSearchRawImage.uvRect = new Rect(0, 0, 1, 1);

                // Expand from dot to corners
                await _opponentFoundEffectImg.transform
                    .DOScale(1f, 0.25f)
                    .SetEase(Ease.OutQuad)
                    .ToUniTask(cancellationToken: token);

                // Show the found image instantly exactly when expansion reaches corners
                if (_opponentFoundImg != null)
                {
                    var foundColor = _opponentFoundImg.color;
                    _opponentFoundImg.color = new Color(foundColor.r, foundColor.g, foundColor.b, 1f);
                }

                // Flash out (fade to 0 transparency)
                await _opponentFoundEffectImg.DOFade(0f, 0.5f)
                    .ToUniTask(cancellationToken: token);

                _opponentFoundEffectImg.gameObject.SetActive(false);
                _opponentFoundEffectImg.transform.localScale = Vector3.one;
            }

            Debug.Log("[MatchmakingScreen] Match found animation sequence complete.");
        }

        // =====================================================
        // PRIVATE HELPERS — Animation internals
        // =====================================================

        private void StartSearchScrollAnimation()
        {
            if (_opponentSearchRawImage == null) return;

            _scrollTween = DOTween.To(
                () => _opponentSearchRawImage.uvRect.y,
                y => _opponentSearchRawImage.uvRect = new Rect(
                    _opponentSearchRawImage.uvRect.x,
                    y,
                    _opponentSearchRawImage.uvRect.width,
                    _opponentSearchRawImage.uvRect.height),
                1f,
                _scrollSpeed)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }

        private void StopSearchScrollAnimation()
        {
            if (_scrollTween != null && _scrollTween.IsActive())
            {
                _scrollTween.Kill();
                _scrollTween = null;
            }
        }

        private async UniTaskVoid PopInAndPlayCoinsAsync(Transform container, Animator animator, bool isMirrored, CancellationToken token)
        {
            if (_coinAnimationDelay > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_coinAnimationDelay), cancellationToken: token);
            }

            if (container != null)
            {
                container.gameObject.SetActive(true);
                container.DOScale(1f, 0.5f).SetEase(Ease.OutBounce).ToUniTask(cancellationToken: token).Forget();
            }

            if (animator == null) return;

            if (!animator.gameObject.activeSelf)
            {
                animator.gameObject.SetActive(true);
            }
            animator.transform.SetAsLastSibling();
            animator.transform.localScale = isMirrored ? new Vector3(-1, 1, 1) : Vector3.one;
            
            var img = animator.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                img.color = Color.white;
                img.enabled = true;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);

            bool isActive = animator.gameObject.activeInHierarchy;
            bool isEnabled = animator.enabled;

            if (isActive && isEnabled)
            {
                animator.Play("CoinsAnimation", -1, 0f);
            }
        }

        private void OnDestroy()
        {
            StopSearchScrollAnimation();
        }
    }
}
