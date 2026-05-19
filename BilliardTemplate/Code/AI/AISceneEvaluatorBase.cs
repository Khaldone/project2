using ibc.solvers;
using Unity.Collections;
using UnityEngine;
using PhysicsScene = ibc.solvers.PhysicsScene;

namespace ibc.ai
{
    public abstract class AISceneEvaluatorBase : MonoBehaviour
    {
        public abstract float EvaluatePhysicsScene(PhysicsScene physicsScene);
        
        public abstract float EvaluateEvents(PhysicsScene physicsScene, NativeList<PhysicsSolver.Event> events);

    }
}