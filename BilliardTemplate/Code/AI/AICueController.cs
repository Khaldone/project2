using System;
using System.Collections;
using System.Collections.Generic;
using ibc.commands;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ibc.ai
{
    public class AICueController : MonoBehaviour
    {
        [SerializeField] private AIManager _aiManager;
        [SerializeField] private AISettings _aiSettings;
        [SerializeField] private Billiard _billiard;
        [SerializeField] private Transform _cueTransform;
        

        [Header("Settings")] 
        [SerializeField] private Vector2 _orientationLerpSpeedMinMax = new Vector2(1, 2);
        [SerializeField] private float _cueDefaultDistance = 0.015f;

        [Header("Strike")]
        [SerializeField] private Vector2 _strikeDelayTimeMinMax = new Vector2(1, 2);
        [SerializeField] private float _maxStrikeVelocity = 5f;
        [SerializeField] private Vector2 _strikeDurationMinMax = new Vector2(0.25f, .5f);
        [SerializeField] private Vector2 _cueDrawDistanceMinMax = new Vector2(0.05f, 0.2f);
        [SerializeField] private float _strikeTimeThresholdNormalized = 0.9f;
        
        [Header("Curves")] 
        [SerializeField] private AnimationCurve _strikeDistanceCurve;
        [SerializeField] private List<AnimationCurve> _sensitivityCurves;
        
        private List<AIManager.StrikeDataOutcome> _bestOutcomes = new();
        private AIManager.StrikeDataOutcome _bestOutcome;
        
        private float _targetOrientation;
        private float _targetOrientationLerpTime;
        private Quaternion _cuePreviousOrientation;
        private bool _canStrike;
        private int _sensitivityCurveIndex;
        private float _orientationLerpSpeed;
        private bool _isPerformingStrike;
        
        private void Awake()
        {
            _aiManager.StartThinkingEvent += StartThinkingEvent;
            _aiManager.StopThinkingEvent += StopThinkingEvent;
            _aiManager.BestOutcomeChangeEvent += BestOutcomeChangeEvent;
        }

        private void BestOutcomeChangeEvent()
        {
            var bestOutcome = _aiManager.BestOutcome;

            if (_bestOutcomes.Count == 0)
            {
                OrientTowardsNextBestOutcome(bestOutcome);
            }

            _bestOutcomes.Add(bestOutcome);
        }

        private void OrientTowardsNextBestOutcome(AIManager.StrikeDataOutcome bestOutcome)
        {
            bestOutcome.StrikeData.Orientation += AIManager.NormalRandom(_aiManager.ActiveSettings.AnglePerturbationSigmaStrike);
            _targetOrientationLerpTime = 0;
            _targetOrientation = bestOutcome.StrikeData.Orientation;
            _cuePreviousOrientation = _cueTransform.rotation;
            _sensitivityCurveIndex = Random.Range(0, _sensitivityCurves.Count);
            _orientationLerpSpeed =
                Mathf.Lerp(_orientationLerpSpeedMinMax.x, _orientationLerpSpeedMinMax.y, Random.value);
            _bestOutcome = bestOutcome;
        }

        private void Update()
        {
            if (!_canStrike) 
                return;
            
            var targetRot = Quaternion.Euler(0, _targetOrientation, 0);
            var sensitivity01 = _sensitivityCurves[_sensitivityCurveIndex].Evaluate(_targetOrientationLerpTime);
            var cueBallFound = _billiard.State.TryGetPhysicsBall(_aiSettings.CueBallIdentifier, out var cueBall);
            Debug.Assert(cueBallFound);
            
            var cueBallPosition = (Vector3)(float3) cueBall.Position;
            _cueTransform.position = cueBallPosition - _cueTransform.rotation * Vector3.forward * _cueDefaultDistance;

            _cueTransform.rotation = Quaternion.SlerpUnclamped(_cuePreviousOrientation, targetRot, sensitivity01);

            if (_targetOrientationLerpTime > 1)
            {
                if (_bestOutcomes.Count > 0)
                {
                    var bestOutcome = _bestOutcomes[0];
                    _bestOutcomes.RemoveAt(0);
                    OrientTowardsNextBestOutcome(bestOutcome);
                }
                else if (!_aiManager.IsThinking && _billiard.State.Stationary && _canStrike)
                {
                    StartCoroutine(StrikeRoutine());
                    _canStrike = false;
                    _isPerformingStrike = true;
                }
            }
            else
            {
                _targetOrientationLerpTime += Time.deltaTime * _orientationLerpSpeed;
            }
        }

        private IEnumerator StrikeRoutine()
        {
            var strikeDelay = Mathf.Lerp(_strikeDelayTimeMinMax.x, _strikeDelayTimeMinMax.y, Random.value);
            yield return new WaitForSeconds(strikeDelay);
            var velocityNormalized = Mathf.InverseLerp(0, _maxStrikeVelocity, _bestOutcome.StrikeData.Velocity);
            var drawDistance = Mathf.Lerp(_cueDrawDistanceMinMax.x, _cueDrawDistanceMinMax.y, velocityNormalized);
            var strikeDuration = Mathf.Lerp(_strikeDurationMinMax.x, _strikeDurationMinMax.y, velocityNormalized);
            var cueBallFound = _billiard.State.TryGetPhysicsBall(_aiSettings.CueBallIdentifier, out var cueBall);
            Debug.Assert(cueBallFound);
            var cueBallPosition = (Vector3)(float3) cueBall.Position;
            var strikeTimer = 0f;
            var distanceToBall = 0f;
            var strikePerformed = false;

            while (strikeTimer < strikeDuration)
            {
                var t = strikeTimer / strikeDuration;
                _cueTransform.position = cueBallPosition - _cueTransform.rotation * Vector3.forward * distanceToBall;

                distanceToBall = Mathf.LerpUnclamped(0, drawDistance, _strikeDistanceCurve.Evaluate(t));
                if (t > _strikeTimeThresholdNormalized && !strikePerformed)
                {
                    PerformStrike();
                    strikePerformed = true;
                }
                strikeTimer += Time.deltaTime;
                yield return null;
            }

            if (!strikePerformed)
            {
                PerformStrike();
            }

            _cueTransform.gameObject.SetActive(false);
            _isPerformingStrike = false;
        }

        public void Cancel()
        {
            _bestOutcomes.Clear();
            _canStrike = false;
        }


        private void StartThinkingEvent()
        {
            _bestOutcomes.Clear();
            _canStrike = true;
            _cueTransform.gameObject.SetActive(true);
            _isPerformingStrike = false;
        }

        
        private void StopThinkingEvent(StopThinkingReason arg0)
        {
            switch (arg0)
            {
                case StopThinkingReason.Invalid:
                    break;
                case StopThinkingReason.Unexpected:
                    break;
                case StopThinkingReason.Finished:
                case StopThinkingReason.TimeRunOut:
                case StopThinkingReason.OptimalStrikeFound:
                case StopThinkingReason.UserRequest:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(arg0), arg0, null);
            }
        }

        private void PerformStrike()
        {
            _billiard.ExecuteStrikeCommand(new StrikeCommand(
                _aiSettings.CueBallIdentifier, _bestOutcome.StrikeData.Velocity,
                _bestOutcome.StrikeData.Offset, _bestOutcome.StrikeData.CueData,
                _bestOutcome.StrikeData.Orientation, 0));
        }
    }
}