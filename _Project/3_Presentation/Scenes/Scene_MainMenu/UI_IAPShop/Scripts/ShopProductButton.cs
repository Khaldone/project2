// Assets/_Project/3_Presentation/Scene_IAP/Scripts/ShopProductButton.cs
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Billiards.Presentation.Shop
{
    /// <summary>
    /// A "dumb" View component that represents a single purchasable item on the screen.
    /// It only knows how to display text and expose its click event.
    /// </summary>
    public class ShopProductButton : MonoBehaviour
    {
        // We expose the button as a property so the ShopMenu can listen to its onClick event
        [field: SerializeField] public Button Button { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private Image _productImage;
        [SerializeField] private TextMeshProUGUI _buttonText; // New explicit reference for the button's text

        // Fired when the button is clicked. Passes the Product ID back to the pool manager.
        public event System.Action<string> OnPrimaryActionClicked;

        /// <summary>
        /// Called by the ShopMenu to populate the visuals.
        /// </summary>
        public async Cysharp.Threading.Tasks.UniTask SetupAsync(Billiards.CoreDomain.Monetization.StoreProduct product, Billiards.CoreDomain.Assets.IStoreAssetProvider assetProvider, bool isOwned = false, bool isEquipped = false)
        {
            if (_nameText != null) _nameText.text = product.Name;

            // Handle the actual price label
            if (_priceText != null)
            {
                _priceText.text = product.LocalizedPrice;
                // Optional: Hide the price label if the item is already owned
                _priceText.gameObject.SetActive(!isOwned);
            }

            // Handle the action verb on the button itself
            var btnText = _buttonText != null ? _buttonText : (Button != null ? Button.GetComponentInChildren<TextMeshProUGUI>() : null);
            if (btnText != null)
            {
                if (isEquipped) btnText.text = "Unequip";
                else if (isOwned) btnText.text = "Equip";
                else btnText.text = "Purchase";
            }

            // Clean up and assign the click event
            if (Button != null)
            {
                Button.onClick.RemoveAllListeners();
                Button.onClick.AddListener(() => OnPrimaryActionClicked?.Invoke(product.Id));
            }

            if (_productImage != null && assetProvider != null && !string.IsNullOrEmpty(product.Id))
            {
                var sprite = await assetProvider.GetStoreItemIconAsync<Sprite>(product.Id);
                if (sprite != null)
                {
                    _productImage.sprite = sprite;
                }
            }
        }
    }
}