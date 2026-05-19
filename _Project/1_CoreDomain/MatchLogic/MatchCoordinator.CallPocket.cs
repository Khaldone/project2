// Assets/_Project/1_CoreDomain/MatchLogic/MatchCoordinator.CallPocket.cs
using System.Linq;


public partial class MatchCoordinator
{
    // The UI sends this command when the player clicks a pocket highlight
    public void SetCalledPocket(int pocketId)
    {
        if (_state.IsAwaitingPocketCall)
        {
            _state.CalledPocketId = pocketId;
            _state.IsAwaitingPocketCall = false;

            // Resume the shot timer now that they've chosen
            _isTimerActive = true;
        }
    }

    // Called by the Network Runner when an RPC packet arrives
    public void TrySetCalledPocket(string senderPlayerId, int pocketId)
    {
        // 1. RULE CHECK: Are we even waiting for a pocket call?
        if (!_state.IsAwaitingPocketCall) return;


        // 2. AUTHORITY CHECK: Did the opponent try to pick the pocket for the active player?
        if (senderPlayerId != _state.ActivePlayerId)
        {
            // Log this! If it happens, they are either severely desynced or using a hacked client.
            UnityEngine.Debug.LogWarning($"Unauthorized pocket call attempt by {senderPlayerId}");
            return;
        }


        // 3. VALIDATION PASSED: Lock it in.
        _state.CalledPocketId = pocketId;
        _state.IsAwaitingPocketCall = false;

        // Resume the shot timer and tell the UI to highlight the chosen pocket
        _isTimerActive = true;
        //_broker.Publish(new PocketSuccessfullyCalledMessage(pocketId));
    }

    // Inside EvaluateShot(ShotResult shot)...
    private RuleEvaluation Evaluate8BallPocket(ShotResult shot)
    {
        RuleEvaluation ruling = new RuleEvaluation();

        // Find the event where the 8-ball was pocketed
        //var eightBallEvent = shot.PocketEvents.FirstOrDefault(p => p.Suit == BallSuit.EightBall);

        //if (eightBallEvent.Suit != BallSuit.None) // It was pocketed
        //{
        //    // Did they call a pocket?
        //    if (_state.Requires8BallCall && _state.CalledPocketId.HasValue)
        //    {
        //        if (eightBallEvent.PocketId != _state.CalledPocketId.Value)
        //        {
        //            // WRONG POCKET! Instant Loss.
        //            ruling.Outcome = TurnOutcome.LoseGame;
        //            ruling.FoulMessage = "8-Ball sunk in the wrong pocket! You lose.";
        //            return ruling;
        //        }
        //    }

        //    // If they sunk it legally in the called pocket (and cleared their own balls)
        //    ruling.Outcome = TurnOutcome.WinGame;
        //    return ruling;
        //}


        return ruling; // Return neutral if 8-ball wasn't sunk
    }
}