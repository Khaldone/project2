// Assets/_Project/3_Presentation/Scene_MainMenu/Scripts/HomeLifetimeScope.cs
using Billiards.Presentation;
using Billiards.Presentation.MainMenu;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class HomeLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // LEAVE EMPTY. 
        // We do not want VContainer building anything prematurely here 
        // while Addressables is still setting up the scene.
    }

    private void Start()
    {
        // 1. Ensure the parent (Bootstrap) is successfully linked and built
        if (this.Parent != null && this.Parent.Container != null)
        {
            // 2. Grab the dependencies from the Global Bootstrap (Tier 4)
            var sharedRouter = this.Parent.Container.Resolve<MainMenuRouter>();
            var playerSession = this.Parent.Container.Resolve<PlayerSession>();
            var entryPoint = this.Parent.Container.Resolve<MainMenuEntryPoint>();

            // 3. Get all root objects in THIS specific Addressable scene
            var rootObjects = this.gameObject.scene.GetRootGameObjects();

            // =========================================================
            // PHASE A: FIND VIEW, CREATE PRESENTER, AND REGISTER
            // =========================================================
            HomeMenu homeMenu = null;
            foreach (var root in rootObjects)
            {
                // Search for the View (the dumb UI script)
                homeMenu = root.GetComponentInChildren<HomeMenu>(true);
                if (homeMenu != null) break;
            }

            if (homeMenu != null)
            {
                // THE MVP DECOUPLING:
                // We manually construct the Presenter. This perfectly bridges the 
                // Tier 1 Data (PlayerSession) with the Tier 3 UI (HomeMenu) 
                // without either of them knowing about the other's framework.
                var presenter = new HomePresenter(playerSession, homeMenu);

                // Tell the presenter to map the data to the View
                presenter.Initialize();

                // Register the dumb View to the Router so it can be animated
                sharedRouter.RegisterMenu(homeMenu);

                Debug.Log($"[DI_SUCCESS] Home dynamically registered! Router ID: {sharedRouter.GetHashCode()} | Total Count: {sharedRouter.MenuCount}");
            }
            else
            {
                Debug.LogError("[DI_ERROR] Could not find HomeMenu anywhere in this scene.");
            }

            // =========================================================
            // PHASE B: FIND NAVIGATION HANDLER(S) AND INJECT ROUTER
            // =========================================================
            bool handlerFound = false;
            foreach (var root in rootObjects)
            {
                var navHandlers = root.GetComponentsInChildren<MainMenuNavigationHandler>(true);

                foreach (var handler in navHandlers)
                {
                    handler.InitializeRouter(sharedRouter, entryPoint);
                    handlerFound = true;
                }
            }

            if (!handlerFound)
            {
                Debug.LogWarning("[DI_WARNING] Could not find any MainMenuNavigationHandler in this scene.");
            }
        }
        else
        {
            Debug.LogError("[DI_CRITICAL] HomeLifetimeScope started, but Parent or Parent.Container is null! The DI Handshake failed.");
        }
    }
}