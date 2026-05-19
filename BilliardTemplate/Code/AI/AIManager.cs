/*
 *  Created by Dragutin Sredojevic.
 *  https://www.nitugard.com
 */
using System;
using System.Collections;
using System.Collections.Generic;
using ibc.objects;
using ibc.solvers;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using PhysicsScene = ibc.solvers.PhysicsScene;
using Random = UnityEngine.Random;


namespace ibc.ai
{

    public enum StopThinkingReason
    {
        Invalid,
        Unexpected,
        Finished,
        TimeRunOut,
        OptimalStrikeFound,
        UserRequest,
    }

    public class AIManager : MonoBehaviour
    {
        private struct ScheduledJob
        {
            public JobHandle JobHandle;
            public Job JobStruct;
            public int Index;
        }

        public struct StrikeDataOutcome : IDisposable
        {
            public PhysicsScene Scene;
            public NativeArray<PhysicsSolver.Event> Events;

            public StrikeData StrikeData;
            public float Score;

            public void Dispose()
            {
                if (Scene.IsCreated)
                    Scene.Dispose();

                if (Events.IsCreated)
                    Events.Dispose();

            }
        }

        [Serializable]
        public class SettingsSerializable
        {
            public string Name;
            
            public float MinThinkingTime = 1f;
            public float MaxThinkingTime = 2f;
            
            public int MinScoreToStrike = 0;
            public int MaxScoreToStrike = 2;

            public float AnglePerturbationSigmaEvaluation;
            public float AnglePerturbationSigmaStrike;
            
            public AIStrikeProvider StrikeProvider;
            public AISceneEvaluatorBase _sceneEvaluator;
        }

        [BurstCompile]
        public struct Job : IJob, IDisposable
        {
            public float MaxTime;
            public int MaxEvents;
            public StrikeData StrikeData;

            public PhysicsSolver Solver;
            public PhysicsScene Scene;
            
            public NativeList<PhysicsSolver.Event> Events;

            public Job(StrikeData strikeData, PhysicsSolver physicsSolver, PhysicsScene physicsScene, float maxTime, int maxEvents)
            {
                StrikeData = strikeData;
                Solver = physicsSolver;
                Scene = new PhysicsScene(physicsScene, Allocator.Persistent);
                Events = new NativeList<PhysicsSolver.Event>(Allocator.Persistent);
                MaxTime = maxTime;
                MaxEvents = maxEvents;
            }

            public void Execute()
            {
                PhysicsSolver.Event currentEvent = default;
                double accTime = 0;
                int iterations = 0;

                Ball b = Scene.Balls[StrikeData.CueBallIndex];
                if (Solver.ResolveBallCueImpact(ref b, StrikeData.CueData, StrikeData.Velocity, StrikeData.Orientation,
                        0,
                        StrikeData.Offset))
                {
                    b.State = Ball.StateType.Struck;
                    Scene.Balls[StrikeData.CueBallIndex] = b;
                    do
                    {
                        Solver.Step(Scene, currentEvent);
                        currentEvent = Solver.GetNextEvent(Scene);
                        Events.Add(currentEvent);
                        accTime += math.max(currentEvent.Time, 0);
                        iterations++;
                    } while (accTime <= MaxTime && iterations < MaxEvents);

                    Solver.Step(Scene, currentEvent);
                }
            }


            public void Dispose()
            {
                Scene.Dispose();
                Events.Dispose();
            }
        }

        public UnityAction StartThinkingEvent;
        public UnityAction<StopThinkingReason> StopThinkingEvent;
        public UnityAction BestOutcomeChangeEvent;

        public bool IsThinking => _isThinking;
        public float ThinkingTime => _thinkingTime;
        
        public bool BestOutcomeSet { get; private set; }
        public StrikeDataOutcome BestOutcome => _bestOutcome;
        public SettingsSerializable ActiveSettings => _settings[_settingsIndex];

        [Header("Settings")] 
        [SerializeField] private int _maxConcurrentJobs = 4;
        [SerializeField] private float _maxSimulationTime = 10f;
        [SerializeField] private int _maxEventsPerSimulation = 1024;
        [SerializeField] private float _doNothingScore = -4f;

        [Header("References")]
        [SerializeField] private PhysicsJobConstantsSerializable _jobConstants;
        [SerializeField] private PhysicsSolverConstantsSerializable _solverConstants;
        [SerializeField] private SettingsSerializable[] _settings;

        [Header("Debug")] [SerializeField] private bool _debug = true;
        
        private Coroutine _routine;
        private BilliardState _state;
        private int _settingsIndex;
        private StrikeDataOutcome _bestOutcome;
        private int _jobIndex;
        private float _thinkingTime;
        private bool _isThinking;
        private float _initialStateScore;
        
        private List<ScheduledJob> _scheduledJobs = new List<ScheduledJob>();
        private Queue<StrikeData> _strikeQueue = new Queue<StrikeData>();

        private void Awake()
        {
            _routine = null;
            Random.InitState(0);
        }

        
        public void StartThinking(BilliardState state, int settingsIndex)
        {
            if (!state.Stationary)
            {
                if(_debug) Debug.LogWarning("Could not start AI, reason: state not stationary");
                return;
            }

            if (_isThinking)
            {
                if(_debug) Debug.LogWarning("Could not start AI, reason: already running");
                return;
            }

            _state = new BilliardState(state, _jobConstants.Data, _solverConstants.Data, Allocator.Persistent);
            _settingsIndex = settingsIndex;
            _jobIndex = 0;
            _thinkingTime = 0;
            _isThinking = true;
            
            //TODO: set best outcome to something?
            BestOutcomeSet = false;
            _bestOutcome = default;
            _bestOutcome.Score = _initialStateScore + _doNothingScore;
            
            _initialStateScore = _settings[settingsIndex]._sceneEvaluator.EvaluatePhysicsScene(_state.GetPhysicsScene());
            
            InitializeStrikeQueue();
            StartThinkingEvent?.Invoke();
            _routine = StartCoroutine(AIRoutine());
        }

