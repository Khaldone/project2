using System;
using System.Collections.Generic;
using ibc.objects;
using Unity.Mathematics;
using UnityEngine;

namespace ibc.highlight
{
    public class HighlightBallsManager : MonoBehaviour
    {
        [SerializeField] private Billiard _billiard;
        [SerializeField] private ObjectPool _objectPool;

        private List<(int, GameObject)> _activeInstances;

        private void Awake()
        {
            _activeInstances = new List<(int, GameObject)>(8);
        }

        /// <summary>
        /// Highlight the provided ball identifiers.
        /// Stops any existing highlights first.
        /// Only highlights balls that are in Normal state.
        /// </summary>
        public void HighlightBalls(List<int> identifiers)
        {
            StopHighlighting();

            if (identifiers == null || identifiers.Count == 0)
                return;

            foreach (var identifier in identifiers)
            {
                if (_billiard.State.TryGetPhysicsBall(identifier, out var ball) && ball.State == Ball.StateType.Normal)
                {
                    HighlightBall(identifier);
                }
            }
        }

        /// <summary>
        /// Highlight a single ball by identifier.
        /// </summary>
        public void HighlightBall(int identifier)
        {
            var obj = _objectPool.GetPooledObject();
            if (!_billiard.State.TryGetPhysicsBall(identifier, out var ball))
            {
                // Return object to pool if no ball found
                obj.SetActive(false);
                return;
            }

            // Only highlight if ball is in normal state
            if (ball.State != Ball.StateType.Normal)
            {
                obj.SetActive(false);
                return;
            }

            obj.transform.position = (float3)ball.Position;
            obj.transform.localScale = Vector3.one * (2f * (float)ball.Radius);
            var animator = obj.GetComponent<Animator>();
            obj.SetActive(true);
            _activeInstances.Add((identifier, obj));
            if (animator)
            {
                animator.Rebind();
                animator.Update(0f);
            }
        }

        /// <summary>
        /// Stop highlighting a specific ball.
        /// </summary>
        public void StopHighlight(int identifier)
        {
            for (var i = _activeInstances.Count - 1; i >= 0; i--)
            {
                var activeInstance = _activeInstances[i];
                if (activeInstance.Item1 == identifier)
                {
                    activeInstance.Item2.SetActive(false);
                    _activeInstances.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Stop all highlighting and clear internal state.
        /// </summary>
        public void StopHighlighting()
        {
            foreach (var activeInstance in _activeInstances)
            {
                if (activeInstance.Item2 != null)
                    activeInstance.Item2.SetActive(false);
            }

            _activeInstances.Clear();
        }
    }
}
