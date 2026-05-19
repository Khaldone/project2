using System.Linq;

namespace ibc.game
{
    public class EightBallWinCondition : IWinCondition
    {
        public bool CheckWin(GameContext context)
        {
            if (!context.SidesAssigned)
            {
                return false;
            }

            var targetBalls = context.GetCurrentPlayerTargetBalls();
            return targetBalls.All(ballId => context.PocketedBalls.Contains(ballId));
        }
    }
}