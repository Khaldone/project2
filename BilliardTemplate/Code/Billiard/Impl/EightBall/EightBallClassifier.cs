using System.Collections.Generic;

namespace ibc.game
{
    public class EightBallClassifier : IBallClassifier
    {
        private readonly int _cueBallId;
        private readonly int _eightBallId;
        private readonly List<int> _solidBalls;
        private readonly List<int> _stripeBalls;

        public EightBallClassifier(int cueBallId = 0, int eightBallId = 8)
        {
            _cueBallId = cueBallId;
            _eightBallId = eightBallId;
            _solidBalls = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            _stripeBalls = new List<int> { 9, 10, 11, 12, 13, 14, 15 };
        }

        public BallType GetBallType(int ballId)
        {
            if (ballId == _cueBallId) return BallType.Cue;
            if (ballId == _eightBallId) return BallType.Eight;
            if (_solidBalls.Contains(ballId)) return BallType.Solid;
            if (_stripeBalls.Contains(ballId)) return BallType.Stripe;
            return BallType.None;
        }

        public List<int> GetBallsByType(BallType type)
        {
            switch (type)
            {
                case BallType.Solid: return new List<int>(_solidBalls);
                case BallType.Stripe: return new List<int>(_stripeBalls);
                case BallType.Eight: return new List<int> { _eightBallId };
                case BallType.Cue: return new List<int> { _cueBallId };
                default: return new List<int>();
            }
        }

        public List<int> GetBallsBySide(PlayerSide side)
        {
            switch (side)
            {
                case PlayerSide.Solid: return new List<int>(_solidBalls);
                case PlayerSide.Stripe: return new List<int>(_stripeBalls);
                default: return new List<int>();
            }
        }
    }
}