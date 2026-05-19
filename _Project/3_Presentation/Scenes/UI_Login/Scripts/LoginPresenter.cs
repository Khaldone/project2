// Assets/_Project/Scenes/UI_Login/Scripts/LoginPresenter.cs
using System;
using VContainer.Unity;


public class LoginPresenter : IStartable, IDisposable
{
    //private readonly ILoginView _view;
    private readonly INativeAuthService _nativeAuth;
    //private readonly IPlayFabAuthService _playFabAuth; // Another wrapper we'd build

    //public LoginPresenter(ILoginView view, INativeAuthService nativeAuth, IPlayFabAuthService playFabAuth)
    //{
    //    _view = view;
    //    _nativeAuth = nativeAuth;
    //    _playFabAuth = playFabAuth;
    //}

    public LoginPresenter(INativeAuthService nativeAuth)
    {
        _nativeAuth = nativeAuth;
    }

    public void Start()
    {
        //_view.OnNativeLoginClicked += HandleNativeLogin;


        // Dynamically configure the UI based on the injected platform!
        //_view.SetButtonText($"Sign in with {_nativeAuth.PlatformName}");

        // (Optional: Use AssetDelivery to load the correct icon using _nativeAuth.ButtonIconKey)
    }

    private async void HandleNativeLogin()
    {
        //_view.ShowLoadingSpinner(true);


        // 1. Get the token from Apple/Google (The presenter doesn't care which)
        string nativeToken = await _nativeAuth.AuthenticateAsync();


        if (!string.IsNullOrEmpty(nativeToken))
        {
            // 2. Hand the token to PlayFab to actually log the player into your backend
            //bool success = await _playFabAuth.LoginWithNativeTokenAsync(_nativeAuth.PlatformName, nativeToken);

            //if (success)
            //{
            //    // Route to the Main Menu!
            //}
        }


        //_view.ShowLoadingSpinner(false);
    }

    public void Dispose()
    {
        //_view.OnNativeLoginClicked -= HandleNativeLogin;
    }
}