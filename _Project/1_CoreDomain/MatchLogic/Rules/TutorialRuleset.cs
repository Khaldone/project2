// Assets/_Project/CoreDomain/MatchLogic/Rules/TutorialRuleset.cs

public class TutorialRuleset : IGameRuleset
{
    public string ModeName => "FTUE_Tutorial";

    //public void SetupRack(IPhysicsSimulator physicsManager)
    //{
    //    // 1. Clear the table completely
    //    physicsManager.ClearAllBalls();


    //    // 2. Place exactly two balls in mathematically perfect, un-missable positions
    //    physicsManager.SpawnBall(BallType.Cue, new Vector3(0, 0, -1.0f));
    //    physicsManager.SpawnBall(BallType.Solid, new Vector3(0, 0, 1.0f)); // Lined up with top-center pocket
    //}

    public void SetupRack()
    {

    }


    // ... simplified foul rules (e.g., no scratching allowed) ...
}