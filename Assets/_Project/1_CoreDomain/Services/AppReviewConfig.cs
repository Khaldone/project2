// Assets/_Project/1_CoreDomain/Services/AppReviewConfig.cs
namespace Billiards.CoreDomain.Services
{
    /// <summary>
    /// Configuration data containing rules, links, and parameters for triggering app reviews.
    /// </summary>
    public class AppReviewConfig
    {
        public bool UseNativeAndroidReviewPopUp { get; }
        public bool UseNativeIosReviewPopUp { get; }
        public string LinkToTheGameAndroid { get; }
        public string LinkToTheGameIOS { get; }
        public int LaunchCountsBeforeShowingPopup { get; }
        public bool ShowCustomPopupFirst { get; }

        /// <summary>
        /// Ratings strictly below this value are considered negative and show
        /// the LowRatingFeedbackMessage instead of redirecting to the store.
        /// Default is 4 (i.e., 1–3 stars = negative, 4–5 stars = positive).
        /// </summary>
        public int LowRatingThreshold { get; }

        /// <summary>
        /// The message shown inside the in-app notification when the player
        /// leaves a low rating. Displayed instantly with no slide animation.
        /// </summary>
        public string LowRatingFeedbackMessage { get; }

        public AppReviewConfig(
            bool useNativeAndroidReviewPopUp,
            bool useNativeIosReviewPopUp,
            string linkToTheGameAndroid,
            string linkToTheGameIOS,
            int launchCountsBeforeShowingPopup,
            bool showCustomPopupFirst,
            int lowRatingThreshold,
            string lowRatingFeedbackMessage)
        {
            UseNativeAndroidReviewPopUp = useNativeAndroidReviewPopUp;
            UseNativeIosReviewPopUp = useNativeIosReviewPopUp;
            LinkToTheGameAndroid = linkToTheGameAndroid;
            LinkToTheGameIOS = linkToTheGameIOS;
            LaunchCountsBeforeShowingPopup = launchCountsBeforeShowingPopup;
            ShowCustomPopupFirst = showCustomPopupFirst;
            LowRatingThreshold = lowRatingThreshold;
            LowRatingFeedbackMessage = lowRatingFeedbackMessage;
        }
    }
}
