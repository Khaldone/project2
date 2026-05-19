using System.Collections.Generic;
using ibc.solvers;
using UnityEngine;

namespace ibc.game
{
    public class EightBallTurnProcessor : ITurnProcessor
    {
        private readonly GameContext _context;
        private readonly IBallClassifier _classifier;
        private readonly IBreakValidator _breakValidator;
        private readonly IFoulDetector _foulDetector;
        private readonly IWinCondition _winCondition;
        private readonly int _eightBallId;

        public EightBallTurnProcessor(
            GameContext context,
            IBallClassifier classifier,
            IBreakValidator breakValidator,
            IFoulDetector foulDetector,
            IWinCondition winCondition,
            int eightBallId = 8)
        {
            _context = context;
            _classifier = classifier;
            _breakValidator = breakValidator;
            _foulDetector = foulDetector;
            _winCondition = winCondition;
            _eightBallId = eightBallId;
        }

        public TurnResult ProcessBreak(List<PhysicsSolver.Event> events)
        {
            _context.BreakComplete = true;

            if (_context.TurnPocketedBalls.Contains(_eightBallId))
            {
                return new TurnResult
                {
                    GameEnded = false,
                    Message = "Eight ball pocketed on break. Re-rack.",
                    RequiresRerack = true,
                };
            }
            
            if (!_breakValidator.IsValidBreak(_context.TurnPocketedBalls, events))
            {
                Debug.Log("Invalid break detected - switching player and requiring rerack");
                _context.SwitchPlayer();
                return new TurnResult
                {
                    Message = "Invalid break. Next player breaks.",
                    RequiresRerack = true
                };
            }

            var cueBallId = _classifier.GetBallsByType(BallType.Cue)[0];
            if (_context.TurnPocketedBalls.Contains(cueBallId))
            {
                _context.CurrentFoul = FoulType.CueBallPocketed;
                _context.SwitchPlayer();
                return new TurnResult
                {
                    FoulOccurred = true,
                    FoulType = FoulType.CueBallPocketed,
                    BallInHand = true,
                    Message = "Cue ball scratched on break."
                };
            }

            if (_context.TurnPocketedBalls.Count > 0)
            {
                return new TurnResult
                {
                    ContinueTurn = true,
                    Message = GetSideAssignmentMessage()
                };
            }

            _context.SwitchPlayer();
            return new TurnResult
            {
                Message = "No balls pocketed on break."
            };
        }

        public TurnResult ProcessRegularTurn(List<PhysicsSolver.Event> events)
        {

            bool eightBallPocketed = _context.TurnPocketedBalls.Contains(_eightBallId);
            var foulType = _foulDetector.DetectFoul(_context.TurnPocketedBalls, events, _context);

            if (foulType != FoulType.None)
            {
                return HandleFoul(foulType, eightBallPocketed);
            }

            _context.CurrentFoul = FoulType.None;

            if (eightBallPocketed)
            {
                return HandleEightBallPocketed();
            }

            if (!_context.SidesAssigned && _context.TurnPocketedBalls.Count > 0)
            {
                AssignSidesFromPocketedBalls(_context.TurnPocketedBalls);
            }

            bool pocketedOwnBall = CheckIfPocketedOwnBalls(_context.TurnPocketedBalls);

            if (pocketedOwnBall)
            {
                return new TurnResult
                {
                    ContinueTurn = true,
                    Message = "Continue shooting."
                };
            }

            _context.SwitchPlayer();
            return new TurnResult
            {
                Message = "Turn complete."
            };
        }

        private TurnResult HandleFoul(FoulType foulType, bool eightBallPocketed)
        {
            _context.CurrentFoul = foulType;

            if (eightBallPocketed)
            {
                int opponentId = _context.GetOpponentId();
                _context.GameOver = true;
                _context.WinnerId = opponentId;

                return new TurnResult
                {
                    GameEnded = true,
                    WinnerId = opponentId,
                    FoulOccurred = true,
                    FoulType = foulType,
                    Message = $"Player {_context.CurrentPlayer} loses! Eight ball pocketed on foul."
                };
            }

            _context.SwitchPlayer();
            return new TurnResult
            {
                FoulOccurred = true,
                FoulType = foulType,
                BallInHand = true,
                Message = GetFoulMessage(foulType)
            };
        }

        private TurnResult HandleEightBallPocketed()
        {
            if (_context.AllPlayerBallsPocketed())
            {
                _context.GameOver = true;
                _context.WinnerId = _context.CurrentPlayer;

                return new TurnResult
                {
                    GameEnded = true,
                    WinnerId = _context.CurrentPlayer,
                    Message = $"Player {_context.CurrentPlayer} wins!"
                };
            }

            int opponentId = _context.GetOpponentId();
            _context.GameOver = true;
            _context.WinnerId = opponentId;

            return new TurnResult
            {
                GameEnded = true,
                WinnerId = opponentId,
                Message = $"Player {_context.CurrentPlayer} loses! Eight ball pocketed early."
            };
        }

        private void AssignSidesFromPocketedBalls(List<int> pocketedBalls)
        {
            if (_context.SidesAssigned) return;

            foreach (var ballId in pocketedBalls)
            {
                var ballType = _classifier.GetBallType(ballId);

                if (ballType == BallType.Solid)
                {
                    Debug.Log($"Assigning Player {_context.CurrentPlayer} to Solids");
                    _context.AssignSides(_context.CurrentPlayer, PlayerSide.Solid);
                    return;
                }
                else if (ballType == BallType.Stripe)
                {
                    Debug.Log($"Assigning Player {_context.CurrentPlayer} to Stripes");
                    _context.AssignSides(_context.CurrentPlayer, PlayerSide.Stripe);
                    return;
                }
            }
        }

        private bool CheckIfPocketedOwnBalls(List<int> pocketedBalls)
        {
            if (!_context.SidesAssigned) return false;

            var targetBalls = _context.GetCurrentPlayerTargetBalls();

            foreach (var ballId in pocketedBalls)
            {
                if (targetBalls.Contains(ballId))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetSideAssignmentMessage()
        {
            var side = _context.GetCurrentPlayerSide();
            return side == PlayerSide.Solid ? "You're solids!" : "You're stripes!";
        }

        private string GetFoulMessage(FoulType foulType)
        {
            switch (foulType)
            {
                case FoulType.CueBallPocketed: return "Foul: Cue ball pocketed.";
                case FoulType.NoBallHit: return "Foul: No ball hit.";
                case FoulType.WrongBallHitFirst: return "Foul: Wrong ball hit first.";
                case FoulType.Timeout: return "Foul: Time expired.";
                default: return "Foul occurred.";
            }
        }
    }
}