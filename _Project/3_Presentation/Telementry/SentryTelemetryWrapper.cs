// Path: Assets/_Project/3_Presentation/Telementry/SentryTelemetryWrapper.cs
using Billiards.CoreDomain.Telemetry;
using Sentry;
using Sentry.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Billiards.Presentation.Telemetry
{
    public sealed class SentryTelemetryWrapper : ITelemetryService
    {
        public void Initialize()
        {
            // Evaluate target operating environment variables dynamically at startup
            string runningEnvironment = DetermineEnvironment();
            string bundleVersion = $"{Application.identifier}@{Application.version}";

            SentrySdk.ConfigureScope(scope =>
            {
                // Assign global platform metadata to the persistent thread context
                scope.Environment = runningEnvironment;

                // Explicitly tags the release variant for server stack-trace map matching
                scope.SetTag("build_type", Debug.isDebugBuild ? "debug_build" : "release_build");
                scope.SetTag("unity_version", Application.unityVersion);
                scope.SetTag("platform", Application.platform.ToString());
            });

            // Log operational breadcrumb tracing to verify configuration handshake
            var contextDetails = new Dictionary<string, string>
            {
                { "environment", runningEnvironment },
                { "release_version", bundleVersion }
            };

            AddBreadcrumb("Programmatic release tracking and environment tags bound to global Sentry scope.", "system", "lifecycle", contextDetails);
        }

        /// <summary>
        /// Contextually derives the exact target execution tier without exposing hardcoded flags.
        /// </summary>
        private string DetermineEnvironment()
        {
#if UNITY_EDITOR
            return "development_editor";
#elif DEVELOPMENT_BUILD
            return "staging_qa";
#else
            // Strict enforcement for raw production builds
            return "production";
#endif
        }

        public void SetUserContext(string anonymizedPlayerId)
        {
            if (string.IsNullOrEmpty(anonymizedPlayerId)) return;

            SentrySdk.ConfigureScope(scope =>
            {
                scope.User = new SentryUser { Id = anonymizedPlayerId };
            });
        }

        public void SetTag(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) return;
            SentrySdk.ConfigureScope(scope => scope.SetTag(key, value));
        }

        public void RemoveTag(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            SentrySdk.ConfigureScope(scope => scope.UnsetTag(key));
        }

        public void AddBreadcrumb(string message, string category = null, string type = null, Dictionary<string, string> data = null)
        {
            if (string.IsNullOrEmpty(message)) return;

            var breadcrumbData = data != null ? new Dictionary<string, string>(data) : null;

            SentrySdk.AddBreadcrumb(
                message: message,
                category: category ?? "gameplay",
                type: type ?? "default",
                data: breadcrumbData,
                level: BreadcrumbLevel.Info
            );
        }

        public void CaptureException(Exception exception, Dictionary<string, string> extraContext = null)
        {
            if (exception == null) return;

            SentrySdk.CaptureException(exception, scope =>
            {
                if (extraContext != null)
                {
                    foreach (var kvp in extraContext)
                    {
                        scope.SetExtra(kvp.Key, kvp.Value);
                    }
                }
            });
        }

        public void CaptureMessage(string message, TelemetrySeverity severity = TelemetrySeverity.Info, Dictionary<string, string> extraContext = null)
        {
            if (string.IsNullOrEmpty(message)) return;

            SentrySdk.CaptureMessage(message, scope =>
            {
                scope.Level = MapSeverity(severity);
                if (extraContext != null)
                {
                    foreach (var kvp in extraContext)
                    {
                        scope.SetExtra(kvp.Key, kvp.Value);
                    }
                }
            });
        }

        private SentryLevel MapSeverity(TelemetrySeverity severity)
        {
            return severity switch
            {
                TelemetrySeverity.Debug => SentryLevel.Debug,
                TelemetrySeverity.Info => SentryLevel.Info,
                TelemetrySeverity.Warning => SentryLevel.Warning,
                TelemetrySeverity.Error => SentryLevel.Error,
                TelemetrySeverity.Fatal => SentryLevel.Fatal,
                _ => SentryLevel.Info
            };
        }
    }
}