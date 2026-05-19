// Attached to: LoginCanvas
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface ILoginView
{

}
public class LoginView : MonoBehaviour, ILoginView
{
    [Header("Buttons")]
    [SerializeField] private Button _nativeLoginButton;
    [SerializeField] private Button _guestLoginButton;

    [Header("Dynamic Visuals")]
    [SerializeField] private TextMeshProUGUI _nativeButtonText;
    [SerializeField] private GameObject _loadingOverlay;


    // The events our LoginPresenter listens to
    public event Action OnNativeLoginClicked;
    public event Action OnGuestLoginClicked;

    private void Awake()
    {
        // Route Unity's internal UI events to our pure C# architecture
        _nativeLoginButton.onClick.AddListener(() => OnNativeLoginClicked?.Invoke());
        _guestLoginButton.onClick.AddListener(() => OnGuestLoginClicked?.Invoke());
    }

    // Called by the Presenter to change "Sign In" to "Sign In with Apple"
    public void SetNativeButtonText(string text)
    {
        _nativeButtonText.text = text;
    }

    // Called by the Presenter to block the screen while PlayFab thinks
    public void ShowLoadingSpinner(bool isVisible)
    {
        _loadingOverlay.SetActive(isVisible);

        // Also disable the buttons physically so players can't spam-click them
        _nativeLoginButton.interactable = !isVisible;
        _guestLoginButton.interactable = !isVisible;
    }

    private void OnDestroy()
    {
        _nativeLoginButton.onClick.RemoveAllListeners();
        _guestLoginButton.onClick.RemoveAllListeners();
    }
}