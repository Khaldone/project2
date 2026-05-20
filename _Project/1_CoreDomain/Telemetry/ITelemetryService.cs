// Path: Assets/_Project/1_CoreDomain/Telemetry/ITelemetryService.cs
using System;
using System.Collections.Generic;

namespace Billiards.CoreDomain.Telemetry
{
    public interface ITelemetryService
    {
        /// <summary>
        /// Explicit hook for post-injection validation or warm startup tracing.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Safely sets an anonymized user identifier context to tie runtime exceptions back to a specific session state.
        /// </summary>
        void SetUserContext(string anonymizedPlayerId);

        /// <summary>
        /// Appends standard high-level searchable operational tags (e.g. match_type, scene_name, connection_interface).
        /// </summary>
        void SetTag(string key, string value);

        /// <summary>
        /// Removes an existing high-level search tag when contextual operations conclude.
        /// </summary>
        void RemoveTag(string key);

        /// <summary>
        /// Records chronological operational events (match events, button taps, physics ticks) to reveal crash preconditions.
        /// </summary>
        void AddBreadcrumb(string message, string category = null, string type = null, Dictionary<string, string> data = null);

        /// <summary>
        /// Manually captures explicit hand-caught or predicted runtime exceptions with specialized operational scopes.
        /// </summary>
        void CaptureException(Exception exception, Dictionary<string, string> extraContext = null);

        /// <summary>
        /// Reports structural non-fatal errors or validation failures directly up to the diagnostic backend dashboard.
        /// </summary>
        void CaptureMessage(string message, TelemetrySeverity severity = TelemetrySeverity.Info, Dictionary<string, string> extraContext = null);
    }

    public enum TelemetrySeverity
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }
}