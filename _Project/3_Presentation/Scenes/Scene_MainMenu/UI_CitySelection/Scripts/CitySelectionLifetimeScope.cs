using Billiards.Presentation.Shop;
using Billiards.Presentation.UI;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using Billiards.Presentation.CitySelection;
namespace Billiards.Presentation
{
    public class CitySelectionLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // 1. Find the Cue_Shop in this specific additive scene
            var cityMenu = FindComponentInRoot<CitySelectionScreen>();

            if (cityMenu != null)
            {
                // Register the View so the Presenter can receive it via constructor injection
                builder.RegisterComponent(cityMenu);
            }

            // 2. Find and register the CitySnapScrollView (required by CitySelectionPresenter)
            var scrollSnap = FindComponentInRoot<CitySnapScrollView>();
            if (scrollSnap != null)
            {
                builder.RegisterComponent(scrollSnap);
            }

            // 3. Register the Presenter as a Scoped dependency
            // This must happen here in Configure, not in Start.
            builder.Register<CitySelectionPresenter>(Lifetime.Scoped).AsImplementedInterfaces().AsSelf();
        }

        private void Start()
        {
            // 3. Ensure the parent (Bootstrap) is successfully linked and built
            if (this.Parent != null && this.Parent.Container != null)
            {
                // 4. Resolve the SINGLE shared router from the Bootstrap Parent
                var sharedRouter = this.Parent.Container.Resolve<MainMenuRouter>();

                // 5. Get root objects to perform the manual DI Handshake
                var rootObjects = this.gameObject.scene.GetRootGameObjects();

                // =========================================================
                // PHASE A: REGISTER THE Cue SCREEN WITH THE ROUTER
                // =========================================================
                CitySelectionScreen cityMenu = null;
                foreach (var root in rootObjects)
                {
                    cityMenu = root.GetComponentInChildren<CitySelectionScreen>(true);
                    if (cityMenu != null) break;
                }

                if (cityMenu != null)
                {
                    sharedRouter.RegisterMenu(cityMenu);
                    Debug.Log($"[DI_SUCCESS] Cue_Shop registered! Router Count: {sharedRouter.MenuCount}");
                }
                else
                {
                    Debug.LogError("[DI_ERROR] Could not find Cue_Shop anywhere in this scene.");
                }

                // =========================================================
                // PHASE B: INJECT ROUTER INTO NAVIGATION HANDLER
                // =========================================================
                foreach (var root in rootObjects)
                {
                    var navHandlers = root.GetComponentsInChildren<CitySelection_NavHandler>(true);
                    foreach (var handler in navHandlers)
                    {
                        handler.InitializeRouter(sharedRouter);
                    }
                }
            }
            else
            {
                Debug.LogError("[DI_CRITICAL] CueShopLifetimeScope failed parent handshake.");
            }
        }

        /// <summary>
        /// Helper to find a component within the root objects of this specific scene.
        /// Used during Configure since we can't use Resolve yet.
        /// </summary>
        private T FindComponentInRoot<T>() where T : Component
        {
            var rootObjects = this.gameObject.scene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null) return component;
            }
            return null;
        }
    }
}
