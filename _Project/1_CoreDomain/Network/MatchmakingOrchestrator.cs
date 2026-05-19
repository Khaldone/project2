// Assets/_Project/CoreDomain/Networking/MatchmakingOrchestrator.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class MatchmakingOrchestrator
{
    private readonly IMatchmakingService _networkService; // Your Photon wrapper
    private readonly IBotProfileProvider _botProvider;
    private readonly IPlayerDataService _playerData; // To get the human's level
    private readonly IMessageBroker _broker; //Used for analytics.

    // We use a global Session state to store who we are playing against
    public PlayerProfile CurrentOpponent { get; private set; }
    public bool IsPlayingBot { get; private set; }


    public MatchmakingOrchestrator(
        IMatchmakingService networkService,
        IBotProfileProvider botProvider,
        IPlayerDataService playerData)
    {
        _networkService = networkService;
        _botProvider = botProvider;
        _playerData = playerData;
    }


    public async Task StartMatchmakingAsync()
    {
        // 1. Create a cancellation token that trips after exactly 7 seconds
        using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(7));


        try
        {
            // 2. Attempt to find a real player on PlayFab/Photon
            // We pass the token so the network knows to abort if time runs out
            //bool foundRealMatch = await _networkService.JoinRandomMatchAsync(timeoutSource.Token);

            bool foundRealMatch = true;
            if (foundRealMatch)
            {
                IsPlayingBot = false;
                // NetworkService would populate CurrentOpponent from the real Photon lobby
                return;
            }
        }
        catch (OperationCanceledException)
        {
            // 3. THE 7 SECONDS EXPIRED! The deception begins.
            Debug.Log("AAA Pipeline: Matchmaking timeout. Injecting Bot Opponent.");

            IsPlayingBot = true;
            //CurrentOpponent = _botProvider.GenerateFakeOpponent(_playerData.Level);


            // Tell Photon to start offline so the Arena's network scripts don't crash
            //await _networkService.StartOfflineBotMatchAsync();
        }

        float startTime = Time.time; // Or use System.Diagnostics.Stopwatch

        _broker.Publish(new MatchmakingFunnelMessage
        {
            FunnelStep = "Search_Started",
            WaitTimeSeconds = 0
        });


        // ... await Photon joining ...


        _broker.Publish(new MatchmakingFunnelMessage
        {
            FunnelStep = "Match_Found",
            WaitTimeSeconds = Mathf.RoundToInt(Time.time - startTime)
        });

    }

    public async Task StartTournamentMatchmakingAsync(TournamentState tourneyState)
    {
        // We create a filter that Photon uses to find a room
        var roomProperties = new Dictionary<string, object>
    {
        { "TournamentId", tourneyState.TournamentId },
        { "Round", tourneyState.CurrentRound }
    };


        // 1. Start the 7-second timeout cancellation token (from our earlier architecture)
        using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(7));


        try
        {
            // 2. Tell Photon to join a random room that matches exactly these properties
            //bool foundMatch = await _networkService.JoinRandomMatchWithFilterAsync(roomProperties, timeoutSource.Token);


            //if (foundMatch)
            //{
            //    IsPlayingBot = false;
            //    return;
            //}
        }
        catch (OperationCanceledException)
        {
            // THE GENIUS OF THE DECEPTION PIPELINE:
            // If nobody in the world is searching for a Round 3 Sydney Cup match right now,
            // the 7-second timer expires. We instantly generate a Bot Profile, give it a
            // high difficulty level, and seamlessly drop the player into an offline Finals match!

            IsPlayingBot = true;
            //CurrentOpponent = _botProvider.GenerateFakeOpponent(playerLevel: 50); // Harder bot for tournaments

            //await _networkService.StartOfflineBotMatchAsync();
        }
    }


}
