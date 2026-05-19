// Assets/_Project/1_CoreDomain/Services/IAvatarService.cs
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Billiards.CoreDomain.Services
{
    public interface IAvatarService
    {
        /// <summary>
        /// Uploads raw image bytes to an external host and sets the authenticated user's avatar URL.
        /// </summary>
        UniTask<string> UploadAvatarAsync(byte[] imageData, CancellationToken token);

        /// <summary>
        /// Gets the current player's avatar URL from the backend.
        /// </summary>
        UniTask<string> GetPlayerAvatarUrlAsync(CancellationToken token);

        /// <summary>
        /// Gets the delete hash for the current avatar, if one exists.
        /// </summary>
        UniTask<string> GetAvatarDeleteHashAsync(CancellationToken token);

        /// <summary>
        /// Deletes an avatar from the external host using its delete hash.
        /// </summary>
        UniTask<bool> DeleteAvatarAsync(string deleteHash, CancellationToken token);
    }
}
