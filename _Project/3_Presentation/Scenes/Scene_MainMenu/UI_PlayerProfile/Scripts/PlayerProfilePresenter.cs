// Assets/_Project/Scenes/UI_PlayerProfile/Scripts/PlayerProfilePresenter.cs
using System;
using System.Text;
using System.Threading;
using Billiards.CoreDomain.Hardware;
using Billiards.CoreDomain.Services;
using Billiards.CoreDomain.Notifications;
using Billiards.Presentation;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using VContainer.Unity;


public class PlayerProfilePresenter : IDisposable
{
    private readonly PlayerProfileScreen _view;
    private readonly PlayerSession _session;
    private readonly IPlayerDataServiceProgression _playerData;
    private readonly IAvatarService _avatarService;
    private readonly IGalleryService _galleryService;
    private readonly IContentModerationService _moderationService;
    private readonly IServerImageModerationService _serverModerationService;
    private readonly AvatarUploadSettings _uploadSettings;
    private readonly Pg13Thresholds _thresholds;
    private readonly INotificationQueue _notificationQueue;

    private CancellationTokenSource _cts;
    private bool _isUploading;

    // Manually constructed by ProfileLifetimeScope (matching HomePresenter pattern)
    public PlayerProfilePresenter(
        PlayerProfileScreen view,
        PlayerSession session,
        IPlayerDataServiceProgression playerData,
        IAvatarService avatarService,
        IGalleryService galleryService,
        IContentModerationService moderationService,
        IServerImageModerationService serverModerationService,
        AvatarUploadSettings uploadSettings,
        Pg13Thresholds thresholds,
        INotificationQueue notificationQueue)
    {
        _view = view;
        _session = session;
        _playerData = playerData;
        _avatarService = avatarService;
        _galleryService = galleryService;
        _moderationService = moderationService;
        _serverModerationService = serverModerationService;
        _uploadSettings = uploadSettings;
        _thresholds = thresholds;
        _notificationQueue = notificationQueue;
        _cts = new CancellationTokenSource();
    }

    public void Initialize()
    {
        // 1. Initial State Setup from static session data (like HomePresenter)
        var profile = _session.CurrentProfile;
        _view.SetPlayerName(string.IsNullOrEmpty(profile.DisplayName) ? "Guest" : profile.DisplayName);
        
        // Dynamic state from player data service
        _view.SetCoinBalance(_playerData.CurrentCoins);

        // 2. Subscribe to user input
        _view.OnChooseImageClicked += HandleChooseImageClicked;

        // 3. Subscribe to backend data changes (Event-Driven!)
        _playerData.OnCoinsChanged += _view.SetCoinBalance;
        _playerData.OnNameChanged += _view.SetPlayerName;

        // 4. Fetch and display the avatar
        LoadAvatarAsync(_cts.Token).Forget();
    }

