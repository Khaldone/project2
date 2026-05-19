// Assets/_Project/1_CoreDomain/Services/ImageCategoryScores.cs
namespace Billiards.CoreDomain.Services
{
    /// <summary>
    /// Per-category confidence scores returned by an image moderation provider,
    /// normalised to the subset that the PG13 policy cares about. Pure data envelope.
    /// </summary>
    public readonly struct ImageCategoryScores
    {
        public readonly float Sexual;
        public readonly float SexualMinors;
        public readonly float Violence;
        public readonly float ViolenceGraphic;
        public readonly float Harassment;
        public readonly float Hate;
        public readonly float HateThreatening;
        public readonly float SelfHarm;

        public ImageCategoryScores(
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
    }
}
