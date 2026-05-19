// Assets/Scripts/CoreDomain/Match/MatchCoordinator.cs
using System;
using System.Collections.Generic;

public partial class MatchCoordinator
{
    private readonly IMatchBroadcaster _broadcaster;
    private readonly RulesEngine _rulesEngine;
    private readonly TurnContext _currentTurnContext;
    private readonly IMessageBroker_New _broker;
    private readonly IGameRuleset _currentRules;
    public int CurrentPlayerId { get; private set; } = 1;
    public event Action<int> OnPlayerTurnChanged;

    // VContainer injects the correct strategy here based on what the player selected in the main menu
    public MatchCoordinator(IMatchBroadcaster broadcaster, RulesEngine rulesEngine, TurnContext context, IMessageBroker_New messageBroker, IGameRuleset rules)
    {
        _broadcaster = broadcaster;
        _rulesEngine = rulesEngine;
        _currentTurnContext = context;
        _broker = messageBroker;
        _currentRules = rules;
    }


    // Called when the TableStateManager confirms all balls have stopped
    public void ResolveTurnEnd()
    {
        RulesEngine.TurnResult result = _rulesEngine.EvaluateTurn(_currentTurnContext);


        if (result == RulesEngine.TurnResult.Foul)
        {
            _broadcaster.BroadcastMatchEvent(MatchEventType.Scratch, CurrentPlayerId);
            PassTurn();
        }
        else if (result == RulesEngine.TurnResult.EndTurn)
        {
            PassTurn();
        }
        else if (result == RulesEngine.TurnResult.ContinueTurn)
        {
            _broadcaster.BroadcastMatchEvent(MatchEventType.PocketedValidBall, CurrentPlayerId);
            // Player keeps their turn, broadcast the same ID again to refresh timers
            _broadcaster.BroadcastActivePlayer(CurrentPlayerId);
        }


        _currentTurnContext.Reset(); // Clear the ledger for the next strike

    //    RulesEngine.TurnResult result = _rulesEngine.EvaluateTurn(_currentTurnContext);


    //    // Leave a breadcrumb so if the game crashes in the next frame,
    //    // Sentry tells us EXACTLY how the turn just ended.
    //    _telemetryService.LeaveBreadcrumb("Gameplay", "Turn Evaluated", new Dictionary<string, string>
    //{
    //    { "Result", result.ToString() },
    //    { "ActivePlayer", CurrentPlayerId.ToString() }
    //});


        // ... rest of the turn resolution logic ...

    }

    //public void ProcessPocketedBall(BallState ball)
    //{
    //    // ... complex logic to check if it was the 8-ball or a foul ...


    //    // The logic is done. Now, we just shout into the void.
    //    _broker.Publish(new BallPocketedMessage
    //    {
    //        BallId = ball.BallId,
    //        WasFoul = false
    //    });
    //}


    private void PassTurn()
    {
        CurrentPlayerId = CurrentPlayerId == 1 ? 2 : 1; // Toggle between player 1 and 2
        _broadcaster.BroadcastActivePlayer(CurrentPlayerId);
    }

    //public void OnShotCompleted(TableState newState, StrikeIntent intent)
    //{
    //    // The Coordinator doesn't know if it's 8-ball or 9-ball. It just asks the strategy.
    //    ShotResult result = _currentRules.EvaluateShot(newState, intent);


    //    if (result.IsFoul)
    //    {
    //        TriggerFoul(result.FoulReason);
    //    }


    //    MatchConclusion conclusion = _currentRules.CheckWinCondition(newState, "Player1");
    //    if (conclusion.IsGameOver)
    //    {
    //        EndMatch(conclusion.WinnerId);
    //    }
    //}


    private void EndGame(string winnerId)
    {
        // ... cleanup table logic ...


        // Publish the event globally!
        _broker.Publish(new MatchEndedMessage
        {
            WinnerId = winnerId,
            TotalTurns = 14,
            WasDisconnect = false
        });
    }

    private void ConcludeMatch(string winnerId, string reason)
    {
        //bool didIWin = winnerId == _localPlayerId;

        _broker.Publish(new MatchConcludedMessage
        {
            //WasBotOpponent = _matchmakingOrchestrator.IsPlayingBot, // The boolean we made earlier!
            //DidLocalPlayerWin = didIWin,
            //TotalTurnsTaken = _turnStateMachine.CurrentTurnCount,
            EndReason = reason
        });
    }




}