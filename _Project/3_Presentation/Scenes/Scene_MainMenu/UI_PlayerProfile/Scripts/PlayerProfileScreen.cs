using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Billiards.Presentation
{
    public interface IPlayerProfileView
    {
        void SetPlayerName(string name);
        void SetCoinBalance(int coins);
        void SetAvatarSprite(Sprite sprite);

        event Action OnCloseButtonClicked;
        event Action OnChooseImageClicked;

        void SetChooseImageInteractable(bool interactable);
    }

    public class PlayerProfileScreen : SsBaseMenu
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _coinText;

        [Header("Avatar")]
        [SerializeField] private Image _avatarImage;
        [SerializeField] private Button _chooseImageBtn;

        public event Action OnChooseImageClicked;

        protected override void Show(enFromDirections fromDirection, bool snap)
        {
            base.Show(fromDirection, snap);
        }

        public override void Awake()
        {
            base.Awake();

            if (_chooseImageBtn != null)
            {
                _chooseImageBtn.onClick.AddListener(() => OnChooseImageClicked?.Invoke());
            }

        }

        public void SetPlayerName(string name) => _nameText.text = name;
        public void SetCoinBalance(int coins) => _coinText.text = $"Coins: {coins:N0}";
        public void SetAvatarSprite(Sprite sprite)
        {
            if (_avatarImage != null)
            {
                _avatarImage.sprite = sprite;
            }
        }

        public void SetChooseImageInteractable(bool interactable)
        {
            if (_chooseImageBtn != null) _chooseImageBtn.interactable = interactable;
        }
    }
}