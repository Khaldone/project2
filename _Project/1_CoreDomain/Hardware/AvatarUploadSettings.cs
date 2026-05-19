// Assets/_Project/1_CoreDomain/Hardware/AvatarUploadSettings.cs
namespace Billiards.CoreDomain.Hardware
{
    /// <summary>
    /// Tunable timing/sizing knobs for the avatar upload flow. Pure data envelope —
    /// populated from inspector fields on the bootstrap scope, consumed by the
    /// player-profile presenter.
    /// </summary>
    public readonly struct AvatarUploadSettings
    {
        /// <summary>
        /// Hard timeout (in seconds) for the Imgur upload request. When exceeded the
        /// presenter aborts via a linked CancellationTokenSource and surfaces a
        /// distinct "timed out" message instead of a generic failure.
        /// </summary>
        public readonly int UploadTimeoutSeconds;

        /// <summary>
        /// When true, the server-side moderation check runs even when the client-side
        /// check has already rejected the image. Used to surface the per-category
        /// server breakdown and the client/server threshold-drift report regardless of
        /// outcome. When false (default) a client rejection short-circuits the flow
        /// and skips the server call — faster UX and no wasted OpenAI hit.
        /// </summary>
        public readonly bool ForceServerCheck;

        public AvatarUploadSettings(int uploadTimeoutSeconds, bool forceServerCheck = false)
        {
            UploadTimeoutSeconds = uploadTimeoutSeconds > 0 ? uploadTimeoutSeconds : 30;
            ForceServerCheck = forceServerCheck;
        }

        public static AvatarUploadSettings Default => new AvatarUploadSettings(30, false);
    }
}