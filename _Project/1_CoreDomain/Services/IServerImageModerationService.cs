// Assets/_Project/1_CoreDomain/Services/IServerImageModerationService.cs
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Billiards.CoreDomain.Services
{
    /// <summary>
    /// Server-authoritative image moderation. The implementation calls out to a
    /// trusted backend (PlayFab CloudScript) which holds the OpenAI API key as a
    /// server secret and applies thresholds there. Clients cannot bypass this gate
    /// by editing local code or intercepting outbound traffic, because the
    /// classification decision is made on the server.
    ///
    /// Distinct from <see cref="IContentModerationService"/>: the latter runs
    /// locally for fast UX feedback, this one is the authoritative check before
    /// any destructive (Imgur) action.
    /// </summary>
    public interface IServerImageModerationService
    {
        UniTask<ServerModerationVerdict> ModerateAsync(byte[] imageBytes, CancellationToken token);
    }
}