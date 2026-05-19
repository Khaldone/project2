using Billiards.Presentation;
using Billiards.Presentation.MainMenu;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Billiards.Presentation.Shop;
using System.Collections.Generic;

public class IAPShopLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 1. Find the IAP_Screen in this specific additive scene
        var shopMenu = FindComponentInRoot<IAP_Screen>();

        if (shopMenu != null)
        {
            // Register the View so the Presenter can receive it via constructor injection
            builder.RegisterComponent(shopMenu);
        }

        // 2. Register the Presenter as a Scoped dependency
        // This must happen here in Configure, not in Start.
        builder.Register<IAPShopPresenter>(Lifetime.Scoped).AsImplementedInterfaces().AsSelf();
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
            // PHASE A: REGISTER THE IAP SCREEN WITH THE ROUTER
            // =========================================================
            IAP_Screen shopMenu = null;
            foreach (var root in rootObjects)
            {
                shopMenu = root.GetComponentInChildren<IAP_Screen>(true);
                if (shopMenu != null) break;
            }

            if (shopMenu != null)
            {
                sharedRouter.RegisterMenu(shopMenu);
                Debug.Log($"[DI_SUCCESS] IAP_Screen registered! Router Count: {sharedRouter.MenuCount}");
            }
            else
            {
                Debug.LogError("[DI_ERROR] Could not find IAP_Screen anywhere in this scene.");
            }

            // =========================================================
            // PHASE B: INJECT ROUTER INTO NAVIGATION HANDLER
            // =========================================================
            foreach (var root in rootObjects)
            {
                var navHandlers = root.GetComponentsInChildren<IAP_NavHandler>(true);
                foreach (var handler in navHandlers)
                {
                    handler.InitializeRouter(sharedRouter);
                }
            }
        }
        else
        {
            Debug.LogError("[DI_CRITICAL] IAPShopLifetimeScope failed parent handshake.");
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