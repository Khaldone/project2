using System;
using System.Collections.Generic;
using UnityEngine;

namespace ibc.game
{
    [Serializable]
    public class GameContext
    {
        public event Action<int, PlayerSide> OnBallTypeAssigned;
        public event Action OnEightBallOnBreak;
        
        public int CurrentPlayer;
        public PlayerSide Player1Side;
        public PlayerSide Player2Side;
        public bool BreakComplete;
        public bool SidesAssigned;
        public int WinnerId;
        public bool GameOver;
        public FoulType CurrentFoul;
        public List<int> PocketedBalls;
        public List<int> TurnPocketedBalls;
    
        private readonly IBallClassifier _ballClassifier;
        
        public GameContext(IBallClassifier ballClassifier)
        {
            _ballClassifier = ballClassifier;
            CurrentPlayer = 1;
            Player1Side = PlayerSide.None;
            Player2Side = PlayerSide.None;
            BreakComplete = false;
            SidesAssigned = false;
            WinnerId = -1;
            GameOver = false;
            CurrentFoul = FoulType.None;
            PocketedBalls = new List<int>();
            TurnPocketedBalls = new List<int>();
        }

        public void Reset()
        {
            CurrentPlayer = 1;
            Player1Side = PlayerSide.None;
            Player2Side = PlayerSide.None;
            BreakComplete = false;
            SidesAssigned = false;
            WinnerId = -1;
            GameOver = false;
            CurrentFoul = FoulType.None;
            PocketedBalls.Clear();
            TurnPocketedBalls.Clear();
        }

        public void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer == 1 ? 2 : 1;
        }

        public PlayerSide GetCurrentPlayerSide()
        {
            return CurrentPlayer == 1 ? Player1Side : Player2Side;
        }

        public int GetOpponentId()
        {
            return CurrentPlayer == 1 ? 2 : 1;
        }

        public void AssignSides(int playerId, PlayerSide side)
        {
            if (SidesAssigned) return;

            if (playerId == 1)
            {
                Player1Side = side;
                Player2Side = side == PlayerSide.Solid ? PlayerSide.Stripe : PlayerSide.Solid;
            }
            else
            {
                Player2Side = side;
                Player1Side = side == PlayerSide.Solid ? PlayerSide.Stripe : PlayerSide.Solid;
            }
            
            SidesAssigned = true;
            OnBallTypeAssigned?.Invoke(playerId, side);
        }

        public List<int> GetCurrentPlayerTargetBalls()
        {
            var side = GetCurrentPlayerSide();
            if (side == PlayerSide.None) return new List<int>();

            bool allCleared = AllPlayerBallsPocketed();
            if (allCleared)
            {
                return _ballClassifier.GetBallsByType(BallType.Eight);
            }

            return _ballClassifier.GetBallsBySide(side);
        }

        public bool AllPlayerBallsPocketed()
        {
            var side = GetCurrentPlayerSide();
            if (side == PlayerSide.None) return false;

            var targetBalls = _ballClassifier.GetBallsBySide(side);
            foreach (var ballId in targetBalls)
            {
                if (!PocketedBalls.Contains(ballId))
                    return false;
            }
            return true;
        }

        public void ClearTurn()
        {
            TurnPocketedBalls.Clear();
        }

        public void OnBallPocketed(int ballId)
        {
            TurnPocketedBalls.Add(ballId);
            
            var ballType = _ballClassifier.GetBallType(ballId);
            if (ballType != BallType.Cue && ballType != BallType.Eight)
            {
                if (!PocketedBalls.Contains(ballId))
                {
                    PocketedBalls.Add(ballId);
                    Debug.Log($"Added ball {ballId} to pocketed balls");
                }
            }
        }
    }
}