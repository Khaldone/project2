using Billiards.Presentation.MainMenu;
using UnityEngine;
using VContainer;
using static MainMenuRouter;

namespace Billiards.Presentation
{
    public class IAP_NavHandler : MonoBehaviour
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

        public void GoTo1stShop()
        {
            if (_router == null)
            {
                Debug.LogError("[NavHandler] Router is null! Transition failed.");
                return;
            }

            _router.TransitionTo<Cue_ShopScreen>(ShowStyle.FromLeft, HideStyle.ToRight);
        }
        public void GoTo2ndShop()
        {
            if (_router == null)
            {
                Debug.LogError("[NavHandler] Router is null! Transition failed.");
                return;
            }

            _router.TransitionTo<CoinShopScreen>(ShowStyle.FromLeft, HideStyle.ToRight);
        }
        
    }


}
