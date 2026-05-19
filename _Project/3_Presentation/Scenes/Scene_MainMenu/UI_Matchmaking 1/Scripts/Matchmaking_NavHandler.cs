using UnityEngine;
using static MainMenuRouter;

namespace Billiards.Presentation
{
    public class Matchmaking_NavHandler : MonoBehaviour
    {
        // 1. Remove the [Inject] attribute!
        private MainMenuRouter _router;

        // 2. Add a manual setup method
        public void InitializeRouter(MainMenuRouter sharedRouter)
        {
            _router = sharedRouter;
            Debug.Log($"[NavHandler] Received Router ID: {_router.GetHashCode()}");
        }

        // Your existing button methods remain the same
        public void GoToHome()
        {
            if (_router == null)
            {
                Debug.LogError("[NavHandler] Router is null! Transition failed.");
                return;
            }

            _router.TransitionTo<HomeMenu>(ShowStyle.FromLeft, HideStyle.ToRight);
        }
    }
}
