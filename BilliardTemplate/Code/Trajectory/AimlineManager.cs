/*
 *  Created by Dragutin Sredojevic.
 *  https://www.nitugard.com
 */
using System;
using ibc.commands;
using ibc.game;
using Unity.Mathematics;
using UnityEngine;

namespace ibc.trajectory
{
    public class AimlineManager : MonoBehaviour
    {
        struct StrikeVelocity
        {
            public float CurrentStrikeVelocity;
            public float MaxStrikeVelocity;
            public float MinStrikeVelocity;

            public bool HasChanged(StrikeVelocity dest)
            {
                if (Mathf.Abs(CurrentStrikeVelocity - dest.CurrentStrikeVelocity) > 1E-4f)
                    return true;
                if (Mathf.Abs(MaxStrikeVelocity - dest.MaxStrikeVelocity) > 1E-4f)
                    return true;
                if (Mathf.Abs(MinStrikeVelocity - dest.MinStrikeVelocity) > 1E-4f)
                    return true;
                return false;
            }
        }

        [Header("References")] 
        [SerializeField] private SpriteRenderer _cueBallGhost;      // valid ghost
        [SerializeField] private SpriteRenderer _invalidGhost;      // invalid ghost (new)
        [SerializeField] private LineRendererController _cueBallAimLine;
        [SerializeField] private LineRendererController _cueBallLine;
        [SerializeField] private LineRendererController _objectBallLine;
        [SerializeField] private TrajectoryManager _trajectoryManager;
        [SerializeField] private BallColorList _colorList;
        [SerializeField] private float _strikeVelocity = 5f;

        [Header("Target Validation")]
        [SerializeField] private MonoBehaviour _targetValidatorSource;
        [SerializeField] private SpriteRenderer _invalidTargetSprite;   // world-space indicator on object ball

        [Header("Settings")]
        [SerializeField] private float _cueBallLineLength = 0.15f;
        [SerializeField] private float _minObjectBallAimLineLength = 0.05f;
        [SerializeField] private float _maxObjectBallAimLineLength = 0.2f;

        [Header("Debug")] 
        [SerializeField] private bool _debugEnabled;

        private bool _isVisible;
        private StrikeCommand _strikeCommand;
        private StrikeCommand _lastStrikeCommand;
        private StrikeVelocity _strikeVelocityCurrent;
        private StrikeVelocity _strikeVelocityPrevious;
        private bool _forceRecalculate;

        private ITargetBallValidator _targetValidator;
        
        private void Awake()
        {
            if (_targetValidatorSource != null)
            {
                _targetValidator = _targetValidatorSource as ITargetBallValidator;
                if (_targetValidator == null)
                {
                    Debug.LogError($"{nameof(AimlineManager)}: " +
                                   $"{_targetValidatorSource.name} does not implement ITargetBallValidator.");
                }
            }

            if (_invalidTargetSprite != null)
                _invalidTargetSprite.gameObject.SetActive(false);

            if (_invalidGhost != null)
                _invalidGhost.gameObject.SetActive(false);
        }

        private void Start()
        {
            Show();
            _strikeCommand.Velocity = _strikeVelocity;
        }

        private void Update()
        {
            if (!_isVisible)
                return;

            if (!_forceRecalculate &&
                !_strikeVelocityPrevious.HasChanged(_strikeVelocityCurrent) &&
                !StrikeCommandHasChanged(_lastStrikeCommand, _strikeCommand, 1E-4f))
                return;

            UpdateAimLine();
            _strikeVelocityPrevious = _strikeVelocityCurrent;
        }
        
        public void SetStrikeData(StrikeCommand strikeCommand, float minVelocity, float maxVelocity)
        {
            _strikeVelocityCurrent = new StrikeVelocity
            {
                CurrentStrikeVelocity = strikeCommand.Velocity,
                MinStrikeVelocity = minVelocity,
                MaxStrikeVelocity = maxVelocity
            };
            
            _strikeCommand = strikeCommand;
            _strikeCommand.Velocity = _strikeVelocity;
        }

        public void Show()
        {
            _cueBallAimLine.Show();
            _isVisible = true;

            if (_cueBallGhost != null)
                _cueBallGhost.gameObject.SetActive(true);

            if (_invalidGhost != null)
                _invalidGhost.gameObject.SetActive(false);

            if (_invalidTargetSprite != null)
                _invalidTargetSprite.gameObject.SetActive(false);

            UpdateAimLine();
        }

        public void Hide()
        { 
            _isVisible = false;
            _forceRecalculate = true;

            if (_cueBallGhost != null)
                _cueBallGhost.gameObject.SetActive(false);

            if (_invalidGhost != null)
                _invalidGhost.gameObject.SetActive(false);

            ForEachAimLine(controller => controller.Hide());

            if (_invalidTargetSprite != null)
                _invalidTargetSprite.gameObject.SetActive(false);
        }
        
