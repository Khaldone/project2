// Assets/_Project/1_CoreDomain/MatchLogic/MatchCoordinator.Timer.cs
using System;


public partial class MatchCoordinator
{
    private const float TURN_TIME_SECONDS = 30.0f;
    private const float PANIC_TIME_SECONDS = 7.0f;

    private float _currentTurnTime;
    private bool _isTimerActive;
    private bool _hasPanicFired;


    public void StartTurnTimer()
    {
        _currentTurnTime = TURN_TIME_SECONDS;
        _isTimerActive = true;
        _hasPanicFired = false;
    }


    // Called by the Infrastructure layer (Photon Fusion) every tick
    public void TickTimer(float deltaTime)
    {
        if (!_isTimerActive || _state.IsWaitingForShotResult) return;


        _currentTurnTime -= deltaTime;


        bool isPanic = _currentTurnTime <= PANIC_TIME_SECONDS;


        // Broadcast the time to the UI and Audio systems
        _broker.Publish(new TurnTimerSyncMessage
        {
            TimeRemaining = Math.Max(0, _currentTurnTime),
            TimeTotal = TURN_TIME_SECONDS,
            IsPanicMode = isPanic
        });


        // Fire a one-off message exactly when panic starts to trigger the heartbeat SFX
        if (isPanic && !_hasPanicFired)
        {
            _hasPanicFired = true;
            //_broker.Publish(new PlayAudioMessage("SFX_Heartbeat_Panic"));
        }


        // Time expired! Force an automatic foul.
        if (_currentTurnTime <= 0)
        {
            _isTimerActive = false;
            ForceTimeExpiredFoul();
        }
    }


    private void ForceTimeExpiredFoul()
    {
        //_broker.Publish(new FoulCommittedMessage { Reason = "Time Expired." });

        // Pass the turn to the opponent with Ball-In-Hand
        //TransitionTurn(toPlayerId: GetOpponentId(), withBallInHand: true);
    }
}
