// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/AI/BotPlayerController.cs
using System;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class BotPlayerController : NetworkBehaviour, IStartable, IDisposable
{
    private ITurnStateMachine _turnState;
    private IBotBrain _botBrain;
    private FusionPoolBall_Old _cueBall; // Reference to the authoritative ball

    private string _botPlayerId;

    [Inject]
    public void Construct(ITurnStateMachine turnState, IBotBrain botBrain, MatchmakingOrchestrator orchestrator)
    {
        _turnState = turnState;
        _botBrain = botBrain;

        // Grab the fake profile ID we generated during the 7-second deception
        _botPlayerId = orchestrator.CurrentOpponent.UserId;
    }

    public void Start()
    {
        _turnState.OnTurnStarted += HandleTurnStarted;
    }

    private async void HandleTurnStarted(string activePlayerId)
    {
        // 1. AUTHORITY CHECK: Only the server/host runs the bot logic.
        // If a human client somehow loads this script, it does nothing.
        if (!Object.HasStateAuthority) return;


        // 2. IDENTITY CHECK: Is it the bot's turn?
        if (activePlayerId == _botPlayerId)
        {
            await ExecuteBotTurnAsync();
        }
    }

    private async Task ExecuteBotTurnAsync()
    {
        // THE DECEPTION DELAY
        // Humans take time to aim. A bot calculates geometry in 1 millisecond.
        // We force the bot to wait 3 to 6 seconds so the human player
        // feels like they are watching a real person line up a shot.
        float thinkTime = UnityEngine.Random.Range(3.0f, 6.0f);
        await Task.Delay(TimeSpan.FromSeconds(thinkTime));


        // Note: In a real AAA game, you would fire an "AimingIntent" event during this
        // delay so the human player sees the bot's cue stick rotating on their screen!


        // 1. Gather the table state (Where are the balls?)
        // (Assuming you have a TableManager to get all active balls)
        var tableState = new System.Collections.Generic.List<BallState>();
        var cueState = _cueBall.GetCurrentState();


        // 2. Ask the pure C# brain for the math
        StrikeIntent optimalShot = _botBrain.CalculateBestShot(tableState, cueState);


        // 3. Pull the trigger via the Universal Funnel
        _cueBall.ApplyValidatedStrike(optimalShot);

        Debug.Log($"AAA Pipeline: Bot executed shot with Angle {optimalShot.Angle}");
    }

    public void Dispose()
    {
        if (_turnState != null) _turnState.OnTurnStarted -= HandleTurnStarted;
    }
}