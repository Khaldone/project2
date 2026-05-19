using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Windows.Input;
using System;
using System.Linq;
using ibc.audio;
using Unity.Collections;

namespace ibc
{
    using unity;
    using objects;
    using solvers;
    using commands;

    
    /// <summary>Class that ties billiard state with unity scene.</summary>
    public sealed class Billiard : MonoBehaviour
    {
        public struct BallPocketEventData
        {
            public int BallIdentifier;
            public int PocketIdentifier;
        }

        public BillardSfxManager BilliardSfxManager;
        public Action<BallPocketEventData> BallPocketed;
        public Action<bool> StateChanged;
        public int CueBallIdentifier => _cueBallIdentifier;
        public int FirstBallHitIdentifier { get; private set; }

        /// <summary>
        /// Reset point data container.
        /// </summary>
        [Serializable]
        public class ResetPoint
        {
            public IBilliardCommand Command;
            public Ball[] Balls;
            public DateTime Time;
            public object UserData;
        }

        /// <summary>Job constants used for simulation via unity jobs.</summary>
        public PhysicsJobConstants JobConstants => _jobConstants.Data;

        /// <summary>Solver constants used during physics simulation.</summary>
        public PhysicsSolverConstants SolverConstants => _solverConstants.Data;

        /// <summary>Managed state of the billiard.</summary>
        public BilliardState State => _state;

        [SerializeField] public Transform _cueBallResetPosition;
        [SerializeField] private PhysicsJobConstantsSerializable _jobConstants;
        [SerializeField] private PhysicsSolverConstantsSerializable _solverConstants;
        [SerializeField] private int _cueBallIdentifier;
        [SerializeField] private Cue _activeCueData = Cue.Default;
        
        private BilliardState _state;
        private BilliardManagedScene _managedScene;
        private List<PhysicsSolver.Event> _cachedEvents;
        private BilliardUnityScene _unityScene; 
        private bool _cueBallPocketed;

        
        private void Awake()
        {
            var balls = FindObjectsOfType<UnityBall>();
            var cushions = FindObjectsOfType<UnityCushion>();
            var holes = FindObjectsOfType<UnityHole>();
            
            Array.Sort(balls, (b0, b1) => b0.Identifier.CompareTo(b1.Identifier));
            Array.Sort(cushions, (b0, b1) => b0.Identifier.CompareTo(b1.Identifier));
            Array.Sort(holes, (b0, b1) => b0.Identifier.CompareTo(b1.Identifier));

            _unityScene = new BilliardUnityScene() {
                Balls = balls,
                Holes = holes,
                Cushions = cushions,
                Cues = FindObjectsOfType<UnityCue>(),
            };

            _cachedEvents = new List<PhysicsSolver.Event>();
            _managedScene = new BilliardManagedScene(_unityScene);
            _state = new BilliardState(_managedScene, _jobConstants.Data, _solverConstants.Data, Allocator.Persistent);

            State.OnPhysicsEvent += OnPhysicsEvent;
            State.OnStableStateChange += OnStableStateChange;
            
        }

        public bool ExecuteStrikeCommand(StrikeCommand strikeCommand)
        {
            FirstBallHitIdentifier = -1;
            bool result = strikeCommand.Execute(State);
            if (BilliardSfxManager != null)
                BilliardSfxManager.OnStrike(strikeCommand.Velocity);
            return result;
        }

        private void Update()
        {

            _state.Tick(Time.deltaTime);

            foreach(var unityBall in _unityScene.Balls)
            {
                Ball ball = _state.GetTemporaryBall(unityBall.Identifier);
                if (ball.Identifier != unityBall.Identifier) Debug.Log($"Mismatch {ball.Identifier} {unityBall.Identifier}");
                if (ball.State != Ball.StateType.Normal) continue;
                unityBall.transform.SetPositionAndRotation((float3)ball.Position, ball.Rotation);
            }
        }

        private void OnPhysicsEvent(PhysicsSolver.Event ev)
        {
            if(ev.Type != PhysicsSolver.EventType.None)
               _cachedEvents.Add(ev);
            
            var ballIdentifier = _unityScene.Balls[ev.BallIndex].Identifier;

            if (ev.Type == PhysicsSolver.EventType.PocketCollision)
            {
                BallPocketEventData ballPocketEventData = new BallPocketEventData()
                {
                    BallIdentifier = ballIdentifier,
                    PocketIdentifier = _unityScene.Holes[ev.OtherIndex].Identifier
                };

                
                Put(ballPocketEventData);

                if (ballIdentifier == _cueBallIdentifier)
                {
                    _cueBallPocketed = true;
                    Debug.Log("Cue ball pocketed");
                }

            }
            else if (ev.Type == PhysicsSolver.EventType.BallCollision)
            {
                var otherBallIdentifier = _unityScene.Balls[ev.OtherIndex].Identifier;
                var nonCueBallIdentifier = ballIdentifier;
                if (ballIdentifier == _cueBallIdentifier)
                    nonCueBallIdentifier = otherBallIdentifier;
                if (FirstBallHitIdentifier == -1)
                    FirstBallHitIdentifier = nonCueBallIdentifier;
            }
            
            if (BilliardSfxManager != null)
                BilliardSfxManager.OnPhysicsEvent(ev);

        }