    private async UniTaskVoid LoadAvatarAsync(CancellationToken token)
    {
        string avatarUrl = await _avatarService.GetPlayerAvatarUrlAsync(token);
        if (string.IsNullOrEmpty(avatarUrl)) return;

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(avatarUrl))
        {
            await uwr.SendWebRequest().ToUniTask(cancellationToken: token);

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[PlayerProfilePresenter] Failed to download avatar image: {uwr.error}");
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                if (texture != null)
                {
                    // Convert Texture2D to Sprite and assign it to the target Image
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    _view.SetAvatarSprite(sprite);
                }
            }
        }
    }

    private void HandleChooseImageClicked()
    {
        if (_isUploading)
        {
            Debug.LogWarning("[PlayerProfilePresenter] Upload already in progress, ignoring.");
            return;
        }
        UploadGalleryAvatarAsync(_cts.Token).Forget();
    }

    private const int PopupHoldMs = 2500;
    private bool _isPopupShowing = false;

    private void SetOrUpdateStatusPopup(NotificationClassification classification, string title, string message)
    {
        if (_notificationQueue == null) return;

        if (!_isPopupShowing)
        {
            _notificationQueue.Enqueue(new NotificationData
            {
                Type = enNotificationType.SystemWarning,
                Classification = classification,
                Layout = NotificationLayout.StatusOverlay,
                SlideIn = NotificationSlideDirection.Immediate,
                SlideOut = NotificationSlideDirection.Immediate,
                Title = title,
                Message = message,
                DisplayDurationSeconds = 0
            });
            _isPopupShowing = true;
        }
        else
        {
            _notificationQueue.UpdateActiveClassification(classification);
            _notificationQueue.UpdateActiveMessage(title, message);
        }
    }

    private async UniTaskVoid UploadGalleryAvatarAsync(CancellationToken token)
    {
        _isUploading = true;
        _view.SetChooseImageInteractable(false);
        try
        {
            // Pick the image as LOSSLESS PNG bytes so moderation receives pixel-perfect
            // input. 2048 px matches Unity's default texture import max size and is the
            // upper end of OpenAI's useful detail tier. 25 MB source-file cap rejects
            // truly huge images (RAW, panoramas) before they can spike memory.
            const long maxSourceBytes = 25L * 1024 * 1024;
            var pick = await _galleryService.PickImageLosslessAsync(maxSize: 2048, maxFileSizeBytes: maxSourceBytes);

            switch (pick.Status)
            {
                case GalleryPickStatus.Cancelled:
                    Debug.Log("[PlayerProfilePresenter] Image picking cancelled.");
                    return; // no popup — user chose to back out
                case GalleryPickStatus.TooLarge:
                    Debug.LogWarning($"[PlayerProfilePresenter] Image is too large ({pick.FileSizeBytes / (1024 * 1024)} MB). Maximum is {maxSourceBytes / (1024 * 1024)} MB.");
                    await ShowPopupForMsAsync(NotificationClassification.Error, "Upload Error", $"Image too large ({pick.FileSizeBytes / (1024 * 1024)} MB). Pick one under {maxSourceBytes / (1024 * 1024)} MB.", token);
                    return;
                case GalleryPickStatus.LoadFailed:
                    Debug.LogError("[PlayerProfilePresenter] Failed to load the selected image.");
                    await ShowPopupForMsAsync(NotificationClassification.Error, "Upload Error", "Could not read image. Try a different file.", token);
                    return;
                case GalleryPickStatus.Loaded:
                    break; // continue below
            }

            byte[] moderationBytes = pick.Bytes;
            Debug.Log($"[PlayerProfilePresenter] Image loaded (lossless PNG, source {pick.FileSizeBytes / 1024} KB → decoded {moderationBytes.Length / 1024} KB).");

            // 1. PG13 moderation gate. Runs BEFORE any destructive Imgur calls so a
            //    rejected image cannot wipe the user's existing avatar.
            if (_moderationService == null)
            {
                Debug.LogWarning("[PlayerProfilePresenter] No moderation service available — refusing to upload unmoderated content.");
                await ShowPopupForMsAsync(NotificationClassification.Error, "Upload Error", "Moderation unavailable. Try again later.", token);
                return;
            }

            SetOrUpdateStatusPopup(NotificationClassification.Info, "Moderation", "Checking image...");
            Debug.Log("[PlayerProfilePresenter] Running PG13 moderation check...");
            var verdict = await _moderationService.CheckImageAsync(moderationBytes, token);
            Debug.Log(FormatModerationBreakdown("[PG13]", "vs. client-side inspector thresholds", verdict, _thresholds));

            if (verdict.DidError)
            {
                var timedOut = !string.IsNullOrEmpty(verdict.Reason) && verdict.Reason.Contains("timed out");
                var msg = timedOut ? "Check timed out. Try again." : "Could not check image. Try again.";
                await ShowPopupForMsAsync(NotificationClassification.Error, "Upload Error", msg, token);
                return;
            }

            if (verdict.IsAllowed)
            {
                // Brief positive feedback on a clean client check before moving to the server step.
                SetOrUpdateStatusPopup(NotificationClassification.Success, "Moderation", "Image approved");
                await UniTask.Delay(PopupHoldMs, cancellationToken: token);
            }
            else
            {
                Debug.LogWarning($"[PlayerProfilePresenter] Image REJECTED by client-side PG13 policy ({verdict.OffendingCategory} = {verdict.HighestScore:0.000}).");

                if (!_uploadSettings.ForceServerCheck)
                {
                    // Default path: trust the client gate and skip the server call. Toggle
                    // "Force Server Check" on BilliardsGameLifetimeScope to also run the
                    // server check on rejected uploads (surfaces drift report + server
                    // breakdown for verification work).
                    await ShowPopupForMsAsync(NotificationClassification.Error, "Upload Error", "Image rejected. Please pick a different one.", token);
                    return;
                }

                Debug.LogWarning("[PlayerProfilePresenter] ForceServerCheck is ON — calling server moderation anyway so the drift detector and server breakdown still print.");
            }

            // 2. SERVER-AUTHORITATIVE moderation via PlayFab CloudScript. On the happy
            //    path this is the hack-proof gate — its verdict overrides any client
            //    outcome. Reached after a client APPROVE, or after a client REJECT only
            //    when ForceServerCheck is enabled (debug-only path for surfacing the
            //    drift report and server breakdown regardless of outcome).
            //
            //    PlayFab Legacy CloudScript caps function arguments at ~32 KB
            //    (JSON-serialized). 256 px JPG q=70 typically lands at 8–15 KB raw
            //    → ~10–20 KB base64, comfortably under the limit even for detail-heavy
            //    photos. We accept the resolution hit because the server check is
            //    defence-in-depth — the client already gated at 2048 px above.
            const int ServerModerationMaxSize = 256;
            const int ServerModerationQuality = 70;
            byte[] serverBytes = ReencodeForUpload(moderationBytes, ServerModerationMaxSize, ServerModerationQuality);
            if (serverBytes == null)
            {
                Debug.LogError("[PlayerProfilePresenter] Failed to re-encode image for server moderation.");
                await ShowPopupForMsAsync(NotificationClassification.Error, "Upload Error", "Could not prepare image for server check.", token);
                return;
            }
            Debug.Log($"[PlayerProfilePresenter] Server-moderation payload: {serverBytes.Length / 1024} KB (after re-encode to {ServerModerationMaxSize} px JPG q={ServerModerationQuality}).");

            SetOrUpdateStatusPopup(NotificationClassification.Info, "Moderation", "Verifying with server...");
            Debug.Log("[PlayerProfilePresenter] Calling server-side moderation (PlayFab CloudScript)...");
            var serverResult = await _serverModerationService.ModerateAsync(serverBytes, token);

            // Verification log — this is what proves the CloudScript actually ran.
            // The serverVersion + serverTimestamp come from server.GetTitleInternalData()
            // and new Date().toISOString() inside the cloud script, so they cannot be
            // forged by a tampered client.
            if (!string.IsNullOrEmpty(serverResult.ServerVersion) || !string.IsNullOrEmpty(serverResult.ServerTimestamp))
            {
                Debug.Log($"[SERVER MODERATION] ✓ CloudScript responded. Server version: {serverResult.ServerVersion ?? "<none>"}, Server timestamp: {serverResult.ServerTimestamp ?? "<none>"}.");
            }
            else
            {
                Debug.LogWarning("[SERVER MODERATION] ⚠ No server version/timestamp returned — CloudScript may not be deployed correctly, OR a man-in-the-middle is faking the response. Aborting upload.");
                await ShowPopupForMsAsync(NotificationClassification.Error, "Upload Error", "Could not verify with server. Try again.", token);
                return;
            }

            var sv = serverResult.Verdict;

            if (sv.DidError)
            {
                Debug.LogError($"[SERVER MODERATION] CloudScript reported an error: {sv.Reason}");
                await ShowPopupForMsAsync(NotificationClassification.Error, "Upload Error", "Server moderation unavailable. Try again.", token);
                return;
            }

            // Server-side per-category breakdown (mirrors the client-side log).
            // Requires CloudScript revision moderate-avatar-1.1 or later. Older revisions
            // won't echo their thresholds — we fall back to a verdict-only log.
            if (serverResult.HasServerThresholds)
            {
                Debug.Log(FormatModerationBreakdown("[SERVER MODERATION]", "vs. server-side cloud script thresholds", sv, serverResult.ServerThresholds));

                // Strategy 1: client and server thresholds should match. Detect drift.
                var mismatchReport = FormatThresholdMismatch(_thresholds, serverResult.ServerThresholds);
                if (mismatchReport != null)
                {
                    Debug.LogWarning(mismatchReport);
                }
                else
                {
                    Debug.Log("[THRESHOLDS] ✓ Client and server thresholds match.");
                }
            }
            else
            {
                Debug.LogWarning("[SERVER MODERATION] ⚠ Server did not echo its thresholds — deployed CloudScript revision is older than moderate-avatar-1.1. Cannot detect threshold drift.");
                Debug.Log($"[SERVER MODERATION] Server verdict: {(sv.IsAllowed ? "APPROVED" : "REJECTED")} (highest: {sv.HighestCategory} = {sv.HighestScore:0.000}).");
            }

            // 3. Combined verdict. Either rejection blocks the upload; the client gets
            //    first say because it's the stricter gate (full-resolution image).
            if (!verdict.IsAllowed)
            {
                await ShowPopupForMsAsync(NotificationClassification.Error, "Upload Error", "Image rejected. Please pick a different one.", token);
                return;
            }
            if (!sv.IsAllowed)
            {
                Debug.LogWarning("[SECURITY] Server REJECTED an image the client APPROVED. Either OpenAI variance, or the client moderation was bypassed. Acting on server verdict.");
                await ShowPopupForMsAsync(NotificationClassification.Error, "Upload Error", "Image rejected by server. Please pick a different one.", token);
                return;
            }

            Debug.Log("[SERVER MODERATION] ✓ Client and server agree. Proceeding with upload.");

            // 4. Re-encode for Imgur (small JPG, no need for moderation-grade fidelity).
            //    Deferred until after both gates pass so a rejected upload doesn't pay
            //    for an encode it won't use.
            byte[] imageBytes = ReencodeForUpload(moderationBytes, maxSize: 512, jpgQuality: 85);
            if (imageBytes == null)
            {
                Debug.LogError("[PlayerProfilePresenter] Failed to re-encode image for upload after moderation passed.");
                await ShowPopupForMsAsync(NotificationClassification.Error, "Upload Error", "Could not prepare image for upload.", token);
                return;
            }
            Debug.Log($"[PlayerProfilePresenter] Re-encoded for Imgur. Size: {imageBytes.Length / 1024} KB");

            // 5. Delete previous avatar (best-effort; failure does not abort the upload).
            string existingDeleteHash = await _avatarService.GetAvatarDeleteHashAsync(token);
            if (!string.IsNullOrEmpty(existingDeleteHash))
            {
                Debug.Log("[PlayerProfilePresenter] Existing avatar found. Deleting old image...");
                bool deleted = await _avatarService.DeleteAvatarAsync(existingDeleteHash, token);
                Debug.Log(deleted
                    ? "[PlayerProfilePresenter] Old avatar successfully deleted from Imgur."
                    : "[PlayerProfilePresenter] Failed to delete old avatar, proceeding with new upload anyway.");
            }

            // 6. Upload — hard timeout wrapper so we can distinguish "failed" from "timed out".
            SetOrUpdateStatusPopup(NotificationClassification.Info, "Uploading", "Uploading avatar...");
            var timeoutSeconds = _uploadSettings.UploadTimeoutSeconds;
            using var uploadTimeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            using var uploadLinkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, uploadTimeoutCts.Token);

            string newUrl;
            try
            {
                Debug.Log("[PlayerProfilePresenter] Triggering new avatar upload...");
                newUrl = await _avatarService.UploadAvatarAsync(imageBytes, uploadLinkedCts.Token);
                Debug.Log($"[PlayerProfilePresenter] Upload complete. New URL: {newUrl}");
            }
            catch (OperationCanceledException) when (uploadTimeoutCts.IsCancellationRequested && !token.IsCancellationRequested)
            {
                Debug.LogWarning("[PlayerProfilePresenter] Upload timed out.");
                await ShowPopupForMsAsync(NotificationClassification.Error, "Upload Error", $"Upload timed out after {timeoutSeconds}s. Try again.", token);
                return;
            }
            catch (OperationCanceledException)
            {
                throw; // caller cancelled — let outer finally clean up
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayerProfilePresenter] Upload failed: {ex.Message}");
                var rateLimited = ex.Message.Contains("429") || ex.Message.Contains("Too Many Requests");
                var msg = rateLimited ? "Too many uploads. Wait a minute and try again." : "Upload failed. Try again.";
                await ShowPopupForMsAsync(NotificationClassification.Error, "Upload Error", msg, token);
                return;
            }

            // 7. Local preview to bypass Imgur CDN propagation delay.
            Texture2D localTex = new Texture2D(2, 2);
            if (localTex.LoadImage(imageBytes))
            {
                Sprite sprite = Sprite.Create(localTex, new Rect(0, 0, localTex.width, localTex.height), new Vector2(0.5f, 0.5f));
                _view.SetAvatarSprite(sprite);
            }

            await ShowPopupForMsAsync(NotificationClassification.Success, "Success", "Avatar updated!", token);
        }
        catch (OperationCanceledException)
        {
            // Caller token fired (e.g. scene unloading) — silently unwind.
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PlayerProfilePresenter] Error during gallery avatar upload: {ex.Message}");
            try { await ShowPopupForMsAsync(NotificationClassification.Error, "Upload Error", "Something went wrong. Try again.", token); }
            catch { /* token may already be dead */ }
        }
        finally
        {
            if (_isPopupShowing && _notificationQueue != null)
            {
                _notificationQueue.DismissActive();
                _isPopupShowing = false;
            }
            _view.SetChooseImageInteractable(true);
            _isUploading = false;
        }
    }

    private async UniTask ShowPopupForMsAsync(NotificationClassification classification, string title, string message, CancellationToken token)
    {
        SetOrUpdateStatusPopup(classification, title, message);
        await UniTask.Delay(PopupHoldMs, cancellationToken: token);
    }

    private void HandleCloseClicked()
    {
        // Tell the router to unload this additive scene
        //_router.CloseMenuAsync("UI_PlayerProfile");
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        // CRITICAL: Always unsubscribe when the scene unloads to prevent memory leaks!
        _view.OnChooseImageClicked -= HandleChooseImageClicked;
        _playerData.OnCoinsChanged -= _view.SetCoinBalance;
        _playerData.OnNameChanged -= _view.SetPlayerName;
    }

    /// <summary>
    /// Decodes lossless PNG bytes, downsamples to maxSize (longest side) via GPU blit,
    /// and re-encodes as JPG for upload. Keeps the moderation-time image lossless while
    /// shipping a small JPG to Imgur.
    /// </summary>
    private static byte[] ReencodeForUpload(byte[] pngBytes, int maxSize, int jpgQuality)
    {
        var source = new Texture2D(2, 2);
        if (!source.LoadImage(pngBytes))
        {
            UnityEngine.Object.Destroy(source);
            return null;
        }

        try
        {
            int srcW = source.width;
            int srcH = source.height;
            int longest = Mathf.Max(srcW, srcH);

            // If already within budget, encode directly (no blit needed).
            if (longest <= maxSize)
                return source.EncodeToJPG(jpgQuality);

            float scale = (float)maxSize / longest;
            int dstW = Mathf.Max(1, Mathf.RoundToInt(srcW * scale));
            int dstH = Mathf.Max(1, Mathf.RoundToInt(srcH * scale));

            RenderTexture rt = RenderTexture.GetTemporary(dstW, dstH, 0, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            Graphics.Blit(source, rt);
            RenderTexture.active = rt;

            var dst = new Texture2D(dstW, dstH, TextureFormat.RGBA32, false);
            dst.ReadPixels(new Rect(0, 0, dstW, dstH), 0, 0);
            dst.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            try { return dst.EncodeToJPG(jpgQuality); }
            finally { UnityEngine.Object.Destroy(dst); }
        }
        finally
        {
            UnityEngine.Object.Destroy(source);
        }
    }

    /// <summary>
    /// Pretty-prints the full per-category score breakdown for a moderation verdict
    /// against the actual thresholds that produced it. Used for BOTH the client-side
    /// log (with inspector-tuned thresholds) and the server-side log (with the
    /// thresholds echoed back by the CloudScript).
    /// </summary>
    private static string FormatModerationBreakdown(string label, string thresholdSource, ContentModerationResult verdict, Pg13Thresholds t)
    {
        var s = verdict.Scores;
        var header = verdict.IsAllowed
            ? $"{label} PASSED (highest: {verdict.HighestCategory} = {verdict.HighestScore:0.000})"
            : $"{label} FLAGGED ({verdict.OffendingCategory} = {verdict.HighestScore:0.000})";

        var sb = new StringBuilder(384);
        sb.AppendLine(header);
        sb.AppendLine($"Category Breakdown ({thresholdSource}):");
        AppendRow(sb, "sexual",           s.Sexual,          t.Sexual);
        AppendRow(sb, "sexual/minors",    s.SexualMinors,    t.SexualMinors);
        AppendRow(sb, "violence",         s.Violence,        t.Violence);
        AppendRow(sb, "violence/graphic", s.ViolenceGraphic, t.ViolenceGraphic);
        AppendRow(sb, "harassment",       s.Harassment,      t.Harassment);
        AppendRow(sb, "hate",             s.Hate,            t.Hate);
        AppendRow(sb, "hate/threatening", s.HateThreatening, t.HateThreatening);
        AppendRow(sb, "self-harm",        s.SelfHarm,        t.SelfHarm);
        return sb.ToString();
    }

    /// <summary>
    /// Detects per-category drift between client-side inspector thresholds and the
    /// thresholds the CloudScript actually used. Returns null when they match (no log
    /// noise on the happy path), or a multi-line report when they don't.
    /// </summary>
    private static string FormatThresholdMismatch(Pg13Thresholds client, Pg13Thresholds server)
    {
        const float Eps = 0.0005f;
        var sb = new StringBuilder(256);
        bool any = false;

        void Check(string name, float c, float s)
        {
            if (Mathf.Abs(c - s) > Eps)
            {
                if (!any)
                {
                    sb.AppendLine("[THRESHOLD MISMATCH] Client and server PG13 thresholds disagree (Strategy 1 expects them to match):");
                    any = true;
                }
                sb.AppendLine($"  {name,-18} client: {c:0.000}  |  server: {s:0.000}");
            }
        }

        Check("sexual",           client.Sexual,          server.Sexual);
        Check("sexual/minors",    client.SexualMinors,    server.SexualMinors);
        Check("violence",         client.Violence,        server.Violence);
        Check("violence/graphic", client.ViolenceGraphic, server.ViolenceGraphic);
        Check("harassment",       client.Harassment,      server.Harassment);
        Check("hate",             client.Hate,            server.Hate);
        Check("hate/threatening", client.HateThreatening, server.HateThreatening);
        Check("self-harm",        client.SelfHarm,        server.SelfHarm);

        return any ? sb.ToString() : null;
    }

    private static void AppendRow(StringBuilder sb, string name, float score, float threshold)
    {
        var flagged = score > threshold;
        var marker = flagged ? "⚠" : " ";
        sb.AppendLine($"  {marker} {name,-18} {score:0.000}  (threshold: {threshold:0.000})");
    }
}
