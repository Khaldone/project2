// Path: Assets/_Project/3_Presentation/Telemetry/TelemetryDiagnosticTester.cs
using Billiards.CoreDomain.Telemetry;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using VContainer;

namespace Billiards.Presentation.Telemetry
{
    [AddComponentMenu("Billiards/Telemetry/Telemetry Diagnostic Tester")]
    public sealed class TelemetryDiagnosticTester : MonoBehaviour
    {
        private ITelemetryService _telemetryService;

        [Inject]
        public void Construct(ITelemetryService telemetryService)
        {
            _telemetryService = telemetryService;
        }

        private void Update()
        {
            // --- PC/Editor Keyboard Shortcuts for Rapid Validation ---

            // Press Alpha 1 to test a standard caught exception with extra metadata context
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TriggerCaughtExceptionWithContext();
            }

            // Press Alpha 2 to throw an unhandled C# exception (Tests managed logging pipelines)
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TriggerUnhandledManagedException();
            }

            // Press Alpha 3 to trigger a native mobile crash sequence (Tests NDK/iOS Minidumps)
            if (Input.GetKeyDown(KeyCode.M))
            {
                TriggerHardNativeCrash();
            }
        }

        /// <summary>
        /// Validates that VContainer method injection completed successfully.
        /// Returns false and logs a diagnostic warning if the service was never injected.
        /// </summary>
        private bool EnsureInjected()
        {
            if (_telemetryService != null) return true;

            Debug.LogWarning(
                "[TelemetryDiagnosticTester] ITelemetryService is null — VContainer [Inject] Construct() was never called. " +
                "Ensure this MonoBehaviour is registered in a LifetimeScope (e.g. via builder.RegisterComponent or InjectGameObject).");
            return false;
        }

        /// <summary>
        /// Simulates a hand-caught failure inside an infrastructure worker or API loop.
        /// </summary>
        public void TriggerCaughtExceptionWithContext()
        {
            if (!EnsureInjected()) return;

            Debug.Log("[TelemetryTest] Injecting hand-caught diagnostic exception into pipeline...");

            try
            {
                // Artificially provoke a standard formatting anomaly
                string unallocatedString = null;
                int characterCount = unallocatedString.Length;
            }
            catch (Exception ex)
            {
                var diagnosticMetadata = new Dictionary<string, string>
                {
                    { "diagnostic_trigger_source", "Manual_User_Verification_Node" },
                    { "active_cue_id", "cue_test_999" }
                };

                _telemetryService.CaptureException(ex, diagnosticMetadata);
            }
        }

        /// <summary>
        /// Simulates an unhandled engine crash on the main thread execution loop.
        /// </summary>
        public void TriggerUnhandledManagedException()
        {
            if (!EnsureInjected()) return;

            Debug.Log("[TelemetryTest] Raising unhandled C# thread exception...");
            _telemetryService.AddBreadcrumb("Provoking main execution loop exception step.", "diagnostic", "panic");

            throw new InvalidOperationException("Billiards Architecture Verification Anomaly: Simulated unhandled presentation thread panic.");
        }

        /// <summary>
        /// Provokes a native engine violation crash to ensure background signal monitors function.
        /// NOTE: This will instantly close the running game build process.
        /// </summary>
        public void TriggerHardNativeCrash()
        {
            if (!EnsureInjected()) return;

            Debug.Log("[TelemetryTest] Triggering system signal termination via Unity native crash API...");
            _telemetryService.AddBreadcrumb("Invoking native mobile hardware sub-system panic sequence.", "diagnostic", "hard_crash");

            // Uses Unity's built-in native crash utility to force an AccessViolation signal abort.
            // Sentry's native integration (NDK/iOS) will capture the resulting minidump automatically.
            Utils.ForceCrash(ForcedCrashCategory.AccessViolation);
        }
    }
}
