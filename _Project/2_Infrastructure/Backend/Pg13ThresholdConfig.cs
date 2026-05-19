// Assets/_Project/2_Infrastructure/Backend/Pg13ThresholdConfig.cs
using System;
using Billiards.CoreDomain.Services;
using UnityEngine;

namespace Billiards.Infrastructure.Backend
{
    /// <summary>
    /// Inspector-facing PG13 threshold configuration. Lives in Infrastructure so it
    /// can implement <see cref="ISerializationCallbackReceiver"/> for default seeding —
    /// Unity reflects [Serializable] class instances into existence without running
    /// C# field initialisers, so without this hook every threshold would deserialise
    /// to zero and incorrectly reject every image (sexual/minors is zero-tolerance).
    ///
    /// Converts to the CoreDomain <see cref="Pg13Thresholds"/> struct via
    /// <see cref="ToCoreDomain"/> when the bootstrap registers the moderation service.
    /// </summary>
    [Serializable]
    public class Pg13ThresholdConfig : ISerializationCallbackReceiver
    {
        [Tooltip("Sexual content. Default 0.09.")]
        [Range(0f, 1f)] public float sexualThreshold;

        [Tooltip("Sexual content involving minors. ZERO TOLERANCE — keep at 0.")]
        [Range(0f, 1f)] public float sexualMinorsThreshold;

        [Tooltip("Violence (general). Default 0.25.")]
        [Range(0f, 1f)] public float violenceThreshold;

        [Tooltip("Graphic violence (gore, mutilation). Default 0.10.")]
        [Range(0f, 1f)] public float violenceGraphicThreshold;

        [Tooltip("Harassment. Default 0.25.")]
        [Range(0f, 1f)] public float harassmentThreshold;

        [Tooltip("Hate speech / imagery. Default 0.25.")]
        [Range(0f, 1f)] public float hateThreshold;

        [Tooltip("Threatening hate content. Default 0.15.")]
        [Range(0f, 1f)] public float hateThreateningThreshold;

        [Tooltip("Self-harm. Default 0.02 (very strict — even logos can trip this).")]
        [Range(0f, 1f)] public float selfHarmThreshold;

        [SerializeField, HideInInspector] private bool _serialized;

        public void OnBeforeSerialize()
        {
            if (_serialized) return;
            _serialized = true;
            sexualThreshold          = Pg13ImagePolicy.SexualThreshold;
            sexualMinorsThreshold    = Pg13ImagePolicy.SexualMinorsThreshold;
            violenceThreshold        = Pg13ImagePolicy.ViolenceThreshold;
            violenceGraphicThreshold = Pg13ImagePolicy.ViolenceGraphicThreshold;
            harassmentThreshold      = Pg13ImagePolicy.HarassmentThreshold;
            hateThreshold            = Pg13ImagePolicy.HateThreshold;
            hateThreateningThreshold = Pg13ImagePolicy.HateThreateningThreshold;
            selfHarmThreshold        = Pg13ImagePolicy.SelfHarmThreshold;
        }

        public void OnAfterDeserialize() { }

        /// <summary>
        /// Snapshots the inspector-edited values into the pure CoreDomain struct
        /// that the policy consumes.
        /// </summary>
        public Pg13Thresholds ToCoreDomain() => new Pg13Thresholds(
            sexual:          sexualThreshold,
            sexualMinors:    sexualMinorsThreshold,
            violence:        violenceThreshold,
            violenceGraphic: violenceGraphicThreshold,
            harassment:      harassmentThreshold,
            hate:            hateThreshold,
            hateThreatening: hateThreateningThreshold,
            selfHarm:        selfHarmThreshold);
    }
}