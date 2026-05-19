// Assets/_Project/1_CoreDomain/MatchLogic/MatchDefinitions.cs
using System.Collections.Generic;


public enum BallSuit { None, Solid, Stripe, EightBall, CueBall }


public enum TurnOutcome
{
    ContinueTurn,
    Turnover,
    TurnoverWithBallInHand,
    WinGame,
    LoseGame
}


// The snapshot of the table BEFORE the shot is taken
public class MatchState
{
    public bool IsOpenTable = true;
    public bool IsBreakShot = true;
    public bool IsWaitingForShotResult;
    public bool IsAwaitingPocketCall;
    public bool Requires8BallCall;
    public bool IsBallInHand;
    public bool WasBreakFoul;

    public string Player1Id;
    public string Player2Id;
    public string ActivePlayerId;

    public int CalledPocketId;


    public Dictionary<string, BallSuit> PlayerSuits = new Dictionary<string, BallSuit>();
    public Dictionary<string, int> PlayerBallsRemaining = new Dictionary<string, int>();


    public BallSuit GetActivePlayerSuit() => PlayerSuits.ContainsKey(ActivePlayerId) ? PlayerSuits[ActivePlayerId] : BallSuit.None;
}


// The output from the Physics Engine AFTER the balls stop moving
public struct ShotResult_New
{
    public BallSuit FirstBallHit;
    public List<BallSuit> PocketedBalls;
    public bool DidAnyBallHitRailAfterContact;
    public bool IsLegalBreak; // Physics engine checks if 4 balls hit rails on break
}


// The final ruling from the Coordinator
public struct RuleEvaluation
{
    public TurnOutcome Outcome;
    public string FoulMessage; // Null if no foul
}
