// Assets/_Project/CoreDomain/Authentication/INativeAuthService.cs
using Cysharp.Threading.Tasks;
public interface INativeAuthService
{
    // The UI will read these to configure the button visually
    string PlatformName { get; }
    string ButtonIconKey { get; } // For Addressables (e.g., "icon_apple", "icon_google")


    // Returns the secure token (Identity Token for Apple, Server Auth Code for Google)
    // that PlayFab needs to verify the user.
    UniTask<string> AuthenticateAsync();

    // Attempts to silently authenticate without prompting the user. Returns token or null.
    UniTask<string> TrySilentAuthenticateAsync();
}