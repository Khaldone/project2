using Cysharp.Threading.Tasks;

namespace Billiards.CoreDomain.Services
{
    public interface IAuthenticationService_New
    {
        // Returns true if successful, false if failed
        UniTask<bool> AuthenticateAsync();

        /// <summary>
        /// Authenticates with a platform-specific native token (e.g., Google Server Auth Code, Apple Identity Token).
        /// </summary>
        /// <param name="platformName">The platform identifier ("Google" or "Apple").</param>
        /// <param name="token">The native token retrieved from the platform SDK.</param>
        /// <returns>True if PlayFab authentication succeeded.</returns>
        UniTask<bool> AuthenticateWithNativeTokenAsync(string platformName, string token);

        /// <summary>
        /// Attempts to silently auto-login using CBS's cached credentials.
        /// </summary>
        UniTask<bool> AutoLoginAsync();
    }
}