// Assets/_Project/Scenes/UI_PlayerProfile/Scripts/ProfileLifetimeScope.cs
using Billiards.CoreDomain.Hardware;
using Billiards.CoreDomain.Services;
using Billiards.Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ProfileLifetimeScope : LifetimeScope
{
    private PlayerProfilePresenter _presenter;

    protected override void Configure(IContainerBuilder builder)
    {
        // LEAVE EMPTY. 
        // We do not want VContainer building anything prematurely here 
        // while Addressables is still setting up the scene.
    }

    private void Start()
    {
        // Ensure the parent (Bootstrap) is successfully linked and built
        if (this.Parent != null && this.Parent.Container != null)
        {
            // Grab the dependencies from the Global Bootstrap (Tier 4)
            var sharedRouter = this.Parent.Container.Resolve<MainMenuRouter>();
            var playerSession = this.Parent.Container.Resolve<PlayerSession>();
            var playerData = this.Parent.Container.Resolve<IPlayerDataServiceProgression>();
            var avatarService = this.Parent.Container.Resolve<IAvatarService>();
            var galleryService = this.Parent.Container.Resolve<IGalleryService>();
            var uploadSettings = this.Parent.Container.Resolve<AvatarUploadSettings>();

            // Optional: moderation may be unregistered if the root scope has no API key configured.
            // Falls back to null so the presenter can no-op moderation and still let the
            // existing test-image button work.
            IContentModerationService moderationService = null;
            try { moderationService = this.Parent.Container.Resolve<IContentModerationService>(); }
            catch { /* not registered — gallery upload will refuse if missing */ }

            // Server-side moderation is the authoritative gate. Always registered
            // alongside PlayFab; if it errors at runtime the presenter fails closed.
            var serverModerationService = this.Parent.Container.Resolve<IServerImageModerationService>();

            // Inspector-tuned PG13 thresholds, registered as an instance by the bootstrap.
            // The presenter uses these to render an accurate breakdown log — the policy
            // itself is already evaluating against them, this exposes them for display.
            var thresholds = this.Parent.Container.Resolve<Pg13Thresholds>();

            // Get all root objects in THIS specific Addressable scene
            var rootObjects = this.gameObject.scene.GetRootGameObjects();

            // Resolve the global notification queue from the additive UI_Popup scene scope
            global::INotificationQueue globalNotificationQueue = null;
            var scopes = Object.FindObjectsByType<VContainer.Unity.LifetimeScope>(FindObjectsSortMode.None);
            foreach (var scope in scopes)
            {
                if (scope.Container == null) continue;
                try
                {
                    globalNotificationQueue = (global::INotificationQueue)scope.Container.Resolve(typeof(global::INotificationQueue));
                    if (globalNotificationQueue != null) break;
                }
                catch { /* Ignore */ }
            }

            // =========================================================
            // PHASE A: FIND VIEW, CREATE PRESENTER, AND REGISTER
            // =========================================================
            PlayerProfileScreen playerProfileMenu = null;
            foreach (var root in rootObjects)
            {
                playerProfileMenu = root.GetComponentInChildren<PlayerProfileScreen>(true);
                if (playerProfileMenu != null) break;
            }

            if (playerProfileMenu != null)
            {
                // THE MVP DECOUPLING:
                // We manually construct the Presenter. This perfectly bridges the 
                // Tier 1 Data with the Tier 3 UI 
                // without either of them knowing about the other's framework.
                _presenter = new PlayerProfilePresenter(playerProfileMenu, playerSession, playerData, avatarService, galleryService, moderationService, serverModerationService, uploadSettings, thresholds, globalNotificationQueue);

                // Tell the presenter to map the data to the View
                _presenter.Initialize();

                // Register the dumb View to the Router so it can be animated
                sharedRouter.RegisterMenu(playerProfileMenu);

                Debug.Log($"[DI_SUCCESS] PlayerProfile dynamically registered! Router Count: {sharedRouter.MenuCount}");
            }
            else
            {
                Debug.LogError("[DI_ERROR] Could not find PlayerProfileScreen anywhere in this scene.");
            }

            // =========================================================
            // PHASE B: INJECT ROUTER INTO NAVIGATION HANDLER
            // =========================================================
            foreach (var root in rootObjects)
            {
                var navHandlers = root.GetComponentsInChildren<PlayerProfile_NavHandler>(true);
                foreach (var handler in navHandlers)
                {
                    handler.InitializeRouter(sharedRouter);
                }
            }
        }
        else
        {
            Debug.LogError("[DI_CRITICAL] ProfileLifetimeScope failed parent handshake.");
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _presenter?.Dispose();
    }
}