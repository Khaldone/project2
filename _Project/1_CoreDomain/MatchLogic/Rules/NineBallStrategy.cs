// Assets/_Project/CoreDomain/MatchLogic/Rules/NineBallStrategy.cs
public class NineBallStrategy : IGameRuleset
{
    public string ModeName => "9-Ball Pro";

    //public ShotResult EvaluateShot(TableState tableState, StrikeIntent intent)
    //{
    //    // 9-Ball specific rule: You MUST hit the lowest numbered ball on the table first
    //    int lowestBallOnTable = GetLowestBall(tableState);
    //    int firstBallHit = tableState.GetFirstCollision(intent);


    //    if (firstBallHit != lowestBallOnTable)
    //    {
    //        return new ShotResult { IsFoul = true, FoulReason = "Failed to hit the lowest ball first." };
    //    }


    //    return new ShotResult { IsFoul = false };
    //}
    // ...
}