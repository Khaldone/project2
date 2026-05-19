using System;
using System.Collections.Generic;
using ibc.objects;
using ibc.solvers;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using PhysicsScene = ibc.solvers.PhysicsScene;

namespace ibc.ai
{
    public class AISceneEvaluator8Ball : AISceneEvaluatorBase
    {
        [SerializeField] private AISettings _aiSettings; 

        [Header("Heuristics")]
        [SerializeField, FormerlySerializedAs("whiteBallPotted")] private float _whiteBallPotted = -3;
        [SerializeField, FormerlySerializedAs("blackBallPottedFail")] private float _blackBallPottedFail = -8;
        [SerializeField, FormerlySerializedAs("blackBallPottedWin")] private float _blackBallPottedWin = 1;
        [SerializeField, FormerlySerializedAs("avoidBallPotted")] private float _avoidBallPotted = -1;
        [SerializeField, FormerlySerializedAs("targetBallPotted")] private float _targetBallPotted = 1;
        
        [Header("Heuristics Events")]
        [SerializeField, FormerlySerializedAs("invalidBallHitFirst")] private float _invalidBallHitFirst = -1f;
        [SerializeField, FormerlySerializedAs("cueBallPerCushionHit")] private float _cueBallPerCushionHit = -0.5f;
        
        //TODO: heuristics regarding ending positions of the balls


        private SideToPlay SideToPlay => _aiSettings.SideToPlay;

        public List<int> GetTargetBalls()
        {
            if (SideToPlay == SideToPlay.Solid) return _aiSettings.SolidIdentifiers;
            return _aiSettings.StripeIdentifiers;
        }

        public List<int> GetAvoidBalls()
        {
            if (SideToPlay == SideToPlay.Solid) return _aiSettings.StripeIdentifiers;
            return _aiSettings.SolidIdentifiers;
        }

        public override float EvaluatePhysicsScene(PhysicsScene physicsScene)
        {
            float score = 0;
            int pocketCount = 0;
            bool blackBallPotted = false;
            bool cueBallPotted = false;
            foreach (var ball in physicsScene.Balls)
            {
                if (ball.State == Ball.StateType.Pocketed)
                {
                    if (ball.Identifier == _aiSettings.CueBallIdentifier)
                    {
                        score += _whiteBallPotted;
                        cueBallPotted = true;
                    }
                    else if (ball.Identifier == _aiSettings.BlackBallIdentifier)
                    {
                        blackBallPotted = true;
                    }
                    else if (GetTargetBalls().Contains(ball.Identifier))
                    {
                        score += _targetBallPotted;
                        pocketCount++;
                    }
                    else if (GetAvoidBalls().Contains(ball.Identifier))
                    {
                        score += _avoidBallPotted;
                    }
                }
            }

            if (pocketCount == GetTargetBalls().Count)
            {
                if (blackBallPotted)
                {
                    if (cueBallPotted)
                        score += _blackBallPottedFail;
                    else
                        score += _blackBallPottedWin;
                }
                else if (cueBallPotted)
                    score += _whiteBallPotted;
            }
            
            return score;
        }

        public override float EvaluateEvents(PhysicsScene physicsScene, NativeList<PhysicsSolver.Event> events)
        {
            float score = 0;
            int collisionEventCounter = 0;
            foreach (var @event in events)
            {
                var ball1Identifier = physicsScene.Balls[@event.BallIndex].Identifier;
                
                bool isCollisionEvent = PhysicsSolver.IsCollisionEvent(@event.Type);
                if (isCollisionEvent )
                    collisionEventCounter++;
                
                switch (@event.Type)
                {
                    case PhysicsSolver.EventType.None:
                        break;
                    case PhysicsSolver.EventType.StateTransition:
                        break;
                    case PhysicsSolver.EventType.BallCollision:
                        
                        var ball2Identifier = physicsScene.Balls[@event.OtherIndex].Identifier;
                        if (_aiSettings.CueBallIdentifier == ball2Identifier)
                            (ball1Identifier, ball2Identifier) = (ball2Identifier, ball1Identifier);
                        
                        if (collisionEventCounter == 1)
                        {
                            if (GetAvoidBalls().Contains(ball2Identifier))
                            {
                                score += _invalidBallHitFirst;
                            }
                        }

                        break;
                    case PhysicsSolver.EventType.CushionCollision:
                        if (ball1Identifier == _aiSettings.CueBallIdentifier)
                            score += _cueBallPerCushionHit;
                        break;
                    case PhysicsSolver.EventType.PocketCollision:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return score;
        }
    }
}