        private void InitializeStrikeQueue()
        {
            var settings = _settings[_settingsIndex];
            var strikes = settings.StrikeProvider.GetStrikes(_state);
            for (var i = 0; i < strikes.Count; i++)
            {
                _strikeQueue.Enqueue(strikes[i]);
            }
            
            if(_debug) Debug.Log($"Total queued strikes: {_strikeQueue.Count}");
        }

        public void StopThinking(StopThinkingReason reason)
        {
            if (_isThinking)
            {
                if(_debug) Debug.Log("Stopped thinking");
                StopCoroutine(_routine);
                _routine = null;
                _isThinking = false;

                foreach (var strikeJobData in _scheduledJobs)
                {
                    if (!strikeJobData.JobHandle.IsCompleted)
                    {
                        strikeJobData.JobHandle.Complete();
                        CompleteStrikeEvaluation(strikeJobData);
                    }
                }
                
                _scheduledJobs.Clear();
                _state.Dispose();
                
                StopThinkingEvent?.Invoke(reason);
            }
            
            _bestOutcome.Dispose();

            if(_debug) Debug.Log($"Best outcome: {_bestOutcome.Score} {_bestOutcome.StrikeData.ToString()}");
        }

        private void OnDestroy()
        {
            StopThinking(StopThinkingReason.Unexpected);
        }


        private IEnumerator AIRoutine()
        {
            while (true)
            {
                if (_scheduledJobs.Count < _maxConcurrentJobs)
                {
                    if (_strikeQueue.TryDequeue(out var strikeData))
                        ScheduleStrikeEvaluation(strikeData);
                }

                for (var i = _scheduledJobs.Count - 1; i >= 0; i--)
                {
                    var scheduledJob = _scheduledJobs[i];
                    if (scheduledJob.JobHandle.IsCompleted)
                    {
                        scheduledJob.JobHandle.Complete();
                        CompleteStrikeEvaluation(scheduledJob);
                        _scheduledJobs.RemoveAt(i);
                    }
                }

                if (_strikeQueue.Count == 0 && _scheduledJobs.Count == 0)
                {
                    if(_debug) Debug.Log("Run through all jobs");
                    StopThinking(StopThinkingReason.Finished);
                }


                if (_isThinking)
                {
                    var setting = _settings[_settingsIndex];
                    if (_thinkingTime > setting.MaxThinkingTime)
                    {
                        if(_debug) Debug.Log("Max time reached");
                        StopThinking(StopThinkingReason.TimeRunOut);
                    }
                    
                    if (_thinkingTime > setting.MinThinkingTime)
                    {
                        var score = _bestOutcome.Score - _initialStateScore;
                        if (score > setting.MinScoreToStrike)
                        {
                            if(_debug) Debug.Log("Found optimal strike");
                            StopThinking(StopThinkingReason.OptimalStrikeFound);
                        }
                    }
                    
                    _thinkingTime += Time.deltaTime;
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private void CompleteStrikeEvaluation(ScheduledJob job)
        {
            var setting = _settings[_settingsIndex];
            var score = setting._sceneEvaluator.EvaluatePhysicsScene(job.JobStruct.Scene);
            score += setting._sceneEvaluator.EvaluateEvents(job.JobStruct.Scene, job.JobStruct.Events);
            

            if (_bestOutcome.Score < score)
            {
                var relativeScore = score - _initialStateScore;
                if (relativeScore < setting.MaxScoreToStrike)
                {
                    StrikeDataOutcome outcome;
                    outcome.StrikeData = job.JobStruct.StrikeData;
                    outcome.Score = score;
                    outcome.Scene = new PhysicsScene(job.JobStruct.Scene, Allocator.Persistent);
                    outcome.Events = job.JobStruct.Events.ToArray(Allocator.Persistent);
                    
                    _bestOutcome.Dispose();
                    _bestOutcome = outcome;
                    BestOutcomeSet = true;
                    BestOutcomeChangeEvent?.Invoke();
                }
            }

            if(_debug) Debug.Log($"Completed strike evaluation for job: {job.Index} Score: {score}");
            job.JobStruct.Dispose();
        }

        private void ScheduleStrikeEvaluation(StrikeData strikeData)
        {
            strikeData.Orientation += NormalRandom(_settings[_settingsIndex].AnglePerturbationSigmaEvaluation);
            Job job = new Job(strikeData, _state.Solver, _state.GetPhysicsScene(), _maxSimulationTime, _maxEventsPerSimulation);
            JobHandle jobHandle = job.Schedule();
            
            _scheduledJobs.Add(new ScheduledJob()
            {
                JobHandle = jobHandle,
                JobStruct = job,
                Index = _jobIndex,
            });
            
            if(_debug) Debug.Log($"Job {_jobIndex} scheduled: {strikeData.ToString()}");
            _jobIndex++;
        }
        
        public static float NormalRandom(float sigma, float mu = 0)
        {
            if (sigma == 0f)
                return mu;
            
            float rand1 = Random.Range(0.0f, 1.0f);
            float rand2 = Random.Range(0.0f, 1.0f);
            float n = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Cos((2.0f * Mathf.PI) * rand2);
            return mu + sigma * n;
        }
    }
}