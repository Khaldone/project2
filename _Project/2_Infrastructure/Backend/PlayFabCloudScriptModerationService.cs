// Assets/_Project/2_Infrastructure/Backend/PlayFabCloudScriptModerationService.cs
using System;
using System.Threading;
using Billiards.CoreDomain.Services;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Billiards.Infrastructure.Backend
{
    /// <summary>
    /// Invokes a PlayFab CloudScript function ("ModerateAvatarImage") that performs
    /// server-side OpenAI moderation. The OpenAI API key lives in PlayFab Title
    /// Internal Data — never reaches the client — so the moderation decision cannot
    /// be skipped or forged by a tampered build.
    ///
    /// Returns a <see cref="ServerModerationVerdict"/> carrying both the verdict and
    /// metadata (server version, server timestamp) that prove the response came
    /// from the deployed function and not from local mock state.
    /// </summary>
    public sealed class PlayFabCloudScriptModerationService : IServerImageModerationService
    {
        private const string FunctionName = "ModerateAvatarImage";

        // PlayFab Legacy CloudScript caps the JSON-serialised FunctionParameter at
        // ~32 KB. Anything over fails with CloudScriptFunctionArgumentSizeExceeded
        // BEFORE the script even runs. We guard with a slightly tighter ceiling so
        // the JSON wrapping overhead doesn't push us over.
        private const int MaxFunctionArgumentBytes = 30 * 1024;

        public async UniTask<ServerModerationVerdict> ModerateAsync(byte[] imageBytes, CancellationToken token)
        {
            Debug.Log($"[PlayFabCloudScriptModerationService] ModerateAsync entered. Image size: {imageBytes?.Length ?? 0} bytes.");

            if (imageBytes == null || imageBytes.Length == 0)
            {
                Debug.LogWarning("[PlayFabCloudScriptModerationService] Empty image payload — aborting.");
                return new ServerModerationVerdict(
                    ContentModerationResult.Error("Image payload was empty."),
                    serverVersion: null, serverTimestamp: null);
            }

            if (!PlayFabClientAPI.IsClientLoggedIn())
            {
                Debug.LogError("[PlayFabCloudScriptModerationService] PlayFab client is NOT logged in. Cannot call ExecuteCloudScript. Verify the auth flow ran before this point.");
                return new ServerModerationVerdict(
                    ContentModerationResult.Error("PlayFab not logged in — cannot reach the server."),
                    serverVersion: null, serverTimestamp: null);
            }

            var base64 = Convert.ToBase64String(imageBytes);
            Debug.Log($"[PlayFabCloudScriptModerationService] Base64 encoded ({base64.Length} chars). Building ExecuteCloudScriptRequest...");

            // Pre-flight size check: base64 + JSON envelope must fit PlayFab's 32 KB
            // function-argument limit. Fail closed with an actionable message rather
            // than letting PlayFab return the cryptic "CloudScriptFunctionArgumentSizeExceeded".
            if (base64.Length > MaxFunctionArgumentBytes)
            {
                var msg = $"Server moderation payload too large: {base64.Length / 1024} KB base64 exceeds PlayFab's ~32 KB function-argument cap. " +
                          $"Re-encode the image at lower resolution or quality before calling, or migrate to CloudScript V2 (Azure Functions) for higher limits.";
                Debug.LogError($"[PlayFabCloudScriptModerationService] {msg}");
                return new ServerModerationVerdict(
                    ContentModerationResult.Error(msg),
                    serverVersion: null, serverTimestamp: null);
            }

            var request = new ExecuteCloudScriptRequest
            {
                FunctionName = FunctionName,
                FunctionParameter = new
                {
                    imageBase64 = base64,
                    mimeType = "image/jpeg",
                },
                GeneratePlayStreamEvent = false,
            };

            Debug.Log($"[PlayFabCloudScriptModerationService] Calling PlayFabClientAPI.ExecuteCloudScript(\"{FunctionName}\")...");

            var tcs = new UniTaskCompletionSource<ExecuteCloudScriptResult>();
            PlayFabClientAPI.ExecuteCloudScript(
                request,
                result =>
                {
                    Debug.Log($"[PlayFabCloudScriptModerationService] PlayFab SUCCESS callback fired. FunctionResult: {(result?.FunctionResult == null ? "<null>" : "<present>")}, Error: {(result?.Error == null ? "<none>" : result.Error.Message)}.");
                    tcs.TrySetResult(result);
                },
                error =>
                {
                    var report = error?.GenerateErrorReport() ?? "<unknown error>";
                    Debug.LogError($"[PlayFabCloudScriptModerationService] PlayFab ERROR callback fired: {report}");
                    tcs.TrySetException(new Exception(report));
                });

            Debug.Log("[PlayFabCloudScriptModerationService] Awaiting PlayFab response (typical 2–8 s; up to 30 s if OpenAI is slow)...");

            ExecuteCloudScriptResult cloudResult;
            try
            {
                cloudResult = await tcs.Task.AttachExternalCancellation(token);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[PlayFabCloudScriptModerationService] Awaiting cancelled by caller token.");
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayFabCloudScriptModerationService] Await threw: {ex.Message}");
                return new ServerModerationVerdict(
                    ContentModerationResult.Error($"CloudScript request failed: {ex.Message}"),
                    serverVersion: null, serverTimestamp: null);
            }

            Debug.Log("[PlayFabCloudScriptModerationService] Response received. Inspecting payload...");

            if (cloudResult.Error != null)
            {
                Debug.LogError($"[PlayFabCloudScriptModerationService] CloudScript runtime error: {cloudResult.Error.Message} (StackTrace: {cloudResult.Error.StackTrace})");
                return new ServerModerationVerdict(
                    ContentModerationResult.Error($"CloudScript runtime error: {cloudResult.Error.Message}"),
                    serverVersion: null, serverTimestamp: null);
            }

            if (cloudResult.FunctionResult == null)
            {
                Debug.LogError("[PlayFabCloudScriptModerationService] FunctionResult is null. Is the function name correct? Is the script deployed?");
                return new ServerModerationVerdict(
                    ContentModerationResult.Error("CloudScript returned no result payload."),
                    serverVersion: null, serverTimestamp: null);
            }

            // PlayFab returns FunctionResult as an object — its ToString() is the JSON
            // representation. Round-trip through Unity's JsonUtility into our typed POCO.
            CloudScriptResponse response;
            string rawJson;
            try
            {
                rawJson = cloudResult.FunctionResult.ToString();
                Debug.Log($"[PlayFabCloudScriptModerationService] Raw FunctionResult JSON ({rawJson?.Length ?? 0} chars): {rawJson}");
                response = JsonUtility.FromJson<CloudScriptResponse>(rawJson);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayFabCloudScriptModerationService] Failed to parse response: {ex.Message}");
                return new ServerModerationVerdict(
                    ContentModerationResult.Error($"Could not parse CloudScript response: {ex.Message}"),
                    serverVersion: null, serverTimestamp: null);
            }

            if (response == null)
            {
                return new ServerModerationVerdict(
                    ContentModerationResult.Error("CloudScript response was null after parsing."),
                    serverVersion: null, serverTimestamp: null);
            }

            if (!string.IsNullOrEmpty(response.error))
            {
                return new ServerModerationVerdict(
                    ContentModerationResult.Error($"Server moderation error: {response.error}"),
                    serverVersion: response.serverVersion,
                    serverTimestamp: response.serverTimestamp);
            }

            // Build the canonical CoreDomain types from the server payload.
            var scores = response.scores != null
                ? new ImageCategoryScores(
                    sexual:          response.scores.sexual,
                    sexualMinors:    response.scores.sexualMinors,
                    violence:        response.scores.violence,
                    violenceGraphic: response.scores.violenceGraphic,
                    harassment:      response.scores.harassment,
                    hate:            response.scores.hate,
                    hateThreatening: response.scores.hateThreatening,
                    selfHarm:        response.scores.selfHarm)
                : default;

            var highestCategory = response.offendingCategory ?? "unknown";
            var highestScore = response.offendingScore;

            var verdict = response.allowed
                ? ContentModerationResult.Allowed(highestCategory, highestScore, scores)
                : ContentModerationResult.Rejected(response.offendingCategory ?? "unknown",
                                                   response.offendingScore,
                                                   highestCategory, highestScore, scores);

            // Server thresholds were added in moderate-avatar-1.1. If the deployed
            // CloudScript revision is older it won't echo them back, so we flag the
            // result as not having server thresholds and the presenter skips the
            // server-side breakdown log accordingly.
            var hasServerThresholds = response.thresholds != null;
            var serverThresholds = hasServerThresholds
                ? new Pg13Thresholds(
                    sexual:          response.thresholds.sexual,
                    sexualMinors:    response.thresholds.sexualMinors,
                    violence:        response.thresholds.violence,
                    violenceGraphic: response.thresholds.violenceGraphic,
                    harassment:      response.thresholds.harassment,
                    hate:            response.thresholds.hate,
                    hateThreatening: response.thresholds.hateThreatening,
                    selfHarm:        response.thresholds.selfHarm)
                : default;

            return new ServerModerationVerdict(verdict, response.serverVersion, response.serverTimestamp, serverThresholds, hasServerThresholds);
        }

        [Serializable]
        private class CloudScriptResponse
        {
            public bool allowed;
            public string offendingCategory;
            public float offendingScore;
            public ScoresPayload scores;
            public ScoresPayload thresholds; // server's PG13 thresholds (added in moderate-avatar-1.1)
            public bool providerFlagged;
            public string serverVersion;
            public string serverTimestamp;
            public string error;
        }

        // Same shape for raw scores and thresholds — both are 8 floats keyed by category.
        [Serializable]
        private class ScoresPayload
        {
            public float sexual;
            public float sexualMinors;
            public float violence;
            public float violenceGraphic;
            public float harassment;
            public float hate;
            public float hateThreatening;
            public float selfHarm;
        }
    }
}