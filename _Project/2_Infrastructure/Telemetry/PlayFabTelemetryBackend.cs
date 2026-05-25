// Assets/_Project/Infrastructure/Telemetry/PlayFabTelemetryBackend.cs
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Billiards.Infrastructure.Telemetry
{
    public sealed class PlayFabTelemetryBackend : Billiards.Core.Analytics.ITelemetryBackend
    {
        public UniTask InitializeAsync()
        {
            // PlayFab identity relies on your existing identity provider model initialization loop
            return UniTask.CompletedTask;
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            if (!PlayFabClientAPI.IsClientLoggedIn()) return;

            PlayFabClientAPI.WritePlayerEvent(new WriteClientPlayerEventRequest
            {
                EventName = eventName,
                Body = parameters
            },
            result => Debug.Log($"[Telemetry] PlayFab successfully captured event: {eventName}"),
            error => Debug.LogWarning($"[Telemetry] PlayFab dropped event: {error.GenerateErrorReport()}"));
        }
    }
}