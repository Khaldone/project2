// Assets/_Project/3_Presentation/Scene_CitySelection/Scripts/CitySlotView.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Billiards.Presentation.CitySelection
{
    public class CitySlotView : MonoBehaviour
    {
        [Header("UI Text References")]
        [SerializeField] private TextMeshProUGUI _cityNameText;
        [SerializeField] private TextMeshProUGUI _prizeText;
        [SerializeField] private TextMeshProUGUI _entryFeeText;
        [SerializeField] private TextMeshProUGUI _unlockRequirementText;

        [Header("Lock State Visuals")]
        [SerializeField] private GameObject _lockedOverlayPanel;
        [SerializeField] private Material _greyscaleMaterial;
        [SerializeField] private List<Image> _imagesToGreyscale;

        [Header("Flip Animation System")]
        [SerializeField] private RectTransform _frontPanel; // The main city info
        [SerializeField] private RectTransform _backPanel;  // The rules/prizes info
        [SerializeField] private Button _flipToggleButton;
        [SerializeField] private float _flipDuration = 0.2f;

        private bool _isFlipping = false;
        private bool _isShowingBack = false;
        // Default to locked — Presenter will unlock after data loads via SetLockState
        private bool _isLocked = true;

        // Expose the interaction event to the Presenter
        public event Action OnFlipRequested;

        private void Awake()
        {
            // Wire the physical toggle button to our local event AND trigger the visual animation
            if (_flipToggleButton != null)
            {
                _flipToggleButton.onClick.AddListener(() => 
                {
                    OnFlipRequested?.Invoke();
                    FlipAsync(this.GetCancellationTokenOnDestroy()).Forget();
                });
            }

            // Ensure correct starting state
            ResetFlipStateImmediate();
        }

        public void RenderCityData(string name, int prize, int fee)
        {
            _cityNameText.text = name;
            _prizeText.text = $"Prize: {prize}";
            _entryFeeText.text = $"Entry: {fee}";
        }

        public void SetLockState(bool isLocked, int reqCups, int reqLevel)
        {
            _isLocked = isLocked;

            if (_lockedOverlayPanel != null) 
            {
                _lockedOverlayPanel.SetActive(isLocked);
            }

            if (_imagesToGreyscale != null)
            {
                foreach (var img in _imagesToGreyscale)
                {
                    if (img == null) continue;

                    if (_greyscaleMaterial != null)
                    {
                        img.material = isLocked ? _greyscaleMaterial : null;
                        img.SetMaterialDirty(); 
                    }
                    else
                    {
                        img.color = isLocked ? Color.gray : Color.white;
                    }
                }
            }

            if (isLocked)
                _unlockRequirementText.text = $"Requires Level {reqLevel}\n{reqCups} CP";
            else
                _unlockRequirementText.text = "Unlocked!";

            // Disable the flip button if the city is locked so they can't see the rules
            if (_flipToggleButton != null)
            {
                _flipToggleButton.interactable = !isLocked;
            }
        }

        /// <summary>
        /// Instantly snaps the card back to the front. Useful for when the pool recycles the UI element.
        /// </summary>
        public void ResetFlipStateImmediate()
        {
            if (_frontPanel != null) _frontPanel.DOKill();
            if (_backPanel != null) _backPanel.DOKill();

            if (_frontPanel != null) _frontPanel.localRotation = Quaternion.identity;
            if (_backPanel != null) _backPanel.localRotation = Quaternion.Euler(0, 90, 0);

            if (_frontPanel != null) _frontPanel.gameObject.SetActive(true);
            if (_backPanel != null) _backPanel.gameObject.SetActive(false);

            _isShowingBack = false;
            _isFlipping = false;
        }

        /// <summary>
        /// Executes the 3D card flip animation using UniTask for safe async flow.
        /// </summary>
        public async UniTask FlipAsync(CancellationToken token)
        {
            // Spam-click prevention or lock state check
            if (_isFlipping || _isLocked) return;

            _isFlipping = true;

            try
            {
                if (!_isShowingBack)
                {
                    // 1. Rotate Front to 90 degrees (invisible edge)
                    if (_frontPanel != null)
                    {
                        await _frontPanel.DOLocalRotate(new Vector3(0, 90, 0), _flipDuration)
                            .SetEase(Ease.InQuad)
                            .ToUniTask(cancellationToken: token);

                        _frontPanel.gameObject.SetActive(false);
                    }

                    // 2. Prep Back at -90 degrees, turn it on, and rotate to 0
                    if (_backPanel != null)
                    {
                        _backPanel.localRotation = Quaternion.Euler(0, -90, 0);
                        _backPanel.gameObject.SetActive(true);

                        await _backPanel.DOLocalRotate(Vector3.zero, _flipDuration)
                            .SetEase(Ease.OutQuad)
                            .ToUniTask(cancellationToken: token);
                    }

                    _isShowingBack = true;
                }
                else
                {
                    // 1. Rotate Back to 90 degrees
                    if (_backPanel != null)
                    {
                        await _backPanel.DOLocalRotate(new Vector3(0, 90, 0), _flipDuration)
                            .SetEase(Ease.InQuad)
                            .ToUniTask(cancellationToken: token);

                        _backPanel.gameObject.SetActive(false);
                    }

                    // 2. Prep Front at -90 degrees, turn it on, and rotate to 0
                    if (_frontPanel != null)
                    {
                        _frontPanel.localRotation = Quaternion.Euler(0, -90, 0);
                        _frontPanel.gameObject.SetActive(true);

                        await _frontPanel.DOLocalRotate(Vector3.zero, _flipDuration)
                            .SetEase(Ease.OutQuad)
                            .ToUniTask(cancellationToken: token);
                    }

                    _isShowingBack = false;
                }
            }
            finally
            {
                // Guarantee the lock is released even if the task was cancelled
                _isFlipping = false;
            }
        }

        private void OnDestroy()
        {
            if (_frontPanel != null) _frontPanel.DOKill();
            if (_backPanel != null) _backPanel.DOKill();
        }
    }
}

