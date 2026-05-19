using System.Collections.Generic;
using ibc.solvers;

namespace ibc.game
{
    public class EightBallBreakValidator : IBreakValidator
    {
        private const int MinimumRailHits = 4;
        private readonly int _cueBallId;

        public EightBallBreakValidator(int cueBallId)
        {
            _cueBallId = cueBallId;
        }

        public bool IsValidBreak(List<int> pocketedBalls, List<PhysicsSolver.Event> events)
        {
            bool hasPocketedBall = pocketedBalls.Count > 0 && !pocketedBalls.Contains(_cueBallId);
            if (hasPocketedBall) return true;

            int railHits = CountRailHits(events);
            return railHits >= MinimumRailHits;
        }

        private int CountRailHits(List<PhysicsSolver.Event> events)
        {
            int count = 0;
            foreach (var ev in events)
            {
                if (ev.Type == PhysicsSolver.EventType.CushionCollision && ev.BallIndex != _cueBallId)
                {
                    count++;
                }
            }
            return count;
        }
    }
}