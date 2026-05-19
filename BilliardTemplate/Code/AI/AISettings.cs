using System.Collections.Generic;
using ibc.objects;
using UnityEngine;
using UnityEngine.Serialization;

namespace ibc.ai
{
    
    public enum SideToPlay
    {
        Solid,
        Stripe,
    }
    
    [CreateAssetMenu(fileName = "AI Settings", menuName = "Billiard/AI Settings", order = 1)]
    public class AISettings : ScriptableObject
    {
        [Header("Data")] 
        [FormerlySerializedAs("cue")]
        public Cue Cue  ;

        [Header("Identifiers")]
        [FormerlySerializedAs("cueBallIdentifier")] public int CueBallIdentifier;
        [FormerlySerializedAs("blackBallIdentifier")] public int BlackBallIdentifier = 8;
        [FormerlySerializedAs("solidIdentifiers")] public List<int> SolidIdentifiers = new() {1, 2, 3, 4, 5, 6, 7};
        [FormerlySerializedAs("stripeIdentifiers")] public List<int> StripeIdentifiers = new() {9, 10, 11, 12, 13, 14, 15};

        [Header("Game")]
        [FormerlySerializedAs("sideToPlay")]
        public SideToPlay SideToPlay = 0;
    }
}