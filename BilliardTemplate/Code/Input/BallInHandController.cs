using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using ibc.objects;
using ibc.unity;
using Plane = UnityEngine.Plane;

namespace ibc.game
{
    /// <summary>
    /// Handles cue ball repositioning during ball-in-hand situations
    /// </summary>
    public class BallInHandController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [Header("References")] 
        [SerializeField] private Billiard _billiard;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private UnityAabb _playingArea;
        [SerializeField] private UnityAabb _playingAreaBreak;

        [Header("Settings")] 
        [SerializeField] private float _collisionBuffer = 0.005f;
        [SerializeField] private int _ringCount = 10;
        [SerializeField] private float _ringStep = 0.05f;
        [SerializeField] private int _ringSampleCountMin = 10;
        [SerializeField] private int _ringSampleCountMax = 100;
        
        [Header("Visual Feedback")] 
        [SerializeField] private bool _invertFeedback = true;
        [SerializeField] private GameObject _dragIndicator;

        private bool _useBreakArea;
        private UnityAabb ActiveArea => (_useBreakArea && _playingAreaBreak != null) ? _playingAreaBreak : _playingArea;

        private bool _isEnabled;
        private bool _isDragging;
        private Ball _cueBall;
        private List<Ball> _otherBalls;

        private Vector3 _dragOffsetWorld;

        public event Action<Vector3> OnDragBegin;
        public event Action<Vector3> OnDragEnd;

        public bool IsEnabled => _isEnabled;
        public bool IsDragging => _isDragging;

