using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ibc.commands;
using Unity.Mathematics;
using ibc.highlight;
using ibc.trajectory;

namespace ibc.game
{
    public class GameManager : MonoBehaviour, ITargetBallValidator
    {
        [Header("Core References")]
        [SerializeField] private Billiard _billiard;
        [SerializeField] private PlayerInputManager _inputManager;
        [SerializeField] private PowerMeterController powerMeter;
        [SerializeField] private CueStickController _cueStickController;
        [SerializeField] private AimlineManager _aimlineManager;
        [SerializeField] private BallInHandController _ballInHandController;
        [SerializeField] private HighlightBallsManager _highlightManager;
        [SerializeField] private PowerMeterAnimator _powerMeterAnimator;

        [Header("Game Settings")]
        [SerializeField] private float _turnDuration = 60f;
        [SerializeField] private float _aimlineMinStrikeVelocity = 0.1f;
        [SerializeField] private float _turnStartDelay = 1f;
        [SerializeField] private float _cueBallResetDelay = 2f;
        
        [Header("Turn timer")]
        [SerializeField] private string _timerTickSfxName = "tick";
        [SerializeField] private bool _tickAlternate = true;
        [SerializeField] private float _tickStartWindow = 5f;
        [SerializeField] private Vector2 _tickIntervalRange = new Vector2(0.70f, 0.15f);
        [SerializeField] private Vector2 _tickPitchOffsetRange = new Vector2(0.00f, 0.35f);
        [SerializeField] private AnimationCurve _tickVolumeCurve = AnimationCurve.EaseInOut(0f, 0.45f, 1f, 1f);

        private Coroutine _tickCoroutine;
        private float _timeRemainingThisTurn;
        
        private EightBallRules _gameRules;
        private bool _canShoot;
        private bool _cueBallPocketed;
        private Coroutine _turnTimerCoroutine;
        private bool _hadPhysicsThisTurn;

        private readonly Stack<Billiard.ResetPoint> _resetHistory = new();
        private Billiard.ResetPoint _frameResetPoint;
        private Billiard.ResetPoint _turnResetPoint; 

        public GameRulesBase Rules => _gameRules;
        public GameContext Context => _gameRules.Context;
        public float TurnDuration => _turnDuration;

        public event Action<int> OnTurnStarted;
        public event Action<float> OnTimerUpdated;
        public event Action OnBallPocketedUpdate;
        public event Action OnSidesAssigned;
        public event Action<int> OnGameWonEvent;
        public event Action OnGameResetEvent;

        
        private void Awake()
        {
            _gameRules = EightBallRules.Default;
        }

        private void Start()
        {
            SubscribeToEvents();
            StartNewGame();
        }

