// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Progression/TrophyMatchListener.cs
using VContainer.Unity;
using System;
using UnityEngine;
using Billiards.Core.Analytics;

public class TrophyMatchListener : IStartable, IDisposable
{
    private readonly IMessageBroker _broker;
    // We inject a secure backend service (PlayFab/CBS wrapper)
    //private readonly IBackendTrophyService _backendTrophyService;


    //public TrophyMatchListener(IMessageBroker broker, IBackendTrophyService backendService)
    //{
    //    _broker = broker;
    //    _backendTrophyService = backendService;
    //}

    public TrophyMatchListener(IMessageBroker broker)
    {
        _broker = broker;

    }


    public void Start()
    {
        _broker.Subscribe<MatchConcludedMessage>(HandleMatchEnded);
    }


    private void HandleMatchEnded(MatchConcludedMessage msg)
    {
        if (msg.WasBotOpponent && !msg.IsRanked) return; // No trophies for offline practice


        // We DO NOT calculate the final math here.
        // We tell the server what happened, and the SERVER calculates the math.
        if (msg.DidLocalPlayerWin)
        {
            //_backendTrophyService.ReportRankedWinAsync(msg.ArenaId); // e.g., "tokyo_tier"
        }
        else
        {
            //_backendTrophyService.ReportRankedLossAsync(msg.ArenaId);
        }
    }


    public void Dispose()
    {
        _broker.Unsubscribe<MatchConcludedMessage>(HandleMatchEnded);
    }
}
