// The types of balls on the table
using System.Collections.Generic;

public enum BallType_New { Cue, Solid, Stripe, EightBall }


// The data the physics engine passes to the coordinator after the balls stop moving
public struct ShotResult
{
    public BallType_New? FirstBallHit;
    public List<BallType_New> PocketedBalls;
    public bool DidCueBallHitRail;
}


// The messages the coordinator publishes to the UI and Network
public struct FoulCommittedMessage_New
{
    public string PlayerId;
    public string Reason;
}


public struct TurnChangedMessage
{
    public string ActivePlayerId;
    public bool IsBallInHand;
}