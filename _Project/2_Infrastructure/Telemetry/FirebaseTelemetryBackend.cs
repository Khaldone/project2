using Billiards.Core.Analytics;
using Cysharp.Threading.Tasks;
using Firebase.Analytics;
using System.Collections.Generic;
using UnityEngine;

namespace Billiards.Infrastructure.Telemetry
{
    public sealed class FirebaseTelemetryBackend : ITelemetryBackend
    {
        private bool _isInitialized;

        public async UniTask InitializeAsync()
        {
            // Await the dependency check so the GMS client lock is fully released
            // before any other Google Play Services call (e.g. GPGS silent auth).
            var dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                _isInitialized = true;
                Debug.Log("[Telemetry] Firebase Analytics ready. GMS lock released.");
            }
            else
            {
                Debug.LogError($"[Telemetry] Could not resolve Firebase dependencies: {dependencyStatus}");
            }
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            if (!_isInitialized) return;

            if (parameters == null || parameters.Count == 0)
            {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            var firebaseParameters = new Parameter[parameters.Count];
            int index = 0;

            foreach (var kvp in parameters)
            {
                if (kvp.Value is long longVal)
                    firebaseParameters[index] = new Parameter(kvp.Key, longVal);
                else if (kvp.Value is int intVal)
                    firebaseParameters[index] = new Parameter(kvp.Key, intVal);
                else if (kvp.Value is double doubleVal)
                    firebaseParameters[index] = new Parameter(kvp.Key, doubleVal);
                else if (kvp.Value is float floatVal)
                    firebaseParameters[index] = new Parameter(kvp.Key, floatVal);
                else
                    firebaseParameters[index] = new Parameter(kvp.Key, kvp.Value?.ToString() ?? string.Empty);

                index++;
            }

            FirebaseAnalytics.LogEvent(eventName, firebaseParameters);
        }
    }
}