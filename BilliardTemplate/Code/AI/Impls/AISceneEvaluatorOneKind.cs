using ibc.objects;
using ibc.solvers;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using PhysicsScene = ibc.solvers.PhysicsScene;

namespace ibc.ai
{
    public class AISceneEvaluatorOneKind : AISceneEvaluatorBase
    {
        [SerializeField] private int _cueBallIdentifier;
        [SerializeField, FormerlySerializedAs("targetBallPotted")] private float _targetBallPotted = 1;
        [SerializeField, FormerlySerializedAs("whiteBallPotted")] private float _whiteBallPotted = -3;
        
        public override float EvaluatePhysicsScene(PhysicsScene physicsScene)
        {
            
            float score = 0;
            foreach (var ball in physicsScene.Balls)
            {
                if (ball.State == Ball.StateType.Pocketed)
                {
                    if (ball.Identifier == _cueBallIdentifier)
                    {
                        score += _whiteBallPotted;
                    }
                    else
                    {
                        score += _targetBallPotted;
                    }
                }
            }
            
            return score;
        }

        public override float EvaluateEvents(PhysicsScene physicsScene, NativeList<PhysicsSolver.Event> events)
        {
            return 0;
        }
    }
}