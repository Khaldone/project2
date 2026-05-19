// Assets/_Project/3_Presentation/Scene_IAP/Scripts/IAP_Screen.cs
using UnityEngine;
using TMPro;

namespace Billiards.Presentation.Shop
{
    public class IAP_Screen : SsBaseMenu
    {
        [Header("Global Transaction UI")]
        [SerializeField] private GameObject _loadingSpinner;
        [SerializeField] private TextMeshProUGUI _statusText;

        public override void Awake()
        {
            base.Awake();
            HideLoading();
        }

        public void ShowLoading(string message)
        {
            if (_statusText != null) _statusText.text = message;
            if (_loadingSpinner != null) _loadingSpinner.SetActive(true);
        }

        public void HideLoading()
        {
            if (_loadingSpinner != null) _loadingSpinner.SetActive(false);
            if (_statusText != null) _statusText.text = "";
        }

        public void ShowSuccess(string productName)
        {
            HideLoading();
            if (_statusText != null) _statusText.text = $"Successfully purchased {productName}!";

            // Optional: You could trigger a DOTween particle explosion here
        }

        public void ShowError(string error)
        {
            HideLoading();
            if (_statusText != null) _statusText.text = $"Error: {error}";
        }
    }
}