        /// <summary>
        /// Takes the pocketed ball from the hole.
        /// </summary>
        /// <param name="ballIdentifier">The ball identifier.</param>
        /// <returns><c>true</c> if ball is removed, <c>false</c> otherwise.</returns>
        public bool Take(int ballIdentifier)
        {
            if (!State.TryGetPhysicsBall(ballIdentifier, out var ball))
            {
                Debug.LogError($"Ball not found: {ballIdentifier}");
                return false;
            }
            
            foreach (var hole in _unityScene.Holes)
            {
                hole.GetComponent<UnityHoleDrop>().Remove(ballIdentifier);
            }

            if (ball.State == Ball.StateType.Pocketed)
            {
                ball.State = Ball.StateType.Normal;
                State.SetPhysicsBall(ball);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Puts the specified ball into a hole.
        /// </summary>
        public void Put(BallPocketEventData ballPocketEventData)
        {
            var holeIdentifier = ballPocketEventData.PocketIdentifier;
            var ballIdentifier = ballPocketEventData.BallIdentifier;
            
            var hole = _unityScene.Holes.GetTarget(holeIdentifier);
            var holeDrop = hole.GetComponent<UnityHoleDrop>();
            var unityBall = _unityScene.Balls.GetTarget(ballIdentifier);
            if (!State.TryGetPhysicsBall(unityBall.Identifier, out var ball))
            {
                Debug.LogError($"Ball not found: {ballIdentifier}");
                return;
            }
            ball.State = Ball.StateType.Pocketed;
            holeDrop.Put(unityBall.Identifier, unityBall.transform, (float3)ball.Velocity, (float3)ball.AngularVelocity, (float)_solverConstants.Data.Gravity, () => { });
            State.SetPhysicsBall(ball);
            BallPocketed?.Invoke(ballPocketEventData);
            Debug.Log($"<color=lightblue>Pocketed ball {ball.Identifier}</color>");
        }

        /// <summary>
        /// Creates the reset point.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>ResetPoint.</returns>
        public ResetPoint CreateResetPoint(IBilliardCommand command)
        {
            return CreateResetPoint(command, DateTime.Now);
        }
        
        
        public BilliardState GetStateCopy(Allocator allocator)
        {
            return new BilliardState(_state, _jobConstants.Data, _solverConstants.Data, allocator);
        }

        /// <summary>
        /// Creates the reset point.
        /// </summary>
        /// <param name="command">The command that was applied after reset point is created .</param>
        /// <param name="time">The time of the reset point.</param>
        /// <param name="userData">The user data, if any.</param>
        /// <returns>ResetPoint.</returns>
        /// <exception cref="Exception">Throws if billiard state is not stationary</exception>
        public ResetPoint CreateResetPoint(IBilliardCommand command, DateTime time, object userData = null)
        {
            if (!State.Stationary)
                throw new Exception("Billiard state must be stationary");

            return new ResetPoint() { Balls = State.GetPhysicsBalls(), Time = time, Command = command, UserData = userData };
        }

        /// <summary>
        /// Called when billiard [stable state change].
        /// </summary>
        private void OnStableStateChange(bool isStable)
        {
            StateChanged?.Invoke(isStable);
        }



        /// <summary>
        /// Returns an enumerable ball identifiers where each ball is in pocketed state.
        /// </summary>
        /// <param name="balls">The balls.</param>
        /// <returns>Returns an enumerable ball identifiers.</returns>
        public static IEnumerable<int> GetPocketedBalls(params Ball[] balls)
        {
            return balls.Where(b => b.State == Ball.StateType.Pocketed).Select(t => t.Identifier);
        }

        public List<PhysicsSolver.Event> GetCachedEvents()
        {
            return _cachedEvents;
        }

        /// <summary>
        /// Resets the billiard state to a cached reset point. Command is not applied.
        /// </summary>
        public void ResetTo(ResetPoint point)
        {
            foreach (var hole in _unityScene.Holes)
            {
                UnityHoleDrop unityHoleDrop = hole.GetComponent<UnityHoleDrop>();
                if (unityHoleDrop)
                    unityHoleDrop.Reset(point.Balls);
            }

            ClearCachedEvents();
            State.Reset(point.Balls);   
        }

        private void OnDestroy()
        {
            _state.Dispose();
        }

        public Cue GetCueData()
        {
            return _activeCueData;
        }

        public void ClearCachedEvents()
        {
            _cachedEvents.Clear();
        }
    }
}