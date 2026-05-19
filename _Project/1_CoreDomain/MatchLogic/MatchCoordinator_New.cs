// Assets/_Project/1_CoreDomain/MatchLogic/MatchCoordinator.cs
using System.Linq;


public partial class MatchCoordinator
{
    private MatchState _state;


    // Evaluates the physics result and determines the next state of the game
    public RuleEvaluation EvaluateShot(ShotResult_New shot)
    {
        RuleEvaluation ruling = new RuleEvaluation { Outcome = TurnOutcome.Turnover, FoulMessage = null };
        BallSuit activeSuit = _state.GetActivePlayerSuit();
        bool isFoul = false;


        // ==========================================
        // STEP 1: FOUL DETECTION (Evaluated First)
        // ==========================================

        // Foul A: Scratch (Cue ball in pocket)
        if (shot.PocketedBalls.Contains(BallSuit.CueBall))
        {
            isFoul = true;
            ruling.FoulMessage = "Scratch! Cue ball pocketed.";
        }
        // Foul B: Hit nothing
        else if (shot.FirstBallHit == BallSuit.None)
        {
            isFoul = true;
            ruling.FoulMessage = "Foul! Did not hit any balls.";
        }
        // Foul C: Wrong ball hit first (Only applies if table is NOT open)
        else if (!_state.IsOpenTable && shot.FirstBallHit != activeSuit && shot.FirstBallHit != BallSuit.EightBall)
        {
            isFoul = true;
            ruling.FoulMessage = $"Foul! You must hit a {activeSuit} first.";
        }
        // Foul D: Hitting the 8-Ball first (Illegal unless you are ON the 8-ball)
        else if (shot.FirstBallHit == BallSuit.EightBall && _state.PlayerBallsRemaining[_state.ActivePlayerId] > 0)
        {
            isFoul = true;
            ruling.FoulMessage = "Foul! Hit the 8-Ball early.";
        }
        // Foul E: No rail hit after contact (Prevents players from tapping balls to stall)
        else if (!shot.DidAnyBallHitRailAfterContact && shot.PocketedBalls.Count == 0)
        {
            isFoul = true;
            ruling.FoulMessage = "Foul! No rail contacted after initial hit.";
        }
        // Foul F: Illegal Break
        else if (_state.IsBreakShot && !shot.IsLegalBreak && shot.PocketedBalls.Count == 0)
        {
            isFoul = true;
            ruling.FoulMessage = "Illegal Break! Not enough balls hit the rail.";
        }


        // ==========================================
        // STEP 2: 8-BALL LOGIC (Game Over Conditions)
        // ==========================================

        if (shot.PocketedBalls.Contains(BallSuit.EightBall))
        {
            if (isFoul)
            {
                // If you scratch WHILE sinking the 8-ball, or sink it early, you instantly lose.
                ruling.Outcome = TurnOutcome.LoseGame;
                ruling.FoulMessage = "Foul on the 8-Ball! You lose.";
                return ruling;
            }

            if (_state.PlayerBallsRemaining[_state.ActivePlayerId] > 0)
            {
                // Sunk the 8-ball before clearing your own balls
                ruling.Outcome = TurnOutcome.LoseGame;
                ruling.FoulMessage = "Sunk the 8-Ball early! You lose.";
                return ruling;
            }


            // Legal 8-Ball pocketed!
            ruling.Outcome = TurnOutcome.WinGame;
            return ruling;
        }


        // If a foul occurred (and it wasn't a game-loss 8-ball foul), return Ball-In-Hand immediately.
        if (isFoul)
        {
            ruling.Outcome = TurnOutcome.TurnoverWithBallInHand;
            return ruling;
        }


        // ==========================================
        // STEP 3: OPEN TABLE & SUIT ASSIGNMENT
        // ==========================================

        bool pocketedLegalBall = false;


        // In standard rules, sinking a ball on the break does NOT assign suits. Table remains open.
        if (_state.IsOpenTable && !_state.IsBreakShot)
        {
            if (shot.PocketedBalls.Contains(BallSuit.Solid) && !shot.PocketedBalls.Contains(BallSuit.Stripe))
            {
                AssignSuits(BallSuit.Solid, BallSuit.Stripe);
                pocketedLegalBall = true;
            }
            else if (shot.PocketedBalls.Contains(BallSuit.Stripe) && !shot.PocketedBalls.Contains(BallSuit.Solid))
            {
                AssignSuits(BallSuit.Stripe, BallSuit.Solid);
                pocketedLegalBall = true;
            }
            // Note: If they sink BOTH a solid and stripe on an open table, it usually remains open
            // and they keep their turn, or they get to choose. We will keep it open for simplicity.
            else if (shot.PocketedBalls.Any(b => b == BallSuit.Solid || b == BallSuit.Stripe))
            {
                pocketedLegalBall = true;
            }
        }
        else if (!_state.IsOpenTable)
        {
            // Table is closed. Did they pocket their own suit?
            pocketedLegalBall = shot.PocketedBalls.Contains(activeSuit);
        }
        else if (_state.IsBreakShot && shot.PocketedBalls.Any(b => b == BallSuit.Solid || b == BallSuit.Stripe))
        {
            // They sunk a ball on the break. Table remains open, but they keep their turn.
            pocketedLegalBall = true;
        }


        // ==========================================
        // STEP 4: TURN CONTINUATION
        // ==========================================

        // Update the state so it is no longer the break shot
        _state.IsBreakShot = false;


        if (pocketedLegalBall)
        {
            ruling.Outcome = TurnOutcome.ContinueTurn;
        }
        else
        {
            ruling.Outcome = TurnOutcome.Turnover;
        }


        return ruling;
    }


    private void AssignSuits(BallSuit activePlayerSuit, BallSuit opponentSuit)
    {
        _state.IsOpenTable = false;
        _state.PlayerSuits[_state.ActivePlayerId] = activePlayerSuit;

        string opponentId = _state.ActivePlayerId == _state.Player1Id ? _state.Player2Id : _state.Player1Id;
        _state.PlayerSuits[opponentId] = opponentSuit;
    }
}
