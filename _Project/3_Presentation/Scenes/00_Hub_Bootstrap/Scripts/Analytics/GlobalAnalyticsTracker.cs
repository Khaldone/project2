// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Analytics/GlobalAnalyticsTracker.cs
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using VContainer.Unity;
using System;
public class GlobalAnalyticsTracker : IStartable, IDisposable
{
    private readonly IMessageBroker _broker;


    public GlobalAnalyticsTracker(IMessageBroker broker)
    {
        _broker = broker;
    }

    public void Start()
    {
        // Subscribe to the global nervous system
        _broker.Subscribe<EconomyTransactionMessage>(LogEconomyEvent);
        _broker.Subscribe<MatchConcludedMessage>(LogMatchResultEvent);
        _broker.Subscribe<MatchmakingFunnelMessage>(LogFunnelEvent);
        _broker.Subscribe<TutorialStepMessage>(LogTutorialEvent);
        // Firebase Crashlytics initializes automatically on boot,
        // no manual subscription needed for standard crashes!
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
        _broker.Unsubscribe<EconomyTransactionMessage>(LogEconomyEvent);
        _broker.Unsubscribe<MatchConcludedMessage>(LogMatchResultEvent);
        _broker.Unsubscribe<MatchmakingFunnelMessage>(LogFunnelEvent);
        _broker.Unsubscribe<TutorialStepMessage>(LogTutorialEvent);
    }





}