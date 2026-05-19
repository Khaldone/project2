using Billiards.Presentation;
using Billiards.Presentation.MainMenu;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Billiards.Presentation.Shop;
using System.Collections.Generic;

public class CoinShopLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 1. Find the Cue_Shop in this specific additive scene
        var coinMenu = FindComponentInRoot<CoinShopScreen>();

        if (coinMenu != null)
        {
            // Register the View so the Presenter can receive it via constructor injection
            builder.RegisterComponent(coinMenu);
        }

        // 2. Register the Presenter as a Scoped dependency
        // This must happen here in Configure, not in Start.
        //builder.Register<CueShopPresenter>(Lifetime.Scoped).AsImplementedInterfaces().AsSelf();
        builder.Register<CoinShopPresenter>(Lifetime.Scoped).AsImplementedInterfaces().AsSelf();
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
            CoinShopScreen coinMenu = null;
            foreach (var root in rootObjects)
            {
                coinMenu = root.GetComponentInChildren<CoinShopScreen>(true);
                if (coinMenu != null) break;
            }

            if (coinMenu != null)
            {
                sharedRouter.RegisterMenu(coinMenu);
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
                var navHandlers = root.GetComponentsInChildren<CoinShop_NavHandler>(true);
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