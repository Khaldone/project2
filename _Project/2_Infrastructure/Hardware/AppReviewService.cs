// Assets/_Project/2_Infrastructure/Hardware/AppReviewService.cs
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Billiards.CoreDomain.Services;

#if UNITY_ANDROID || UNITY_EDITOR
using Google.Play.Common;
using Google.Play.Review;
#endif

namespace Billiards.Infrastructure.Hardware
{
    /// <summary>
    /// Data structure representing the persisted review state.
    /// </summary>
    [Serializable]
    public struct AppReviewSaveData
    {
        public int LaunchCount;
        public bool HasReviewed;
    }

    /// <summary>
    /// Platform implementation of IAppReviewService managing native reviews and PlayerPrefs-like ILocalSaveService persistence.
    /// </summary>
    public class AppReviewService : IAppReviewService
    {
        private const string SaveKey = "AppReviewState";

        private readonly ILocalSaveService _saveService;
        private readonly AppReviewConfig _config;

        public int LaunchCount { get; private set; }
        public bool HasReviewed { get; private set; }
        public bool IsEligibleForReview => !HasReviewed && LaunchCount >= _config.LaunchCountsBeforeShowingPopup;

        private bool _isInitialized;
        private bool _isInitializing;

        public AppReviewService(ILocalSaveService saveService, AppReviewConfig config)
        {
            _saveService = saveService;
            _config = config;
        }

        /// <summary>
        /// Loads persistence state and increments the boot launch counter.
        /// Idempotent — safe to call from multiple sites. The first caller
        /// runs the I/O; concurrent callers poll until it finishes.
        /// </summary>
        public async UniTask InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_isInitialized) return;

            if (_isInitializing)
            {
                // Another caller is already running init — wait for it to finish.
                await UniTask.WaitUntil(() => _isInitialized, cancellationToken: cancellationToken);
                return;
            }

            _isInitializing = true;

            Debug.Log("[AppReviewService] Starting InitializeAsync...");

            try
            {
                var state = await _saveService.LoadDataAsync<AppReviewSaveData>(SaveKey, new AppReviewSaveData());
                Debug.Log($"[AppReviewService] Loaded state: LaunchCount={state.LaunchCount}, HasReviewed={state.HasReviewed}");

                LaunchCount = state.LaunchCount + 1;
                HasReviewed = state.HasReviewed;

                await SaveStateAsync();

                Debug.Log($"[AppReviewService] Initialization complete. LaunchCount: {LaunchCount}, HasReviewed: {HasReviewed}, Eligible: {IsEligibleForReview} (Threshold: {_config.LaunchCountsBeforeShowingPopup})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AppReviewService] Error during InitializeAsync: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                _isInitialized = true;
                _isInitializing = false;
            }
        }

