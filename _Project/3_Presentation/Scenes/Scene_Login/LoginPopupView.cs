using UnityEngine;
using UnityEngine.UI; // or TMPro if using TextMeshPro

namespace Billiards.Presentation.Login
{
    public class LoginPopupView : MonoBehaviour
    {
        [SerializeField] private GameObject _popupPanel;
        [SerializeField] private Text _statusText; // Change to TMP_Text if needed
        [SerializeField] private Image _overlayPanel; // The new overlay

        public void ShowLoading()
        {
            _popupPanel.SetActive(true);
            _statusText.text = "Authenticating with Server...";
            _statusText.color = Color.white;
        }

        public void ShowSuccess()
        {
            _statusText.text = "Login Successful! Loading Profile...";
            _statusText.color = Color.green;
        }

        public void ShowError(string message)
        {
            _statusText.text = "Error: " + message;
            _statusText.color = Color.red;
        }

        public void Hide()
        {
            _popupPanel.SetActive(false);
        }

        public void SetOverlayDim()
        {
            if (_overlayPanel != null)
            {
                _overlayPanel.gameObject.SetActive(true);
                _overlayPanel.color = new Color(0.15f, 0.15f, 0.15f, 0.85f); // Dim dark grey
            }
        }

        public void HideOverlay()
        {
            if (_overlayPanel != null)
            {
                _overlayPanel.gameObject.SetActive(false);
            }
        }
    }
}