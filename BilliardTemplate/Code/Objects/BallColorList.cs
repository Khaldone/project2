using UnityEngine;

namespace ibc.trajectory
{
    [CreateAssetMenu(fileName = "Color List", menuName = "Billiard/Color List")]
    public sealed class BallColorList : ScriptableObject
    {
        [SerializeField]
        private Color[] _colors;
        
        public Color GetColor(int ballIndex)
        {
            if(ballIndex > 8)
                return _colors[ballIndex % 8];

            return _colors[ballIndex];
        }

        
    }
}