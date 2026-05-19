// Assets/_Project/1_CoreDomain/Analytics/AnalyticsOrchestrator.cs
using System;
using System.Collections.Generic;
using VContainer.Unity;


public class AnalyticsOrchestrator : IStartable, IDisposable
{
    private readonly IAnalyticsService _analytics;
    private readonly IMessageBroker_New _broker;


    // Memory tokens
    private IDisposable _matchFinishedSub;
    private IDisposable _currencyUpdatedSub;
    private IDisposable _foulCommittedSub;


    public AnalyticsOrchestrator(IAnalyticsService analytics, IMessageBroker_New broker)
    {
        _analytics = analytics;
        _broker = broker;
    }


    public void Start()
    {
        // The Orchestrator wiretaps the Message Broker, listening to the game
        // without the game knowing it's being listened to.
        //_matchFinishedSub = _broker.Subscribe<MatchFinishedMessage>(OnMatchFinished);
        //_currencyUpdatedSub = _broker.Subscribe<CurrencyUpdatedMessage>(OnCurrencyUpdated);
        _foulCommittedSub = _broker.Subscribe<FoulCommittedMessage>(OnFoulCommitted);
    }


    //private void OnMatchFinished(MatchFinishedMessage msg)
    //{
    //    var props = new Dictionary<string, object>
    //    {
    //        { "Opponent_Type", msg.WasBotMatch ? "Bot" : "Human" },
    //        { "Result", msg.DidLocalPlayerWin ? "Win" : "Loss" },
    //        { "Duration_Seconds", msg.MatchDurationSeconds },
    //        { "Tier", msg.ArenaTierName }
    //    };


    //    _analytics.TrackEvent("Match_Completed", props);
    //    _analytics.SetUserProperty("Lifetime_Matches", msg.NewTotalLifetimeMatches);
    //}


    //private void OnCurrencyUpdated(CurrencyUpdatedMessage msg)
    //{
    //    var props = new Dictionary<string, object>
    //    {
    //        { "Currency_Type", msg.CurrencyId },
    //        { "Amount_Changed", msg.DeltaAmount },
    //        { "Source", msg.TransactionSource } // e.g., "IAP_Shop" or "Daily_Reward"
    //    };


    //    _analytics.TrackEvent("Economy_Transaction", props);
    //}


    private void OnFoulCommitted(FoulCommittedMessage msg)
    {
        // We can track specific gameplay friction points to see where players struggle
        var props = new Dictionary<string, object>
        {
            { "Foul_Reason", msg.FoulReason } // e.g., "Sunk_Cue_Ball"
        };
        _analytics.TrackEvent("Gameplay_Friction_Foul", props);
    }


    public void Dispose()
    {
        _matchFinishedSub?.Dispose();
        _currencyUpdatedSub?.Dispose();
        _foulCommittedSub?.Dispose();
    }
}