        private void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            Disable();
            _otherBalls = new List<Ball>();
        }

        public void Enable()
        {
            if (_billiard.State.TryGetPhysicsBall(_billiard.CueBallIdentifier, out _cueBall))
            {
                transform.position = new Vector3((float)_cueBall.Position.x, transform.position.y,
                    (float)_cueBall.Position.z);
            }

            _isEnabled = true;
            gameObject.SetActive(true);
            HideAreaVisual();

            if (_dragIndicator)
                _dragIndicator.SetActive(!_invertFeedback);

            CacheOtherBallPositions();
        }

        public void Disable()
        {
            _isEnabled = false;
            _isDragging = false;

            if (_dragIndicator)
                _dragIndicator.SetActive(_invertFeedback);

            HideAreaVisual();
            gameObject.SetActive(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isEnabled) return;

            _isDragging = true;

            Vector3 mouseWorld = ScreenToWorldPosition(eventData.position);
            Vector3 ballWorld =
                new Vector3((float)_cueBall.Position.x, transform.position.y, (float)_cueBall.Position.z);
            _dragOffsetWorld = ballWorld - mouseWorld;

            ShowAreaVisual();

            OnDragBegin?.Invoke(ballWorld);

            if (_dragIndicator)
                _dragIndicator.SetActive(_invertFeedback);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isEnabled) return;

            Vector3 mouseWorld = ScreenToWorldPosition(eventData.position);
            Vector3 desired = mouseWorld + _dragOffsetWorld;

            UpdateCueBallPosition(desired);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isEnabled) return;

            _isDragging = false;
            HideAreaVisual();

            Vector3 finalPos =
                new Vector3((float)_cueBall.Position.x, transform.position.y, (float)_cueBall.Position.z);
            OnDragEnd?.Invoke(finalPos);

            if (_dragIndicator)
                _dragIndicator.SetActive(!_invertFeedback);
        }

        private void UpdateCueBallPosition(Vector3 desiredWorldPosition)
        {
            Vector3 lastValid =
                new Vector3((float)_cueBall.Position.x, transform.position.y, (float)_cueBall.Position.z);
            Vector3 candidate = GetValidatedPosition(desiredWorldPosition);

            if (IsInsidePlayingArea(candidate) && !OverlapsAnyBall(candidate) && !OverlapsAnyHole(candidate))
            {
                UpdateCueBallWorldPosition(candidate);
            }
            else
            {
                UpdateCueBallWorldPosition(lastValid);
            }
        }

        private Vector3 ScreenToWorldPosition(Vector2 screenPos)
        {
            Ray ray = _mainCamera.ScreenPointToRay(screenPos);
            Plane tablePlane = new Plane(Vector3.up, 0);

            if (tablePlane.Raycast(ray, out float distance))
            {
                Vector3 p = ray.GetPoint(distance);
                p.y = transform.position.y;
                return p;
            }

            return transform.position;
        }

        private Vector3 GetValidatedPosition(Vector3 desiredPosition)
        {
            Vector3 validatedPosition = desiredPosition;

            validatedPosition = ConstrainToPlayingArea(validatedPosition);
            validatedPosition = ResolveBallCollisions(validatedPosition);
            validatedPosition = ResolveHoleOverlaps(validatedPosition);

            
            if (!IsInsidePlayingArea(validatedPosition))
            {
                validatedPosition = ClampToPlayingArea(validatedPosition);
            }

            return validatedPosition;
        }

        public float3 FindValidPosition(float3 targetPosition, Ball cueBall)
        {
            _cueBall = cueBall;
            CacheOtherBallPositions();

            Vector3 target = targetPosition;
            
            if (IsValidPosition(target))
            {
                return targetPosition;
            }

            for (int ring = 1; ring <= _ringCount; ring++)
            {
                float radius = ring * ((float)cueBall.Radius * Mathf.Lerp(1, 3, (float)ring / _ringCount));
                int pointCount = (int)Mathf.Lerp(_ringSampleCountMin, _ringSampleCountMax, (float)ring / _ringCount);
                float dTheta = 2 * Mathf.PI / pointCount;
                
                for (int i = 0; i < pointCount; i++)
                {
                    float angle = dTheta * i;
                    float3 offset = new float3(math.cos(angle), 0f, math.sin(angle)) * radius;
                    Vector3 candidatePosition = target + (Vector3)offset;
                    
                    if (IsValidPosition(candidatePosition))
                    {
                        Debug.Log($"Found valid position at distance {radius} from target");
                        return candidatePosition;
                    }
                }
            }
            
            Debug.LogWarning("Could not find perfectly valid cue ball position, using best attempt");
            return GetValidatedPosition(target);
        }

        private void ShowAreaVisual()
        {
            var area = ActiveArea;
            if (area != null && area.ShowQuad) area.Show();
        }

        private void HideAreaVisual()
        {
            if (_playingArea) _playingArea.Hide();
            if (_playingAreaBreak) _playingAreaBreak.Hide();
        }

        public void SetBreakPhase(bool isBreakPhase)
        {
            _useBreakArea = isBreakPhase;
        }

        private Vector3 ResolveBallCollisions(Vector3 position)
        {
            float3 currentPos = position;
            float combinedRadius = (float)_cueBall.Radius * 2f + _collisionBuffer;

            foreach (var ball in _otherBalls)
            {
                if (ball.State != Ball.StateType.Normal)
                    continue;

                float3 ballPos = (float3)ball.Position;
                float3 delta = currentPos - ballPos;
                float distance = math.length(delta);

                if (distance < combinedRadius && distance > 0.0001f)
                {
                    float3 direction = delta / math.max(distance, 0.0001f);
                    float overlap = combinedRadius - distance;
                    currentPos += direction * overlap;
                }
            }

            return currentPos;
        }

        private Vector3 ResolveHoleOverlaps(Vector3 position)
        {
            float3 currentPos = position;
            var scene = _billiard.State.GetPhysicsScene();

            foreach (Hole hole in scene.Holes)
            {
                float holeR = (float)hole.Radius;
                float3 holePos = (float3)hole.Position;

                float2 deltaXZ = new float2(currentPos.x - holePos.x, currentPos.z - holePos.z);
                float dist = math.length(deltaXZ);

                float minDist = holeR + (float)_cueBall.Radius + _collisionBuffer;

                if (dist < minDist && dist > 0.0001f)
                {
                    float2 dir = deltaXZ / dist;
                    float push = (minDist - dist);

                    currentPos.x += dir.x * push;
                    currentPos.z += dir.y * push;
                }
            }

            return currentPos;
        }

        private bool IsValidPosition(Vector3 position)
        {
            return IsInsidePlayingArea(position) && 
                   !OverlapsAnyBall(position) && 
                   !OverlapsAnyHole(position);
        }

        private bool IsInsidePlayingArea(Vector3 position)
        {
            var area = ActiveArea;
            if (!area) return true;

            Bounds b = area.GetWorldBounds();
            float inset = (float)_cueBall.Radius;
            b.Expand(new Vector3(-2f * inset, 0f, -2f * inset));

            return position.x >= b.min.x && position.x <= b.max.x &&
                   position.z >= b.min.z && position.z <= b.max.z;
        }

        private Vector3 ClampToPlayingArea(Vector3 position)
        {
            var area = ActiveArea;
            if (!area) return position;

            Bounds b = area.GetWorldBounds();
            float inset = (float)_cueBall.Radius;
            b.Expand(new Vector3(-2f * inset, 0f, -2f * inset));

            float x = Mathf.Clamp(position.x, b.min.x, b.max.x);
            float z = Mathf.Clamp(position.z, b.min.z, b.max.z);
            return new Vector3(x, position.y, z);
        }

        private Vector3 ConstrainToPlayingArea(Vector3 position)
        {
            return IsInsidePlayingArea(position) ? position : ClampToPlayingArea(position);
        }

        private bool OverlapsAnyBall(Vector3 position)
        {
            float minCenterDist = (float)_cueBall.Radius * 2f + _collisionBuffer;

            foreach (var ball in _otherBalls)
            {
                if (ball.State != Ball.StateType.Normal) continue;
                float3 ballPos = (float3)ball.Position;
                if (math.distancesq((float3)position, ballPos) < minCenterDist * minCenterDist)
                    return true;
            }

            return false;
        }

        private bool OverlapsAnyHole(Vector3 position)
        {
            var scene = _billiard.State.GetPhysicsScene();

            foreach (Hole hole in scene.Holes)
            {
                float minDist = (float)hole.Radius + (float)_cueBall.Radius + _collisionBuffer;
                float3 holePos = (float3)hole.Position;
                float2 deltaXZ = new float2(position.x - holePos.x, position.z - holePos.z);
                if (math.lengthsq(deltaXZ) < minDist * minDist)
                    return true;
            }

            return false;
        }

        private void UpdateCueBallWorldPosition(Vector3 worldPosition)
        {
            _cueBall.Position = (float3)worldPosition;
            _billiard.State.SetPhysicsBall(_cueBall);
            transform.position = worldPosition;
        }

        private void CacheOtherBallPositions()
        {
            _otherBalls.Clear();
            foreach (var ball in _billiard.State.PhysicsBalls)
            {
                if (ball.Identifier != _billiard.CueBallIdentifier)
                    _otherBalls.Add(ball);
            }
        }
    }
}