        /// <summary>
        /// Triggers native system store review flow.
        /// </summary>
        public async UniTask<bool> RequestNativeReviewAsync(CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Log("[AppReviewService] RequestNativeReviewAsync called in Editor. Simulating success.");
            await UniTask.Delay(100, cancellationToken: cancellationToken);
            return true;
#elif UNITY_ANDROID
            try
            {
                var reviewManager = new ReviewManager();
                var requestFlowOp = reviewManager.RequestReviewFlow();
                await WaitOperation(requestFlowOp);

                if (requestFlowOp.Error != ReviewErrorCode.NoError)
                {
                    Debug.LogError($"[AppReviewService] RequestReviewFlow failed: {requestFlowOp.Error}");
                    return false;
                }

                var reviewInfo = requestFlowOp.GetResult();
                var launchFlowOp = reviewManager.LaunchReviewFlow(reviewInfo);
                await WaitOperation(launchFlowOp);

                if (launchFlowOp.Error != ReviewErrorCode.NoError)
                {
                    Debug.LogError($"[AppReviewService] LaunchReviewFlow failed: {launchFlowOp.Error}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
#elif UNITY_IOS
            try
            {
                UnityEngine.iOS.Device.RequestStoreReview();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
#else
            await UniTask.CompletedTask;
            return false;
#endif
        }

        /// <summary>
        /// Opens the store listing for the app without leaving the application.
        /// On Android, uses the <c>market://</c> URI scheme so the Play Store
        /// renders as an in-app overlay (the user never leaves the game).
        /// The package name is extracted from the configured <c>LinkToTheGameAndroid</c>
        /// URL (the <c>?id=</c> query parameter) so it always targets the intended app,
        /// not this app's own bundle ID.
        /// </summary>
        public void OpenAppStoreReviewPage()
        {
#if UNITY_EDITOR
            // In the Editor, open whichever link is configured so the developer can verify it.
            string link = !string.IsNullOrEmpty(_config.LinkToTheGameAndroid)
                ? _config.LinkToTheGameAndroid
                : _config.LinkToTheGameIOS;
            if (!string.IsNullOrEmpty(link))
            {
                Debug.Log($"[AppReviewService] Editor: Opening URL: {link}");
                Application.OpenURL(link);
            }
            else
            {
                Debug.LogWarning("[AppReviewService] No store link configured in Editor.");
            }
#elif UNITY_ANDROID
            // Extract the package name from the configured https://play.google.com URL
            // (the ?id=com.xxx.yyy query parameter) and build a market:// intent from it.
            // market:// is routed by Android to the Play Store app as an in-app overlay —
            // the user never leaves the game.
            string packageId = ExtractAndroidPackageId(_config.LinkToTheGameAndroid);

            if (!string.IsNullOrEmpty(packageId))
            {
                string marketUrl = $"market://details?id={packageId}";
                Debug.Log($"[AppReviewService] Opening Play Store overlay: {marketUrl} (package: {packageId})");
                Application.OpenURL(marketUrl);
            }
            else if (!string.IsNullOrEmpty(_config.LinkToTheGameAndroid))
            {
                // URL is set but ID couldn't be parsed — fall back to the raw URL.
                Debug.LogWarning($"[AppReviewService] Could not parse package ID from '{_config.LinkToTheGameAndroid}'. Falling back to direct URL.");
                Application.OpenURL(_config.LinkToTheGameAndroid);
            }
            else
            {
                Debug.LogWarning("[AppReviewService] LinkToTheGameAndroid is not configured.");
            }
#elif UNITY_IOS
            if (!string.IsNullOrEmpty(_config.LinkToTheGameIOS))
            {
                Application.OpenURL(_config.LinkToTheGameIOS);
            }
            else
            {
                Debug.LogWarning("[AppReviewService] LinkToTheGameIOS is not configured.");
            }
#else
            Debug.LogWarning("[AppReviewService] OpenAppStoreReviewPage not supported on this platform.");
#endif
        }

        /// <summary>
        /// Parses the <c>id=</c> query parameter from a Play Store HTTPS URL.
        /// Example input:  https://play.google.com/store/apps/details?id=com.supercell.brawlstars
        /// Example output: com.supercell.brawlstars
        /// Returns null if the URL is malformed or does not contain an <c>id</c> parameter.
        /// </summary>
        private static string ExtractAndroidPackageId(string storeUrl)
        {
            if (string.IsNullOrEmpty(storeUrl)) return null;
            try
            {
                // Cheap manual parse — avoids allocating a Uri object on the game thread.
                const string marker = "id=";
                int idx = storeUrl.IndexOf(marker, System.StringComparison.OrdinalIgnoreCase);
                if (idx < 0) return null;
                int start = idx + marker.Length;
                int end = storeUrl.IndexOf('&', start);
                return end < 0
                    ? storeUrl.Substring(start)
                    : storeUrl.Substring(start, end - start);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Postpones the prompt by resetting launch counter.
        /// </summary>
        public async UniTask PostponeReviewAsync()
        {
            LaunchCount = 0;
            await SaveStateAsync();
            Debug.Log("[AppReviewService] App review postponed. LaunchCount reset.");
        }

        /// <summary>
        /// Permanently sets the reviewed flag.
        /// </summary>
        public async UniTask MarkAsReviewedAsync()
        {
            HasReviewed = true;
            await SaveStateAsync();
            Debug.Log("[AppReviewService] App review marked as completed.");
        }

        private async UniTask SaveStateAsync()
        {
            var state = new AppReviewSaveData
            {
                LaunchCount = LaunchCount,
                HasReviewed = HasReviewed
            };
            Debug.Log($"[AppReviewService] Saving state: LaunchCount={LaunchCount}, HasReviewed={HasReviewed}");
            await _saveService.SaveDataAsync(SaveKey, state);
            Debug.Log("[AppReviewService] Save state complete.");
        }

#if UNITY_ANDROID || UNITY_EDITOR
        private async UniTask WaitOperation<TResult, TError>(PlayAsyncOperation<TResult, TError> op)
            where TError : struct
        {
            while (!op.IsDone)
            {
                await UniTask.Yield();
            }
        }
#endif
    }
}
