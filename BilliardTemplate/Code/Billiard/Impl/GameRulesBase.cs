using System;
using System.Collections.Generic;
using ibc.solvers;

namespace ibc.game
{
    public enum BallType { None, Solid, Stripe, Eight, Cue }
    public enum PlayerSide { None, Solid, Stripe }

    [Flags]
    public enum FoulType
    {
        None = 0,
        CueBallPocketed = 1 << 0,
        NoBallHit = 1 << 1,
        WrongBallHitFirst = 1 << 2,
        Timeout = 1 << 3
    }

    [Serializable]
    public struct TurnResult
    {
        public bool FoulOccurred;
        public FoulType FoulType;
        public bool GameEnded;
        public int WinnerId;
        public string Message;
        public bool BallInHand;
        public bool ContinueTurn;
        public bool RequiresRerack;
    }

    [Serializable]
    public class GameRulesBase
    {
        protected GameContext _context;
        protected IBallClassifier _classifier;
        protected ITurnProcessor _turnProcessor;

        public event Action<int, BallType> OnBallPocketed;
        public event Action<int> OnGameWon;
        public event Action<FoulType> OnFoul;
        public event Action OnTimeWarning;
        public event Action<int> OnTurnChanged;
        public event Action OnGameReset;

        protected GameRulesBase(GameContext context, IBallClassifier classifier, ITurnProcessor turnProcessor)
        {
            _context = context;
            _classifier = classifier;
            _turnProcessor = turnProcessor;
        }

        public GameContext Context => _context;
        public BallType GetBallType(int ballId) => _classifier.GetBallType(ballId);
        public List<int> GetBallsOfType(BallType type) => _classifier.GetBallsByType(type);
        public List<int> GetCurrentPlayerTargetBalls() => _context.GetCurrentPlayerTargetBalls();

        public virtual TurnResult ProcessBreakShot(List<PhysicsSolver.Event> events)
        {
            var result = _turnProcessor.ProcessBreak(events);
            RaiseEventsForResult(result);
            return result;
        }

        public virtual TurnResult ProcessTurn(List<PhysicsSolver.Event> events)
        {
            var result = _turnProcessor.ProcessRegularTurn(events);
            RaiseEventsForResult(result);
            return result;
        }

        public void SwitchPlayer()
        {
            _context.SwitchPlayer();
            OnTurnChanged?.Invoke(_context.CurrentPlayer);
        }

        public void ResetGame()
        {
            _context.Reset();
            OnGameReset?.Invoke();
        }

        public void NotifyTimeWarning() => OnTimeWarning?.Invoke();
        public void RaiseBallPocketed(int ballId) => OnBallPocketed?.Invoke(ballId, GetBallType(ballId));

        private void RaiseEventsForResult(TurnResult result)
        {
            if (result.FoulOccurred) OnFoul?.Invoke(result.FoulType);
            if (result.GameEnded) OnGameWon?.Invoke(result.WinnerId);
            if (!result.ContinueTurn && !result.GameEnded) OnTurnChanged?.Invoke(_context.CurrentPlayer);
        }
    }
}