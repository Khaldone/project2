// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Analytics/GlobalAnalyticsTracker.cs
using Billiards.Core.Analytics;
using Billiards.CoreDomain.Services;
using Billiards.Infrastructure.Telemetry.Serialization;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Threading;
using VContainer.Unity;
public class GlobalAnalyticsTracker : IAsyncStartable, IDisposable
{
    private readonly IMessageBroker _messageBroker;
    private readonly IReadOnlyList<ITelemetryBackend> _backends;
    private readonly IPlatformServicesGate _platformGate;

    public GlobalAnalyticsTracker(
        IMessageBroker messageBroker,
        IReadOnlyList<ITelemetryBackend> backends,
        IPlatformServicesGate platformGate)
    {
        _messageBroker = messageBroker;
        _backends = backends;
        _platformGate = platformGate;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        // Await ALL backends in parallel. Firebase's CheckAndFixDependenciesAsync
        // holds the GMS client lock — this must complete before any GPGS auth call.
        var initTasks = new UniTask[_backends.Count];
        for (int i = 0; i < _backends.Count; i++)
            initTasks[i] = _backends[i].InitializeAsync();
        await UniTask.WhenAll(initTasks);

        // Signal that Firebase (and all platform services) are done.
        // LoginEntryPoint awaits this gate before attempting GPGS silent auth.
        _platformGate.MarkReady();

        // Force an initialization event. Since collection starts disabled in the manifest
        // to prevent GMS locking, the automatic session_start event is skipped. Sending a
        // manual event here triggers the session in the Firebase dashboard.
        DispatchEvent("app_session_initialized", new Dictionary<string, object>());

        // Hook tracking subscriptions passively to execution mail system
        _messageBroker.Subscribe<MatchTelemetryMessage>(OnMatchTracked);
        _messageBroker.Subscribe<EconomyTelemetryMessage>(OnEconomyTracked);
        _messageBroker.Subscribe<InputTactileTelemetryMessage>(OnInputTactileTracked);

        // Subscribe to the global nervous system
        _messageBroker.Subscribe<EconomyTransactionMessage>(LogEconomyEvent);
        _messageBroker.Subscribe<MatchConcludedMessage>(LogMatchResultEvent);
        _messageBroker.Subscribe<MatchmakingFunnelMessage>(LogFunnelEvent);
        _messageBroker.Subscribe<TutorialStepMessage>(LogTutorialEvent);
        // Firebase Crashlytics initializes automatically on boot,
        // no manual subscription needed for standard crashes!
    }

    private void OnMatchTracked(MatchTelemetryMessage msg)
    {
        // Build raw data dictionary frame utilizing pooled system context definitions
        Dictionary<string, object> pooledPayload = TelemetryEventFactory.BuildMatchPayload(msg);

        try
        {
            // Execute dispatch over our operational ACL interfaces safely
            foreach (var backend in _backends)
            {
                backend.LogEvent("match_completed", pooledPayload);
            }
        }
        finally
        {
            // Guarantee recycling execution block frame runs to prevent mobile allocations leak
            TelemetryEventPool.Release(pooledPayload);
        }
    }

    private void OnEconomyTracked(EconomyTelemetryMessage msg)
    {
        // Build raw data dictionary frame utilizing pooled system context definitions
        Dictionary<string, object> pooledPayload = TelemetryEventFactory.BuildEconomyPayload(msg);

        try
        {
            // Execute dispatch over our operational ACL interfaces safely
            foreach (var backend in _backends)
            {
                backend.LogEvent("virtual_economy_action", pooledPayload);
            }
        }
        finally
        {
            // Guarantee recycling execution block frame runs to prevent mobile allocations leak
            TelemetryEventPool.Release(pooledPayload);
        }
    }

    private void OnInputTactileTracked(InputTactileTelemetryMessage msg)
    {
        var data = new Dictionary<string, object>
            {
                { "state_name", msg.CurrentStateName },
                { "intensity", msg.InputIntensity },
                { "cue_id", msg.EquippedCueId }
            };

        // Dispatched primarily for Crashlytics tracking/context matching during mechanical shifts
        DispatchEvent("tactile_input_state", data);
    }

    private void DispatchEvent(string eventName, Dictionary<string, object> parameters)
    {
        foreach (var backend in _backends)
        {
            backend.LogEvent(eventName, parameters);
        }
    }

    private void LogEconomyEvent(EconomyTransactionMessage msg)
    {
        SendPlayFabEvent("economy_event", new Dictionary<string, object>
        {
            { "currency", msg.Currency },
            { "amount_change", msg.AmountChange },
            { "source_sink", msg.SourceSinkId }
        });



        // Optional: Also send to Firebase if it's a critical monetization event
        // FirebaseAnalytics.LogEvent("spend_virtual_currency", ...);
    }

    private void LogMatchResultEvent(MatchConcludedMessage msg)
    {
        var request = new WriteClientPlayerEventRequest
        {
            EventName = "match_concluded",
            Body = new Dictionary<string, object>
            {
                { "was_bot", msg.WasBotOpponent },
                { "turns", msg.TotalTurnsTaken },
                { "reason", msg.EndReason }
            }
        };
        PlayFabClientAPI.WritePlayerEvent(request, null, null);
    }

    private void LogFunnelEvent(MatchmakingFunnelMessage msg)
    {
        SendPlayFabEvent("matchmaking_funnel", new Dictionary<string, object>
        {
            { "step", msg.FunnelStep },
            { "wait_time", msg.WaitTimeSeconds }
        });
    }

    private void LogTutorialEvent(TutorialStepMessage msg)
    {
        SendPlayFabEvent("tutorial_progress", new Dictionary<string, object>
        {
            { "step_index", msg.StepIndex },
            { "step_name", msg.StepName },
            { "time_spent_seconds", msg.TimeSpentSeconds } // Added to the backend payload
        });
    }

    // A reusable helper method to format and send the data to PlayFab
    private void SendPlayFabEvent(string eventName, Dictionary<string, object> payload)
    {
        // Safety check: Don't send analytics if the player isn't logged in
        if (!PlayFabClientAPI.IsClientLoggedIn()) return;


        var request = new WriteClientPlayerEventRequest
        {
            EventName = eventName,
            Body = payload
        };


        PlayFabClientAPI.WritePlayerEvent(request,
            result => { /* Silently succeed */ },
            error => UnityEngine.Debug.LogWarning($"Analytics Failed: {error.ErrorMessage}")
        );
    }
    public void Dispose()
    {
        _messageBroker.Unsubscribe<EconomyTransactionMessage>(LogEconomyEvent);
        _messageBroker.Unsubscribe<MatchConcludedMessage>(LogMatchResultEvent);
        _messageBroker.Unsubscribe<MatchmakingFunnelMessage>(LogFunnelEvent);
        _messageBroker.Unsubscribe<TutorialStepMessage>(LogTutorialEvent);
        _messageBroker.Unsubscribe<MatchTelemetryMessage>(OnMatchTracked);
        _messageBroker.Unsubscribe<EconomyTelemetryMessage>(OnEconomyTracked);
        _messageBroker.Unsubscribe<InputTactileTelemetryMessage>(OnInputTactileTracked);
    }





}