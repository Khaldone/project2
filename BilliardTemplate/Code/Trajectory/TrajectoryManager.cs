/*
 *  Created by Dragutin Sredojevic.
 *  https://www.nitugard.com
 */
using System;
using ibc.commands;
using ibc.objects;
using ibc.solvers;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using PhysicsScene = ibc.solvers.PhysicsScene;

namespace ibc.trajectory
{
    /// <summary>Class that manages ball trajectory display.</summary>
    public class TrajectoryManager : MonoBehaviour
    {
        /// <summary>
        /// Post collision data.
        /// </summary>
        public struct OutputData
        {
            public bool ObjectBallHit;
            public bool CushionHit;
            public bool PocketHit;

            public Vector3 InitialCueBallPosition;
            public Vector3 InitialCueBallVelocity;

            public Vector3 CueBallPosition;
            public Vector3 CueBallDirection;
            
            public int ObjectBallIdentifier;
            public int ObjectBallIndex;
            
            public Vector3 ObjectBallPosition;
            public Vector3 ObjectBallDirection;
            public float ObjectBallVelocity;

        }

        [BurstCompile]
        struct Job : IJob
        {
            public PhysicsSolver Solver;
            public PhysicsScene Scene;
            public NativeArray<PhysicsSolver.Event> CollisionEvent;

            public NativeArray<Ball> TemporaryBalls;

            public void Execute()
            {
                //perform simulation
                PhysicsSolver.Event currentEvent = default;
                double accTime = 0;
                int iterations = 0;
                do
                {
                    Solver.Step(Scene, currentEvent);
                    currentEvent = Solver.GetNextEvent(Scene);

                    CollisionEvent[0] = currentEvent;
                    accTime += math.max(currentEvent.Time, 0);
                    iterations++;
                } while (accTime <= 10f && iterations < 1024 && !PhysicsSolver.IsCollisionEvent(currentEvent.Type));


                Solver.Step(Scene, currentEvent);
                Solver.Step(Scene, TemporaryBalls, 0);
            }
        }

        [SerializeField] private Billiard _billiard;
        
        private bool _initialized;
        
        public OutputData CalculateOutput(StrikeCommand strikeCommand)
        {
            OutputData data = new OutputData();
            
            //cache the physics scene
            var tempState = _billiard.GetStateCopy(Allocator.Persistent);
            
            //perform strike
            strikeCommand.Execute(tempState);
            tempState.TryGetPhysicsBall(strikeCommand.BallIdentifier, out var ball);
            var velocity = ball.Velocity;

            PhysicsScene physicsScene = tempState.GetPhysicsScene();
            Job job = new Job()
            {
                CollisionEvent = new NativeArray<PhysicsSolver.Event>(1, Allocator.TempJob),
                TemporaryBalls = new NativeArray<Ball>(tempState.GetBallCount(), Allocator.TempJob),
                Scene = physicsScene,
                Solver = tempState.Solver,
            };

            job.Schedule().Complete();
            var lastEvent = job.CollisionEvent[0];

            
            tempState.TryGetPhysicsBallIndex(strikeCommand.BallIdentifier, out var cueBallIndex);

            //update points
            var cueBall = job.TemporaryBalls[cueBallIndex];

            data.InitialCueBallPosition = (float3)_billiard.State.GetPhysicsBallByIndex(cueBallIndex).Position;
            data.CueBallPosition = (float3)cueBall.Position;
            data.CueBallDirection = math.normalizesafe((float3)cueBall.Velocity);
            data.InitialCueBallVelocity = (float3)velocity;
            
            if (lastEvent.Type == PhysicsSolver.EventType.BallCollision)
            {
                data.ObjectBallHit = true;
                
                int objectBallIndex = lastEvent.BallIndex;
                if (objectBallIndex == cueBallIndex)
                    objectBallIndex = lastEvent.OtherIndex;
                
                var objectBall = job.TemporaryBalls[objectBallIndex];
                data.ObjectBallIndex = objectBallIndex;
                data.ObjectBallIdentifier = objectBall.Identifier;
                data.ObjectBallPosition = (float3)(objectBall.Position);
                data.ObjectBallDirection = (float3) math.normalizesafe(objectBall.Velocity);
                data.ObjectBallVelocity = (float)math.length(objectBall.Velocity);

            }
            else if (lastEvent.Type == PhysicsSolver.EventType.CushionCollision)
            {
                data.CushionHit = true;
            }
            else if (lastEvent.Type == PhysicsSolver.EventType.PocketCollision)
            {
                data.PocketHit = true;
            }

            job.TemporaryBalls.Dispose();
            job.CollisionEvent.Dispose();
            tempState.Dispose();
            return data;
        }

    }
}