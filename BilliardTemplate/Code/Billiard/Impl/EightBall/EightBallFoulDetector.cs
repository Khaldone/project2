using System.Collections.Generic;
using System.Linq;
using ibc.solvers;

namespace ibc.game
{
    public class EightBallFoulDetector : IFoulDetector
    {
        private readonly int _cueBallId;
        private readonly int _eightBallId;

        public EightBallFoulDetector(int cueBallId, int eightBallId)
        {
            _cueBallId = cueBallId;
            _eightBallId = eightBallId;
        }

        public FoulType DetectFoul(List<int> pocketedBalls, List<PhysicsSolver.Event> events, GameContext context)
        {
            if (pocketedBalls.Contains(_cueBallId))
                return FoulType.CueBallPocketed;

            if (!DidCueBallHitAnyBall(events))
                return FoulType.NoBallHit;

            if (context.SidesAssigned)
            {
                int firstBallHit = GetFirstBallHit(events);
                if (firstBallHit == -1)
                    return FoulType.NoBallHit;

                var targetBalls = context.GetCurrentPlayerTargetBalls();
                if (!targetBalls.Contains(firstBallHit))
                    return FoulType.WrongBallHitFirst;
            }

            return FoulType.None;
        }

        private bool DidCueBallHitAnyBall(List<PhysicsSolver.Event> events)
        {
            return events.Any(e => 
                (e.BallIndex == _cueBallId || e.OtherIndex == _cueBallId) && 
                e.Type == PhysicsSolver.EventType.BallCollision);
        }

        private int GetFirstBallHit(List<PhysicsSolver.Event> events)
        {
            var firstHit = events.FirstOrDefault(e =>
                (e.BallIndex == _cueBallId || e.OtherIndex == _cueBallId) && 
                e.Type == PhysicsSolver.EventType.BallCollision);

            if (firstHit.Type == PhysicsSolver.EventType.None)
                return -1;

            return firstHit.BallIndex == _cueBallId ? firstHit.OtherIndex : firstHit.BallIndex;
        }
    }
}