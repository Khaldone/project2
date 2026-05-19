using System.Collections.Generic;

namespace ibc.game
{
    public interface IBallClassifier
    {
        BallType GetBallType(int ballId);
        List<int> GetBallsByType(BallType type);
        List<int> GetBallsBySide(PlayerSide side);
    }
}