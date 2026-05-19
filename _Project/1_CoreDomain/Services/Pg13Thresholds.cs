// Assets/_Project/1_CoreDomain/Services/Pg13Thresholds.cs
namespace Billiards.CoreDomain.Services
{
    /// <summary>
    /// Pure-C# threshold values used by <see cref="Pg13ImagePolicy"/>. A category
    /// score strictly greater than its threshold rejects the image. Sexual/Minors
    /// is zero-tolerance.
    ///
    /// Lives in CoreDomain (no UnityEngine). The Infrastructure layer holds the
    /// inspector-facing serializable counterpart and converts to this struct.
    /// </summary>
    public readonly struct Pg13Thresholds
    {
        public readonly float Sexual;
        public readonly float SexualMinors;
        public readonly float Violence;
        public readonly float ViolenceGraphic;
        public readonly float Harassment;
        public readonly float Hate;
        public readonly float HateThreatening;
        public readonly float SelfHarm;

        public Pg13Thresholds(
            float sexual,
            float sexualMinors,
            float violence,
            float violenceGraphic,
            float harassment,
            float hate,
            float hateThreatening,
            float selfHarm)
        {
            Sexual = sexual;
            SexualMinors = sexualMinors;
            Violence = violence;
            ViolenceGraphic = violenceGraphic;
            Harassment = harassment;
            Hate = hate;
            HateThreatening = hateThreatening;
            SelfHarm = selfHarm;
        }

        /// <summary>
        /// AI-Toolbox-demo defaults. Use as the fallback when no configuration is supplied.
        /// </summary>
        public static Pg13Thresholds Default => new Pg13Thresholds(
            sexual:          Pg13ImagePolicy.SexualThreshold,
            sexualMinors:    Pg13ImagePolicy.SexualMinorsThreshold,
            violence:        Pg13ImagePolicy.ViolenceThreshold,
            violenceGraphic: Pg13ImagePolicy.ViolenceGraphicThreshold,
            harassment:      Pg13ImagePolicy.HarassmentThreshold,
            hate:            Pg13ImagePolicy.HateThreshold,
            hateThreatening: Pg13ImagePolicy.HateThreateningThreshold,
            selfHarm:        Pg13ImagePolicy.SelfHarmThreshold);
    }
}