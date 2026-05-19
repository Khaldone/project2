// Assets/_Project/1_CoreDomain/Services/ContentModerationResult.cs
namespace Billiards.CoreDomain.Services
{
    /// <summary>
    /// Outcome of a moderation check. Three disjoint cases: Allowed, Rejected, Error.
    /// Error is distinct from Rejected so the UI can degrade gracefully (e.g. retry
    /// vs. ask the user for a different image).
    ///
    /// <see cref="Scores"/> carries the raw per-category response from the provider so
    /// callers can log a full breakdown when tuning thresholds. Populated on Allowed
    /// and Rejected; default (all zeros) on Error.
    /// </summary>
    public readonly struct ContentModerationResult
    {
        public readonly bool IsAllowed;
        public readonly bool DidError;
        public readonly string Reason;
        public readonly string OffendingCategory;   // null on Allowed/Error
        public readonly string HighestCategory;     // dominant category by score (null on Error)
        public readonly float HighestScore;
        public readonly ImageCategoryScores Scores; // per-category raw scores from the provider

        private ContentModerationResult(bool isAllowed, bool didError, string reason, string offendingCategory, string highestCategory, float highestScore, ImageCategoryScores scores)
        {
            IsAllowed = isAllowed;
            DidError = didError;
            Reason = reason;
            OffendingCategory = offendingCategory;
            HighestCategory = highestCategory;
            HighestScore = highestScore;
            Scores = scores;
        }

        public static ContentModerationResult Allowed(string highestCategory, float highestScore, ImageCategoryScores scores)
            => new ContentModerationResult(true, false, null, null, highestCategory, highestScore, scores);

        public static ContentModerationResult Rejected(string offendingCategory, float offendingScore, string highestCategory, float highestScore, ImageCategoryScores scores)
            => new ContentModerationResult(false, false, $"Image flagged: {offendingCategory} score {offendingScore:0.000} exceeds PG13 threshold.", offendingCategory, highestCategory, highestScore, scores);

        public static ContentModerationResult Error(string reason)
            => new ContentModerationResult(false, true, reason, null, null, 0f, default);
    }
}