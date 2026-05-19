// Assets/_Project/1_CoreDomain/Services/Pg13ImagePolicy.cs
namespace Billiards.CoreDomain.Services
{
    /// <summary>
    /// Pure threshold-based PG13 policy. Mirrors the limits validated in the AI
    /// Toolbox moderation demo, but lets the caller swap in a custom
    /// <see cref="Pg13Thresholds"/> at runtime. Lives in CoreDomain so it can be
    /// NUnit-tested without spinning up the Editor or hitting OpenAI.
    /// </summary>
    public static class Pg13ImagePolicy
    {
        // Default thresholds, preserved for tests / fallback when no config is supplied.
        public const float SexualThreshold          = 0.09f;
        public const float SexualMinorsThreshold    = 0.00f;
        public const float ViolenceThreshold        = 0.25f;
        public const float ViolenceGraphicThreshold = 0.10f;
        public const float HarassmentThreshold      = 0.25f;
        public const float HateThreshold            = 0.25f;
        public const float HateThreateningThreshold = 0.15f;
        public const float SelfHarmThreshold        = 0.02f;

        /// <summary>
        /// Convenience overload using the built-in default thresholds.
        /// </summary>
        public static ContentModerationResult Evaluate(ImageCategoryScores scores, bool providerFlagged)
            => Evaluate(scores, providerFlagged, Pg13Thresholds.Default);

        /// <summary>
        /// Evaluates a set of category scores plus the provider's own boolean flag
        /// against the supplied tunable thresholds. First rejection wins; categories
        /// are checked in order of severity so the reported "offending category" is
        /// the most policy-relevant one. Highest-scoring category is also reported,
        /// regardless of verdict, to make threshold tuning self-documenting.
        /// </summary>
        public static ContentModerationResult Evaluate(ImageCategoryScores scores, bool providerFlagged, Pg13Thresholds thresholds)
        {
            var (highestCat, highestScore) = HighestScoreWithCategory(scores);

            if (providerFlagged)
            {
                return ContentModerationResult.Rejected("provider-flagged", highestScore, highestCat, highestScore, scores);
            }

            if (scores.SexualMinors    > thresholds.SexualMinors)    return ContentModerationResult.Rejected("sexual/minors",    scores.SexualMinors,    highestCat, highestScore, scores);
            if (scores.Sexual          > thresholds.Sexual)          return ContentModerationResult.Rejected("sexual",           scores.Sexual,          highestCat, highestScore, scores);
            if (scores.SelfHarm        > thresholds.SelfHarm)        return ContentModerationResult.Rejected("self-harm",        scores.SelfHarm,        highestCat, highestScore, scores);
            if (scores.ViolenceGraphic > thresholds.ViolenceGraphic) return ContentModerationResult.Rejected("violence/graphic", scores.ViolenceGraphic, highestCat, highestScore, scores);
            if (scores.HateThreatening > thresholds.HateThreatening) return ContentModerationResult.Rejected("hate/threatening", scores.HateThreatening, highestCat, highestScore, scores);
            if (scores.Violence        > thresholds.Violence)        return ContentModerationResult.Rejected("violence",         scores.Violence,        highestCat, highestScore, scores);
            if (scores.Hate            > thresholds.Hate)            return ContentModerationResult.Rejected("hate",             scores.Hate,            highestCat, highestScore, scores);
            if (scores.Harassment      > thresholds.Harassment)      return ContentModerationResult.Rejected("harassment",       scores.Harassment,      highestCat, highestScore, scores);

            return ContentModerationResult.Allowed(highestCat, highestScore, scores);
        }

        private static (string category, float score) HighestScoreWithCategory(ImageCategoryScores s)
        {
            string cat = "sexual";
            float h = s.Sexual;
            if (s.SexualMinors    > h) { h = s.SexualMinors;    cat = "sexual/minors"; }
            if (s.Violence        > h) { h = s.Violence;        cat = "violence"; }
            if (s.ViolenceGraphic > h) { h = s.ViolenceGraphic; cat = "violence/graphic"; }
            if (s.Harassment      > h) { h = s.Harassment;      cat = "harassment"; }
            if (s.Hate            > h) { h = s.Hate;            cat = "hate"; }
            if (s.HateThreatening > h) { h = s.HateThreatening; cat = "hate/threatening"; }
            if (s.SelfHarm        > h) { h = s.SelfHarm;        cat = "self-harm"; }
            return (cat, h);
        }
    }
}