        private void SubscribeToEvents()
        {
            _billiard.StateChanged += OnBilliardStateChanged;
            _billiard.BallPocketed += OnBallPocketed;
            _ballInHandController.OnDragBegin += _ => DisableShootingControls();
            _ballInHandController.OnDragEnd += _ => EnableShootingControls();

            _gameRules.Context.OnBallTypeAssigned += OnBallTypeAssigned;
            _gameRules.OnTurnChanged += OnTurnChanged;
            _gameRules.OnGameReset += () => _highlightManager?.StopHighlighting();
            _gameRules.OnGameWon += OnGameWon;
            _gameRules.OnFoul += OnFoul;
            
            powerMeter.OnStrikeEvent.AddListener(OnStrike);
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents()
        {
            if (_billiard != null)
            {
                _billiard.StateChanged -= OnBilliardStateChanged;
                _billiard.BallPocketed -= OnBallPocketed;
            }

            if (powerMeter != null)
                powerMeter.OnStrikeEvent.RemoveListener(OnStrike);

            if (_cueStickController != null)
                _cueStickController.OnStrikeImpact -= OnCueStickImpact;

            if (_gameRules != null)
            {
                _gameRules.Context.OnBallTypeAssigned -= OnBallTypeAssigned;
                _gameRules.OnTurnChanged -= OnTurnChanged;
                _gameRules.OnGameWon -= OnGameWon;
                _gameRules.OnFoul -= OnFoul;
            }
        }
        
        private void PushReset(Billiard.ResetPoint p)
        {
            if (p != null) _resetHistory.Push(p);
        }

        private void RestoreTo(Billiard.ResetPoint p)
        {
            if (p == null) return;
            _billiard.ResetTo(p);
            _billiard.ClearCachedEvents();
        }

        private void CaptureFrameResetPoint()
        {
            _frameResetPoint = _billiard.CreateResetPoint(new NullCommand("frame-start"));
            PushReset(_frameResetPoint);
        }

        private void CaptureTurnResetPoint()
        {
            _turnResetPoint = _billiard.CreateResetPoint(new NullCommand("turn-start"));
            PushReset(_turnResetPoint);
        }
        
        public void RestartGame()
        {
            StopTurnTimer();
            RestoreTo(_frameResetPoint);
            StartNewGame();
        }

        private void OnBallTypeAssigned(int playerId, PlayerSide ballType)
        {
            Debug.Log($"Ball type assigned: Player {playerId} = {ballType}");
            UpdateBallHighlighting();
            OnSidesAssigned?.Invoke();
        }

        private void OnTurnChanged(int currentPlayer)
        {
            Debug.Log($"Turn changed to Player {currentPlayer}");
            UpdateBallHighlighting();
        }

        private void OnGameWon(int winnerId)
        {
            Debug.Log($"Game won by Player {winnerId}");
            OnGameWonEvent?.Invoke(winnerId);
        }

        private void OnFoul(FoulType foulType)
        {
            Debug.Log($"Foul occurred: {foulType}");
        }

        private void StartNewGame()
        {
            StopTurnTimer();
            _resetHistory.Clear();
            _gameRules.ResetGame();
            _canShoot = false;
            _highlightManager?.StopHighlighting();
            _cueStickController?.Hide();
            _ballInHandController.Disable();
            _billiard.ClearCachedEvents();
            
            CaptureFrameResetPoint();
            CaptureTurnResetPoint();

            Debug.Log("Starting new game - Player 1 breaks");
            StartTurn(default);
            OnGameResetEvent?.Invoke();
        }

        private void OnBilliardStateChanged(bool isStable)
        {
            if (isStable)
            {
                OnStableStateReached();
            }
            else
            {
                OnPhysicsStarted();
            }
        }

        private void OnPhysicsStarted()
        {
            _hadPhysicsThisTurn = true;
            _cueBallPocketed = false;
            _canShoot = false;
            _gameRules.Context.ClearTurn();
            _highlightManager?.StopHighlighting();
            _ballInHandController.Disable();
            _billiard.ClearCachedEvents();
            _powerMeterAnimator?.Hide();

            DisableCue();
        }

        private void OnStableStateReached()
        {
            if (!_hadPhysicsThisTurn)
            {
                Debug.Log("Stable state reached but no physics this turn – ignoring.");
                return;
            }

            ProcessTurnResult();
        }

        private void ProcessTurnResult()
        {
            _cueBallPocketed = _gameRules.Context.TurnPocketedBalls.Contains(_billiard.CueBallIdentifier);

            TurnResult result = !_gameRules.Context.BreakComplete
                ? _gameRules.ProcessBreakShot(_billiard.GetCachedEvents())
                : _gameRules.ProcessTurn(_billiard.GetCachedEvents());

            Debug.Log($"Turn result: {result.Message} | Continue: {result.ContinueTurn} | Foul: {result.FoulOccurred} | GameEnded: {result.GameEnded} | Rerack: {result.RequiresRerack}");
            Debug.Log($"Current Player after processing: {_gameRules.Context.CurrentPlayer}");

            if (result.RequiresRerack)
            {
                HandleRerack(result);
            }
            else if (result.GameEnded)
            {
                HandleGameEnd(result);
            }
            else if (result.FoulOccurred)
            {
                HandleFoul(result);
            }
            else
            {
                HandleNormalTurnEnd(result);
            }
        }

        private void HandleRerack(TurnResult result)
        {
            StopTurnTimer();
            Debug.Log("Handling rerack - rewinding to frame reset point");
            StartCoroutine(RerackSequence(result));
        }
        

        private IEnumerator RerackSequence(TurnResult result)
        {
            yield return new WaitForSeconds(2f);
            RestoreTo(_frameResetPoint);

            _gameRules.Context.BreakComplete = false;
            _gameRules.Context.TurnPocketedBalls.Clear();
            _gameRules.Context.PocketedBalls.Clear();
            _cueBallPocketed = false;

            StartTurn(default);
        }


        private void ResetCueBall()
        {
            var cueBallId = _billiard.CueBallIdentifier;
            if (!_cueBallPocketed) return;

            Debug.Log("Resetting cue ball to starting position");
            _billiard.Take(cueBallId);
            
            if (_billiard.State.TryGetPhysicsBall(cueBallId, out var cueBall))
            {
                // find valid position
                float3 targetPosition = (float3)_billiard._cueBallResetPosition.position;
                float3 validPosition = _ballInHandController.FindValidPosition(targetPosition, cueBall);
                
                cueBall.Velocity = double3.zero;
                cueBall.AngularVelocity = double3.zero;
                cueBall.Position = validPosition;
                _billiard.State.SetPhysicsBall(cueBall);
                
                Debug.Log($"Cue ball reset to position: {validPosition}");
            }
        }

        private void HandleGameEnd(TurnResult result)
        {
            StopTurnTimer();
            _highlightManager?.StopHighlighting();
            Debug.Log($"Game ended: {result.Message}");
        }

        private void HandleFoul(TurnResult result)
        {
            StopTurnTimer();
            Debug.Log($"Handling foul: {result.Message}");
            StartCoroutine(DelayedTurnStart(result));
        }

        private void HandleNormalTurnEnd(TurnResult result)
        {
            Debug.Log($"Normal turn end: {result.Message}");
            StartCoroutine(DelayedTurnStart(result));
        }

        private IEnumerator DelayedTurnStart(TurnResult result)
        {
            if (_cueBallPocketed)
            {
                yield return new WaitForSeconds(_cueBallResetDelay);
                ResetCueBall();
            }

            yield return new WaitForSeconds(_turnStartDelay);
            StartTurn(result);
        }

        private void StartTurn(TurnResult result)
        {
            Debug.Log($"Starting turn for Player {_gameRules.Context.CurrentPlayer}");
            OnTurnStarted?.Invoke(_gameRules.Context.CurrentPlayer);

            _canShoot = true;
            _hadPhysicsThisTurn = false;

            EnableCue();
            StartTurnTimer();
            UpdateBallHighlighting();
            _cueStickController?.ResetOffset();

            bool isBreakPhase = !_gameRules.Context.BreakComplete;
            _ballInHandController.SetBreakPhase(isBreakPhase);
            
            CaptureTurnResetPoint();
            _powerMeterAnimator?.Show();
            
            if (result.BallInHand || isBreakPhase)
            {
                Debug.Log($"Enabling ball in hand (BreakPhase: {isBreakPhase}, BallInHand: {result.BallInHand})");
                _ballInHandController.Enable();
            }
        }

        private void EnableCue()
        {
            _inputManager?.SetEnabled(true);
            powerMeter?.SetInteractable(true);
            powerMeter?.ResetSlider();

            _cueStickController?.PrepareForTurn(() => _aimlineManager.Show());

            if (_billiard.State.TryGetPhysicsBall(_billiard.CueBallIdentifier, out var ball))
                _cueStickController?.SetCueBallPosition((float3)ball.Position);
        }

        private void DisableCue()
        {
            _aimlineManager?.Hide();
            _inputManager?.SetEnabled(false);
            powerMeter?.SetInteractable(false);
        }

        private void DisableShootingControls()
        {
            _cueStickController?.Hide();
            _aimlineManager?.Hide();
            _inputManager?.SetEnabled(false);
            powerMeter?.SetInteractable(false);
        }

        private void EnableShootingControls()
        {
            _cueStickController?.PrepareForTurn(() => _aimlineManager.Show());

            if (_billiard.State.TryGetPhysicsBall(_billiard.CueBallIdentifier, out var ball))
                _cueStickController?.SetCueBallPosition((float3)ball.Position);

            _inputManager?.SetEnabled(true);
            powerMeter?.SetInteractable(true);
        }

        private void Update()
        {
            if (_billiard.State.Stationary)
            {
                var strikeCommand = GetStrikeCommand();
                strikeCommand.Velocity = Mathf.Max(_aimlineMinStrikeVelocity, strikeCommand.Velocity);
                var minVelocity = powerMeter.CalculateSpringTerminalVelocity(0);
                var maxVelocity = powerMeter.CalculateSpringTerminalVelocity(1);
                _aimlineManager.SetStrikeData(strikeCommand, minVelocity, maxVelocity);
            }
        }

        private void OnStrike(float velocity)
        {
            if (!_canShoot || !_billiard.State.Stationary) return;

            Debug.Log($"Strike initiated by Player {_gameRules.Context.CurrentPlayer} with velocity {velocity}");
            StopTurnTimer();
            _cueStickController.OnStrikeImpact += OnCueStickImpact;
            _cueStickController?.ExecuteStrike(velocity);
            _canShoot = false;
            DisableCue();
        }

        private void OnCueStickImpact(float velocity)
        {
            _cueStickController.OnStrikeImpact -= OnCueStickImpact;

            var cue = _billiard.GetCueData();
            var orientation = _inputManager.GetCueStickJaw();
            float2 spinOffset = _cueStickController.GetOffset();

            var strikeCommand = new StrikeCommand(
                _billiard.CueBallIdentifier,
                velocity,
                spinOffset,
                cue,
                orientation,
                0f);

            if (_turnResetPoint != null)
                _turnResetPoint.Command = strikeCommand;
            
            if (!_billiard.ExecuteStrikeCommand(strikeCommand))
            {
                Debug.LogWarning("Strike command failed to execute.");
                _canShoot = true;
                EnableCue();
            }
        }

        private void OnBallPocketed(Billiard.BallPocketEventData data)
        {
            Debug.Log($"Ball {data.BallIdentifier} pocketed");
            _gameRules.Context.OnBallPocketed(data.BallIdentifier);
            _gameRules.RaiseBallPocketed(data.BallIdentifier);
            OnBallPocketedUpdate?.Invoke();
        }

        private void StartTurnTimer()
        {
            _turnTimerCoroutine = StartCoroutine(TurnTimerRoutine());
        }

        private void StopTurnTimer()
        {
            if (_turnTimerCoroutine != null)
            {
                StopCoroutine(_turnTimerCoroutine);
                _turnTimerCoroutine = null;
            }
            if (_tickCoroutine != null)
            {
                StopCoroutine(_tickCoroutine);
                _tickCoroutine = null;
            }
        }

        private IEnumerator TurnTimerRoutine()
        {
            float elapsed = 0f;
            bool timeWarningSent = false;

            while (elapsed < _turnDuration)
            {
                elapsed += Time.deltaTime;
                float remaining = _turnDuration - elapsed;
                _timeRemainingThisTurn = remaining; 

                float t = Mathf.Clamp01(remaining / _turnDuration);
                OnTimerUpdated?.Invoke(t);

                if (!timeWarningSent && remaining <= _tickStartWindow)
                {
                    _gameRules.NotifyTimeWarning();
                    timeWarningSent = true;

                    if (_tickCoroutine == null)
                        _tickCoroutine = StartCoroutine(TickRoutine());
                }

                yield return null;
            }

            OnTimeExpired();
        }
        
        private void OnTimeExpired()
        {
            Debug.Log("Time expired");
            OnTimerUpdated?.Invoke(0f);
            _gameRules.Context.CurrentFoul = FoulType.Timeout;
            _gameRules.SwitchPlayer();

            var result = new TurnResult
            {
                FoulOccurred = true,
                FoulType = FoulType.Timeout,
                BallInHand = true,
                Message = "Time expired! Ball in hand."
            };

            HandleFoul(result);
        }
        
        private IEnumerator TickRoutine()
        {
            if (!AudioManager.Instance) yield break;

            while (_timeRemainingThisTurn > 0f && _timeRemainingThisTurn <= _tickStartWindow)
            {
                float x = 1f - Mathf.Clamp01(_timeRemainingThisTurn / _tickStartWindow);
                float interval = Mathf.Lerp(_tickIntervalRange.x, _tickIntervalRange.y, x);
                float pitchOffset = Mathf.Lerp(_tickPitchOffsetRange.x, _tickPitchOffsetRange.y, x);
                float vol = Mathf.Clamp01(_tickVolumeCurve.Evaluate(x));

                AudioManager.Instance.PlaySFX(
                    _timerTickSfxName,
                    volume: vol,
                    pitchOffset: pitchOffset,
                    loop: false
                );

                yield return new WaitForSeconds(interval);
            }

            _tickCoroutine = null;
        }

        private void UpdateBallHighlighting()
        {
            _highlightManager?.StopHighlighting();

            if (!_gameRules.Context.SidesAssigned)
            {
                Debug.Log("Sides not assigned yet - no highlighting");
                return;
            }

            var targetBalls = _gameRules.GetCurrentPlayerTargetBalls();
            Debug.Log($"Highlighting {targetBalls.Count} target balls for Player {_gameRules.Context.CurrentPlayer}");
            
            if (targetBalls != null && targetBalls.Count > 0)
            {
                _highlightManager?.HighlightBalls(targetBalls);
            }
        }

        private StrikeCommand GetStrikeCommand()
        {
            return new StrikeCommand(
                _billiard.CueBallIdentifier,
                powerMeter.GetTerminalVelocity(),
                float2.zero,
                _billiard.GetCueData(),
                _inputManager.GetCueStickJaw());
        }

        public bool IsValidTarget(int ballIdentifier)
        {
            return _gameRules.IsValidTarget(ballIdentifier);
        }
    }
}