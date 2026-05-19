using System;
using System.Collections.Generic;
using ibc.objects;
using ibc.unity;
using Unity.Mathematics;
using UnityEngine;

namespace ibc.ai
{
    public class AIStrikeProvider : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private AISettings _settings;
        
        [Header("Strike Variations")]
        [SerializeField] private float _velocityStep = 0.1f;
        [SerializeField] private float _strikeVariationDx = 0.025f;
        [SerializeField] private float _strikePruneAngleThreshold = 1f;

        [Header("Raycast")] 
        [SerializeField] private LayerMask _ballMask;
        [SerializeField] private float _maxRaycastDistance = 5f;

        [Header("Velocity")]
        [SerializeField] private float _minVelocity = 1f;
        [SerializeField] private float _maxVelocity = 5f;


        public List<StrikeData> GetStrikes(BilliardState state)
        {
            var strikes = new List<StrikeData>();
            if (!state.TryGetPhysicsBall(_settings.CueBallIdentifier, out var cueBall))
            {
                throw new Exception("Could not find cue ball");
            }
            
            state.TryGetPhysicsBallIndex(_settings.CueBallIdentifier, out var cueBallIndex);

            Debug.Assert(_velocityStep > float.Epsilon);

            var scene = state.GetPhysicsScene();
            foreach (var ball in scene.Balls)
            {
                if (ball.Identifier == _settings.CueBallIdentifier)
                    continue;
                if (ball.State == Ball.StateType.Pocketed)
                    continue;
                
                float3 targetBallPos = (float3)ball.Position;
                float3 displ = (float3) (targetBallPos - cueBall.Position);
                var length = math.length(displ);
                float3 dir = displ / length;
                float maxAngleOffset = Mathf.Rad2Deg * Mathf.Atan2((float) (ball.Radius + cueBall.Radius), length);
                float angleOffset = Mathf.Rad2Deg * Mathf.Atan2((float) (_strikeVariationDx), length);

                float angle = Quaternion.FromToRotation(Vector3.forward, dir).eulerAngles.y;
                Debug.Assert(angleOffset > 0);

                if (angleOffset <= 0)
                    angleOffset += 0.1f;
                
                var ballHit = CheckBallHit((float3) cueBall.Position, angle, (float) cueBall.Radius, out var hitCollider);
                if (ballHit)
                {
                    var ballHitComponent = hitCollider.GetComponent<UnityBall>();
                    if (ballHitComponent == null || ballHitComponent.Identifier != ball.Identifier)
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
                
                if (strikes.FindIndex(t => Mathf.Abs(t.Orientation - angle) < _strikePruneAngleThreshold) != -1)
                    continue;
                
                for (float vel = _minVelocity; vel < _maxVelocity; vel += _velocityStep)
                {
                    var invLength = 1 / length;
                    var invLengthClamped = Mathf.Clamp(invLength, 0.1f, 10f);
                    var strikeData = new StrikeData()
                    {
                        CueBallIndex = cueBallIndex,
                        CueData = _settings.Cue,
                        Offset = float2.zero,
                        Orientation = angle,
                        Velocity = vel,
                        Priority = 1 + invLengthClamped,
                    };
                    
                    strikes.Add(strikeData);
                    
                    for (float x = angle - maxAngleOffset; x < angle + maxAngleOffset; x += angleOffset)
                    {
                        strikeData.Orientation = x;
                        strikeData.Priority = invLengthClamped;
                        strikes.Add(strikeData);   
                    }
                }
            }
            
            strikes.Sort((strike1, strike2) => strike1.Priority.CompareTo(strike2.Priority));
            return strikes;
        }

        public float InverseLerpVelocity(float vel)
        {
            return Mathf.InverseLerp(_minVelocity, _maxVelocity, vel);
        }
        
        
        private bool CheckBallHit(float3 position, float angle, float ballRadius, out Collider collider)
        {

            if (Physics.SphereCast(new Ray(position, Quaternion.Euler(0, angle, 0) * Vector3.forward), ballRadius,
                    out var hitInfo, _maxRaycastDistance, _ballMask))
            {
                collider = hitInfo.collider;
                return true;
            }

            collider = null;
            return false;
        }
    }
}