using ibc.objects;
using Unity.Mathematics;

namespace ibc.ai
{
    public struct StrikeData
    {
        public float Orientation;
        public float2 Offset;
        public int CueBallIndex;
        public float Velocity;
        public Cue CueData;

        public float Priority;
            
        public override string ToString()
        {
            return $"\n Orientation: {Orientation} Velocity: {Velocity} Offset: {Offset} CueBallIndex: {CueBallIndex} Priority: {Priority}";
        }
    }
}