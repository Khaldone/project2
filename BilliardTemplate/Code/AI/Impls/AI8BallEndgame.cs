using ibc.ai;
using ibc.objects;
using ibc.solvers;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using PhysicsScene = ibc.solvers.PhysicsScene;

namespace ibc.game
{
    public class AI8BallEndgame : MonoBehaviour
    {
        [SerializeField] private UnityEvent<int> GameOver8Ball;
        
        [SerializeField] private AIManager _aiManager;
        [SerializeField] private AISettings _aiSettings;
        [SerializeField] private AISceneEvaluator8Ball _aiSceneEvaluator;
        
        private void Awake()
        {
            _aiManager.StopThinkingEvent += StopThinkingEvent;
        }

        private void StopThinkingEvent(StopThinkingReason reason)
        {
            if (_aiManager.BestOutcomeSet)
            {
                if (IsGameover(_aiManager.BestOutcome.Scene))
                {
                    if (GetHoleIdentifier(_aiManager.BestOutcome.Scene, _aiManager.BestOutcome.Events, out var holeIdentifier))
                    {
                        GameOver8Ball?.Invoke(holeIdentifier);
                        Debug.Log($"GameOver: 8Ball is potted in {holeIdentifier}");
                    }
                    else
                    {
                        Debug.LogError($"Unexpected: GameOver but no hole identifier was found for the black ball!");
                    }
                }
            }
        }


        public bool GetHoleIdentifier(PhysicsScene scene, NativeArray<PhysicsSolver.Event> events, out int holeIdentifier)
        {
            foreach (var @event in events)
            {
                if (@event.Type != PhysicsSolver.EventType.PocketCollision) continue;
                if (@event.BallIndex != _aiSettings.BlackBallIdentifier) continue;
                holeIdentifier = scene.Holes[@event.OtherIndex].Identifier;
                return true;
            }

            holeIdentifier = -1;
            return false;
        }
        
        public bool IsGameover(PhysicsScene scene)
        {
            int pocketCount = 0;
            bool blackBallPotted = false;
            bool cueBallPotted = false;
            foreach (var ball in scene.Balls)
            {
                if (ball.State == Ball.StateType.Pocketed)
                {
                    if (ball.Identifier == _aiSettings.CueBallIdentifier)
                    {
                        cueBallPotted = true;
                    }
                    else if (ball.Identifier == _aiSettings.BlackBallIdentifier)
                    {
                        blackBallPotted = true;
                    }
                    else if (_aiSceneEvaluator.GetTargetBalls().Contains(ball.Identifier))
                    {
                        pocketCount++;
                    }
                }
            }

            if (pocketCount == _aiSceneEvaluator.GetTargetBalls().Count)
            {
                if (blackBallPotted)
                {
                    if (cueBallPotted)
                    {
                        
                    }

                    return true;
                }
            }

            return false;
        }
    }

}