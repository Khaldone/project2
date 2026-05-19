// Assets/_Project/1_CoreDomain/Services/ServerModerationVerdict.cs
namespace Billiards.CoreDomain.Services
{
    /// <summary>
    /// Wraps a <see cref="ContentModerationResult"/> from the server with metadata
    /// that proves the cloud script actually ran (version string + ISO-8601 server
    /// timestamp) plus the thresholds it evaluated against. <see cref="HasServerThresholds"/>
    /// is false when an older CloudScript revision is deployed that doesn't return
    /// its thresholds in the response — in that case the presenter falls back to
    /// logging only the client-side breakdown.
    /// </summary>
    public readonly struct ServerModerationVerdict
    {
        public readonly ContentModerationResult Verdict;
        public readonly string ServerVersion;
        public readonly string ServerTimestamp;
        public readonly Pg13Thresholds ServerThresholds;
        public readonly bool HasServerThresholds;

        public ServerModerationVerdict(
            ContentModerationResult verdict,
            string serverVersion,
            string serverTimestamp,
            Pg13Thresholds serverThresholds,
            bool hasServerThresholds)
        {
            Verdict = verdict;
            ServerVersion = serverVersion;
            ServerTimestamp = serverTimestamp;
            ServerThresholds = serverThresholds;
            HasServerThresholds = hasServerThresholds;
        }

        public ServerModerationVerdict(ContentModerationResult verdict, string serverVersion, string serverTimestamp)
            : this(verdict, serverVersion, serverTimestamp, default, false)
        {
        }
    }
}