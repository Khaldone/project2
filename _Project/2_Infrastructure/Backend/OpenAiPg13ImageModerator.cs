// Assets/_Project/2_Infrastructure/Backend/OpenAiPg13ImageModerator.cs
using System;
using System.Threading;
using AiToolbox;
using Billiards.CoreDomain.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Billiards.Infrastructure.Backend
{
    /// <summary>
    /// Bridges the AI Toolbox OpenAI moderation API into the CoreDomain
    /// <see cref="IContentModerationService"/> contract. Applies the project's
    /// PG13 thresholds (<see cref="Pg13ImagePolicy"/>) to the returned scores.
    /// Honours a hard timeout — if OpenAI doesn't respond within
    /// <paramref name="timeoutSeconds"/>, the request is aborted and an Error
    /// verdict is returned (which the presenter treats as fail-closed).
    ///
    /// SECURITY: this is a client-side gate. The API key ships with the build
    /// and a determined attacker can bypass this call entirely. Treat as a
    /// soft filter; pair with server-side moderation before public release.
    /// </summary>
    public sealed class OpenAiPg13ImageModerator : IContentModerationService
    {
        private readonly ModerationParameters _parameters;
        private readonly Pg13Thresholds _thresholds;
        private readonly int _timeoutSeconds;

        public OpenAiPg13ImageModerator(
            ModerationParameters parameters,
            Pg13Thresholds thresholds,
            int timeoutSeconds)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _thresholds = thresholds;
            _timeoutSeconds = timeoutSeconds > 0 ? timeoutSeconds : 12;
        }

        public async UniTask<ContentModerationResult> CheckImageAsync(byte[] imageBytes, CancellationToken token)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return ContentModerationResult.Error("Image payload was empty.");

            if (string.IsNullOrEmpty(_parameters.apiKey)
                && _parameters.apiKeyEncryption == ApiKeyEncryption.None)
            {
                return ContentModerationResult.Error("Moderation API key is not configured.");
            }

            // Texture2D.LoadImage auto-detects PNG/JPG. Allocate then dispose
            // deterministically so we don't leak GPU memory on the avatar path.
            var texture = new Texture2D(2, 2);
            if (!texture.LoadImage(imageBytes))
            {
                UnityEngine.Object.Destroy(texture);
                return ContentModerationResult.Error("Could not decode image bytes (unsupported format).");
            }

            // Linked CTS: caller-cancellation OR our hard timeout, whichever fires first.
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutCts.Token);
            var linkedToken = linkedCts.Token;

            var tcs = new UniTaskCompletionSource<(ImageCategoryScores scores, bool flagged)>();
            Action cancel = null;

            try
            {
                cancel = Moderation.RequestImage(
                    texture,
                    _parameters,
                    response =>
                    {
                        var mapped = Map(response);
                        tcs.TrySetResult(mapped);
                    },
                    (errorCode, errorMsg) =>
                    {
                        tcs.TrySetException(new Exception($"OpenAI moderation failed ({errorCode}): {errorMsg}"));
                    });

                using (linkedToken.Register(() =>
                {
                    try { cancel?.Invoke(); } catch { /* AI Toolbox throws on double-cancel */ }
                    tcs.TrySetCanceled();
                }))
                {
                    var result = await tcs.Task;
                    return Pg13ImagePolicy.Evaluate(result.scores, result.flagged, _thresholds);
                }
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !token.IsCancellationRequested)
            {
                // Hard timeout fired (not user cancellation). Convert to a fail-closed
                // Error verdict so the presenter aborts the upload.
                return ContentModerationResult.Error($"Moderation timed out after {_timeoutSeconds}s — aborting upload.");
            }
            catch (OperationCanceledException)
            {
                // Caller's own token fired — propagate so the upload flow unwinds.
                throw;
            }
            catch (Exception ex)
            {
                return ContentModerationResult.Error(ex.Message);
            }
            finally
            {
                UnityEngine.Object.Destroy(texture);
            }
        }

        private static (ImageCategoryScores scores, bool flagged) Map(ModerationResponse response)
        {
            if (response.results == null || response.results.Length == 0)
                return (default, false);

            var r = response.results[0];
            var cs = r.category_scores;

            var scores = new ImageCategoryScores(
                sexual:          cs.sexual,
                sexualMinors:    cs.sexualMinors,
                violence:        cs.violence,
                violenceGraphic: cs.violenceGraphic,
                harassment:      cs.harassment,
                hate:            cs.hate,
                hateThreatening: cs.hateThreatening,
                selfHarm:        cs.selfHarm);

            return (scores, r.flagged);
        }
    }
}