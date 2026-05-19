// Assets/_Project/3_Presentation/TrophyRoad/Scopes/TrophyRoadLifetimeScope.cs
using Billiards.Core.Progression;
using Billiards.Infrastructure.Backend;
using Billiards.Presentation.TrophyRoad;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Billiards.Presentation.TrophyRoad.Scopes
{
    public sealed class TrophyRoadLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // 1. Locate the View script within this specific additive scene layer
            var viewInstance = FindComponentInRoot<TrophyRoadScreen>();

            if (viewInstance != null)
            {
                // Register the View to resolve via constructor injection across the presenter boundary
                builder.RegisterComponent<ITrophyRoadView>(viewInstance);
            }
            else
            {
                UnityEngine.Debug.LogError("[TrophyRoadLifetimeScope] Critical Error: TrophyRoadScreen missing from hierarchy.");
            }

            // 2. Register the Presenter as a unique Scoped execution lifecycle element
            builder.Register<TrophyRoadPresenter>(Lifetime.Scoped);
            // Trophy Road: Bind the Infrastructure impl against the CoreDomain contract
            builder.Register<CbsTrophyRoadOrchestrator>(Lifetime.Scoped).As<ITrophyRoadOrchestrator>();
        }

        private void Start()
        {
            // 3. Confirm that parent container links exist before initializing the context
            if (this.Parent != null && this.Parent.Container != null)
            {
                // 4. Resolve core global systems from our root bootstrap architecture
                var globalRouter = this.Parent.Container.Resolve<MainMenuRouter>();

                // 5. Instantiate and initialize our presentation layer lifecycle
                var view = Container.Resolve<ITrophyRoadView>();
                var presenter = Container.Resolve<TrophyRoadPresenter>();

                // Wires the logic up and starts data synchronization routines automatically
                presenter.Start();

                // 6. Safely register this screen with your global navigation system
                if (view is TrophyRoadScreen screenInstance)
                {
                    globalRouter.RegisterMenu(screenInstance);
                    UnityEngine.Debug.Log($"[TrophyRoadScope] Handshake Complete. Registered screen with Router.");
                }

                // 7. Inject Router into Navigation Handlers
                var rootObjects = this.gameObject.scene.GetRootGameObjects();
                foreach (var root in rootObjects)
                {
                    var navHandlers = root.GetComponentsInChildren<TrophyRoad_NavHandler>(true);
                    foreach (var handler in navHandlers)
                    {
                        handler.InitializeRouter(globalRouter);
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogError("[TrophyRoadLifetimeScope] VContainer context resolution failed: Parent reference is Null.");
            }
        }

        private T FindComponentInRoot<T>() where T : Component
        {
            var rootObjects = this.gameObject.scene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                var comp = root.GetComponentInChildren<T>(true);
                if (comp != null) return comp;
            }
            return null;
        }
    }
}