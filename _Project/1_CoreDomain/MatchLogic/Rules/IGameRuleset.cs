// Assets/_Project/CoreDomain/MatchLogic/Rules/IGameRuleset.cs
public interface IGameRuleset
{
    //bool IsLegalBreak(TableState state);
    //bool IsFoul(BallCollisionData firstCollision, TableState state);
    //string CheckForWinner(TableState state);
    //void SetupRack(IPhysicsSimulator physicsManager);
    string ModeName { get; }

    //// Validates if the shot was legal (e.g., in 9-ball, did they hit the lowest number first?)
    //ShotResult EvaluateShot(TableState tableState, StrikeIntent intent);

    //// Determines what happens to a sunken ball (e.g., cue ball scratches go back to hand)
    //BallPocketedResult HandleBallPocketed(BallState pocketedBall);

    //// Checks if someone won or lost the game
    //MatchConclusion CheckWinCondition(TableState tableState, string activePlayerId);

}