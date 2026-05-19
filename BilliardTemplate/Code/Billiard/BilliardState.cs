/*
 *  Created by Dragutin Sredojevic.
 *  https://www.nitugard.com
 */

using System;
using UnityEngine.Events;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace ibc
{
    using objects;
    using solvers;

    [Serializable]
    /// <summary>Class that manages billiard state simulation and interpolation.</summary>
    public class BilliardState : IDisposable
    {
        /// <summary>An event raised by the physics simulation system.</summary>
        public UnityAction<PhysicsSolver.Event> OnPhysicsEvent;
        /// <summary>An event indicating that stable physics state is changed. 
        /// Called after state becomes stable or before state becomes unstable.</summary>
        public UnityAction<bool> OnStableStateChange;
        /// <summary>Time that passed since next event was acquired.</summary>
        public double Timer => _timer;
        /// <summary> Physics balls used in the physics simulation.</summary>
        public IEnumerable<Ball> PhysicsBalls => _physicsJob.Scene.Balls;
        /// <summary>Temporary balls used to represent state between physics events. These balls do not affect simulation and should be
        /// used whenever ball state is required between physics events otherwise physics balls are proffered.</summary>
        public IEnumerable<Ball> TemporaryBalls => _balls;
        /// <summary>Whether billiard state is stationary(interpolation has finished or next event is none)</summary>
        public bool Stationary => Timer > _physicsJob.Output.GetLastEvent().Time || _physicsJob.Output.GetLastEvent().Type == PhysicsSolver.EventType.None;
        /// <summary>Physics solver used by the physics job.</summary>
        public PhysicsSolver Solver => _physicsJob.Solver;

        private Ball[] _balls;
        private PhysicsJob _physicsJob;
        private double _timer;
        private bool _wasStable;
        private double _accumulator;
        private float _step;

        /// <summary>Initializes a new instance of the <see cref="T:ibc.BilliardStateBase" /> class by copying from another billiard state.</summary>
        /// <param name="src">The source.</param>
        /// <param name="jobConstants"></param>
        /// <param name="solverConstants"></param>
        /// <param name="allocator"></param>
        public BilliardState(BilliardState src, PhysicsJobConstants jobConstants,
            PhysicsSolverConstants solverConstants, Allocator allocator) : 
            this(new BilliardManagedScene(src._physicsJob.Scene.Balls.ToArray(), src._physicsJob.Scene.Holes.ToArray(), src._physicsJob.Scene.Cushions.ToArray()), jobConstants, solverConstants, allocator)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:ibc.BilliardStateBase" /> class.</summary>
        /// <param name="scene">The billiard scene used to provide starting state and world information.</param>
        /// <param name="jobConstants">The job constants.</param>
        /// <param name="solverConstants">The solver constants.</param>
        /// <param name="allocator">The allocator.</param>
        public BilliardState(BilliardManagedScene scene, PhysicsJobConstants jobConstants, PhysicsSolverConstants solverConstants, Allocator allocator)
        {
            var physicsSolver = new PhysicsSolver(solverConstants);
            var physicsScene = new PhysicsScene(scene, allocator);
            var physicsOutput = new PhysicsJobOutput(jobConstants, allocator);

            _balls = scene.Balls.ToArray();
            _physicsJob = new PhysicsJob(physicsSolver, physicsScene, physicsOutput, jobConstants);
            _timer = 0;
            _wasStable = true;
            _accumulator = 0;
            _step = (float)jobConstants.MinimumEventTime;
            
            //force the job to compile?
            _physicsJob.Schedule().Complete();
        }

        /// <summary>Steps the billiard state forward by dt amount.</summary>
        /// <param name="dt">The dt.</param>
        public void Tick(float dt)
        {
            _accumulator += dt;

            while (_accumulator > _step)
            {
                if (!TryAcquireNextEvent(out var reachedTargetEvent))
                    _accumulator = 0;

                _physicsJob.Solver.Step(_physicsJob.Scene, _balls, _timer);
                _accumulator -= _step;
                _timer += _step;

                IntegrateRotation(_step);
            }
        }

        private bool TryAcquireNextEvent(out bool reachedTargetEvent)
        {
            var lastEvent = _physicsJob.Output.GetLastEvent();
            reachedTargetEvent = _timer > lastEvent.Time;
            bool noEvent = lastEvent.Type == PhysicsSolver.EventType.None;

            if (reachedTargetEvent || noEvent)
            {
                try
                {
                    InvokePhysicsEventCbk(lastEvent);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

                _physicsJob.Schedule().Complete();
                ChangeStateAndInvokeStateChangeEvents();

                return lastEvent.Type != PhysicsSolver.EventType.None;
            }

            return true;
        }

        private void ChangeStateAndInvokeStateChangeEvents()
        {
            //check if next event(towards witch state is transitioned) is acquired
            //if so reset timer used for smoothly transitioning to the new state and send on stable state change event
            if (_physicsJob.Output.GetLastEvent().Type != PhysicsSolver.EventType.None)
            {
                if (_wasStable)
                {
                    try
                    {
                        OnStableStateChange?.Invoke(false);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                    _wasStable = false;
                }
                
                for (int i = 0; i < _physicsJob.Output.GetEventCount() - 1; ++i)
                    InvokePhysicsEventCbk(_physicsJob.Output.Events[i]);

                _timer = 0f;
            }
            else
            {
                if (!_wasStable)
                {
                    for (int i = 0; i < _physicsJob.Output.GetEventCount() - 1; ++i)
                        InvokePhysicsEventCbk(_physicsJob.Output.Events[i]);

                    try
                    {
                        OnStableStateChange?.Invoke(true);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                    _wasStable = true;
                }
                else if (_physicsJob.Output.GetEventCount() > 1)
                {
                    try
                    {
                        OnStableStateChange?.Invoke(false);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                    
                    for (int i = 0; i < _physicsJob.Output.GetEventCount(); ++i)
                        InvokePhysicsEventCbk(_physicsJob.Output.Events[i]);
                    
                    try
                    {
                        OnStableStateChange?.Invoke(true);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        public void IntegrateRotation(float dt)
        {
            //integration of rotation
            for (var i = 0; i < _balls.Length; i++)
            {
                Ball ball = _balls[i];

                if (ball.State == Ball.StateType.Normal)
                {
                    var deltaRot = quaternion.Euler((float3)ball.AngularVelocity * dt);
                    ball.Rotation = math.mul(deltaRot, ball.Rotation);
                    _balls[i] = ball;
                }
            }
        }

        private void InvokePhysicsEventCbk(PhysicsSolver.Event ev)
        {
            try
            {
                OnPhysicsEvent?.Invoke(ev);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>Gets the temporary ball.</summary>
        /// <param name="identifier">The ball identifier.</param>
        public Ball GetTemporaryBall(int identifier)
        {
            for (int i = 0; i < _balls.Length; ++i)
            {
                if (_balls[i].Identifier == identifier)
                    return _balls[i];
            }

            throw new ArgumentOutOfRangeException();
        }

        /// <summary>Gets the temporary ball.</summary>
        /// <param name="identifier">The ball index.</param>
        public Ball GetTemporaryBallByIndex(int index)
        { 
            return _balls[index];
        }

        /// <summary>Gets the physics ball.</summary>
        /// <param name="identifier">The ball identifier.</param>
        public bool TryGetPhysicsBall(int identifier, out Ball ball)
        {
            for (int i = 0; i < _physicsJob.Scene.Balls.Length; ++i)
            {
                if (_physicsJob.Scene.Balls[i].Identifier == identifier)
                {
                    ball = _physicsJob.Scene.Balls[i];
                    return true;
                }
            }

            ball = default;
            return false;
        }
        
        public bool TryGetPhysicsBallIndex(int identifier, out int ballIndex)
        {
            for (int i = 0; i < _physicsJob.Scene.Balls.Length; ++i)
            {
                if (_physicsJob.Scene.Balls[i].Identifier == identifier)
                {
                    ballIndex = i;
                    return true;
                }
            }

            ballIndex = -1;
            return false;
        }

        /// <summary>Gets the physics ball.</summary>
        /// <param name="identifier">The ball identifier.</param>
        public Ball GetPhysicsBallByIndex(int index)
        {
            return _physicsJob.Scene.Balls[index];
        }

        /// <summary>Sets the physics ball.</summary>
        /// <param name="identifier">The ball identifier.</param>
        public void SetPhysicsBall(Ball ball)
        {
            for(int i=0; i< _physicsJob.Scene.Balls.Length; ++i)
            {
                if (_physicsJob.Scene.Balls[i].Identifier == ball.Identifier)
                {
                    _physicsJob.Scene.Balls[i] = ball;
                    return;
                }
            }

            throw new ArgumentOutOfRangeException();
        }

        /// <summary>
        /// Resets balls to match the provided ones, event output is also reset. Timer is set to zero.
        /// This is done in order to invalidate all events and force the calculation of the new ones since the ball state
        /// has changed.
        /// </summary>
        public void Reset(Ball[] balls)
        {
            _physicsJob.Reset(balls);
            _balls = balls.ToArray();
            _physicsJob.Output.ResetEventCount();
            _timer = 0;
        }

        /// <summary>
        /// Returns the number of balls.
        /// </summary>
        public int GetBallCount()
        {
            return _balls.Length;
        }

        /// <summary>Returns the <strong>copy</strong> of the physics balls.</summary>
        public Ball[] GetPhysicsBalls()
        {
            return _physicsJob.Scene.Balls.ToArray();
        }

        /// <summary>
        /// Disposes this instance releasing all used resources.
        /// </summary>
        public void Dispose()
        {
            _physicsJob.Dispose();
        }

        public PhysicsScene GetPhysicsScene()
        {
            return _physicsJob.Scene;
        }
    }
}