        private bool StrikeCommandHasChanged(StrikeCommand sc1, StrikeCommand sc2, float tolerance)
        {
            if (Math.Abs(sc1.Orientation - sc2.Orientation) > tolerance)
                return true;
            
            if (Math.Abs(sc1.Velocity - sc2.Velocity) > tolerance)
                return true;
            
            if (math.lengthsq(sc1.Offset - sc2.Offset) > tolerance * tolerance)
                return true;
            
            return false;
        }

        private void ForEachAimLine(Action<LineRendererController> action)
        {
            action.Invoke(_cueBallAimLine);
            action.Invoke(_objectBallLine);
            action.Invoke(_cueBallLine);
        }
        
        private void UpdateAimLine()
        {
            var outputData = _trajectoryManager.CalculateOutput(_strikeCommand);

            UpdateCueBallLine(outputData);
            UpdateCueBallGhost(outputData);
            UpdateObjectBallLine(outputData);
            UpdateCueBallAimLine(outputData);
            UpdateInvalidTargetIndicator(outputData);

            _forceRecalculate = false;
            _lastStrikeCommand = _strikeCommand;
            
            if(_debugEnabled) Debug.Log("Aim line updated");
        }

        private void UpdateCueBallLine(TrajectoryManager.OutputData data)
        {
            if (data.PocketHit || data.CushionHit)
            {
                _cueBallLine.Hide();
                return;
            }
            
            bool objectHit = data.ObjectBallHit;
            bool hasValidator = _targetValidator != null;
            bool isInvalidTarget = false;
            if (objectHit && hasValidator)
            {
                isInvalidTarget = !_targetValidator.IsValidTarget(data.ObjectBallIdentifier);
            }

            if (isInvalidTarget)
            {
                _cueBallLine.Hide();
                return;
            }


            _cueBallLine.SetPoints(
                data.CueBallPosition,
                data.CueBallPosition + data.CueBallDirection * _cueBallLineLength);
            _cueBallLine.Show();
        }

        private void UpdateCueBallGhost(TrajectoryManager.OutputData data)
        {
            if (_cueBallGhost != null)
                _cueBallGhost.transform.position = data.CueBallPosition;

            if (_invalidGhost != null)
                _invalidGhost.transform.position = data.CueBallPosition;
        }

        private void UpdateObjectBallLine(TrajectoryManager.OutputData data)
        {
            if (!data.ObjectBallHit)
            {
                _objectBallLine.Hide();
                return;
            }
            
            bool objectHit = data.ObjectBallHit;
            bool hasValidator = _targetValidator != null;

            bool isInvalidTarget = false;
            if (objectHit && hasValidator)
            {
                isInvalidTarget = !_targetValidator.IsValidTarget(data.ObjectBallIdentifier);
            }

            if (isInvalidTarget)
            {
                _objectBallLine.Hide();
                return;
            }


            _objectBallLine.Show();

            var t = Mathf.InverseLerp(
                _strikeVelocityCurrent.MinStrikeVelocity,
                _strikeVelocityCurrent.MaxStrikeVelocity,
                _strikeVelocityCurrent.CurrentStrikeVelocity);

            var objectBallAimLineLength = Mathf.Lerp(
                _minObjectBallAimLineLength,
                _maxObjectBallAimLineLength,
                t);

            _objectBallLine.SetColorNoAlphaChange(_colorList.GetColor(data.ObjectBallIdentifier));
            _objectBallLine.SetPoints(
                data.ObjectBallPosition,
                data.ObjectBallPosition + data.ObjectBallDirection * objectBallAimLineLength);
        }

        private void UpdateCueBallAimLine(TrajectoryManager.OutputData data)
        {
            _cueBallAimLine.SetPoints(data.InitialCueBallPosition, data.CueBallPosition);
        }

        private void UpdateInvalidTargetIndicator(TrajectoryManager.OutputData data)
        {
            bool objectHit = data.ObjectBallHit;
            bool hasValidator = _targetValidator != null;

            bool isInvalidTarget = false;
            if (objectHit && hasValidator)
            {
                isInvalidTarget = !_targetValidator.IsValidTarget(data.ObjectBallIdentifier);
            }

            if (_invalidTargetSprite != null)
            {
                if (objectHit && isInvalidTarget)
                {
                    _invalidTargetSprite.transform.position = data.CueBallPosition;
                    _invalidTargetSprite.gameObject.SetActive(true);
                }
                else
                {
                    _invalidTargetSprite.gameObject.SetActive(false);
                }
            }

            if (_cueBallGhost != null)
                _cueBallGhost.gameObject.SetActive(!isInvalidTarget && _isVisible);

            if (_invalidGhost != null)
                _invalidGhost.gameObject.SetActive(isInvalidTarget && _isVisible);
        }
    }
}
