using System;
using Unity.Mathematics;
using UnityEngine;

namespace ibc.game
{
    public class HitSpotIndicator : MonoBehaviour
    {
        [SerializeField] private CueStickController _cueStickController;
        [SerializeField] private RectTransform _hitSpotIndicator;
        [SerializeField] private float _visualRadialOffset;
        private float2 _offset;

        private void Start()
        {
            SetOffset(_cueStickController.GetOffset());
        }

        private void UpdateHitSpotIndicator()
        {
            {
                var radialSize = (_hitSpotIndicator.parent.GetComponent<RectTransform>().rect.width - _visualRadialOffset) * 0.5f;
                _hitSpotIndicator.anchoredPosition = _offset * radialSize;
            }
        }

        public void SetOffset(float2 offset)
        {
            _offset = offset;
            UpdateHitSpotIndicator();
        }
        
        public void SetOffset(Vector2 offset)
        {
            _offset = offset;
            UpdateHitSpotIndicator();
        }
    }
}