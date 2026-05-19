// Assets/_Project/1_CoreDomain/Services/IContentModerationService.cs
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Billiards.CoreDomain.Services
{
    /// <summary>
    /// Validates user-supplied content against the game's rating policy (PG13)
    /// before it can be persisted to a public service (Imgur, etc.).
    /// Infrastructure layer wraps the actual moderation provider (OpenAI today).
    /// </summary>
    public interface IContentModerationService
    {
        /// <summary>
        /// Submits raw image bytes (PNG or JPG) for content classification and
        /// returns a verdict resolved against the configured rating thresholds.
        /// The implementation MUST NOT throw on network failure — callers rely on
        /// <see cref="ContentModerationResult"/> to distinguish error from rejection.
        /// </summary>
        UniTask<ContentModerationResult> CheckImageAsync(byte[] imageBytes, CancellationToken token);
    }
}
