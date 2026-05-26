using AiToolbox;
using Billiards.Bootstrapper;
using Billiards.Core.Analytics;
using Billiards.CoreDomain.Hardware;
using Billiards.CoreDomain.Monetization;
using Billiards.CoreDomain.Progression;
using Billiards.CoreDomain.Services;
using Billiards.CoreDomain.Telemetry;
using Billiards.Infrastructure.Assets;
using Billiards.Infrastructure.Authentication;
using Billiards.Infrastructure.Authentication.NativeAuth;
using Billiards.Infrastructure.Backend;
using Billiards.Infrastructure.Progression;
using Billiards.Infrastructure.Telemetry;
using Billiards.Presentation;
using Billiards.Presentation.Telemetry;
using Billiards.CoreDomain.Notifications;
using Billiards.Infrastructure.Notifications;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Billiards.Bootstrapper
{
    public class BilliardsGameLifetimeScope : LifetimeScope
    {
        [Header("AI Moderation (OpenAI)")]
        [Tooltip("Per-category PG13 thresholds. A category score strictly greater than its threshold rejects the image. Sexual/Minors is zero-tolerance.")]
        [SerializeField] private Pg13ThresholdConfig _moderationThresholds = new Pg13ThresholdConfig();

        [Tooltip("Hard timeout (seconds) for the OpenAI moderation request. If the call doesn't complete in time it's aborted and the upload is refused.")]
        [SerializeField] private int _moderationTimeoutSeconds = 12;

        [Tooltip("Hard timeout (seconds) for the Imgur avatar upload. If exceeded the upload is aborted and the user sees a 'timed out' message.")]
        [SerializeField] private int _avatarUploadTimeoutSeconds = 30;

        [Tooltip("DEBUG: When ON, the server moderation check runs even when the client has already rejected the image. Use this to surface the server-side per-category breakdown and verify client/server thresholds are in sync. Costs an extra OpenAI call per rejected upload — leave OFF in production.")]
        [SerializeField] private bool _forceServerCheck = false;

        [Tooltip("Set the OpenAI API key here. Used to PG13-gate user-uploaded avatars before they hit Imgur.")]
        [SerializeField] private ModerationParameters _moderationParameters;

        protected override void Configure(IContainerBuilder builder)
        {

            builder.RegisterEntryPoint<GameEntryPoint>();

            builder.Register<PlayerSession>(Lifetime.Singleton);
            // Give me the PlayFabAuthService implementation of
            // of the interrface IAuthenticationService_New
            //builder.Register<PlayFabAuthService>(Lifetime.Singleton)
            //.As<IAuthenticationService_New>();

            // Register the new CBS Auth Wrapper to the Global Scope
            builder.Register<CbsAuthServiceWrapper>(Lifetime.Singleton)
           .As<IAuthenticationService_New>();

            // Register Native Auth Services based on platform
#if UNITY_EDITOR
            builder.Register<EditorMockAuthWrapper>(Lifetime.Singleton).As<INativeAuthService>();
#elif UNITY_IOS
            builder.Register<AppleAuthWrapper>(Lifetime.Singleton).As<INativeAuthService>();
#elif UNITY_ANDROID
            builder.Register<GoogleAuthWrapper>(Lifetime.Singleton).As<INativeAuthService>();
#else
            // Fallback
            builder.Register<EditorMockAuthWrapper>(Lifetime.Singleton).As<INativeAuthService>();
#endif

            MonetizationModule.Register(builder);
            Debug.Log("[RootScope] Global Services Registered.");
            builder.Register<SecureFileSaver>(Lifetime.Singleton).As<ILocalSaveService>();
            builder.Register<CbsSaveBackend>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PlayerDataService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<IAchievementDataSource, CbsAchievementService>(Lifetime.Singleton);
            
            // Avatar Service + upload settings (timeout, force-server-check toggle, etc.)
            builder.Register<ImgurAvatarService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterInstance(new AvatarUploadSettings(_avatarUploadTimeoutSeconds, _forceServerCheck));

            // Server-authoritative moderation via PlayFab CloudScript. The OpenAI API
            // key is held server-side as Title Internal Data — clients cannot bypass.
            builder.Register<PlayFabCloudScriptModerationService>(Lifetime.Singleton).AsImplementedInterfaces();
            
            // Arena Progression Service (used by CitySelectionPresenter)
            builder.Register<CbsArenaProgressionService>(Lifetime.Singleton).As<IArenaProgressionService>();

            // Asset Delivery Service
            builder.Register<UnityAddressablesWrapper>(Lifetime.Singleton).As<IAssetDeliveryService>();

            // Gallery Service
            builder.Register<Billiards.Infrastructure.Hardware.NativeGalleryWrapper>(Lifetime.Singleton).AsImplementedInterfaces();

            // Content Moderation (PG13 gate over OpenAI omni-moderation-latest).
            // Inspector-editable Pg13ThresholdConfig → pure CoreDomain struct at registration time.
            if (_moderationParameters != null)
            {
                if (_moderationThresholds == null) _moderationThresholds = new Pg13ThresholdConfig();
                var thresholds = _moderationThresholds.ToCoreDomain();

                builder.RegisterInstance(_moderationParameters);
                builder.RegisterInstance(thresholds);
                builder.Register<IContentModerationService>(
                    resolver => new OpenAiPg13ImageModerator(
                        _moderationParameters,
                        thresholds,
                        _moderationTimeoutSeconds),
                    Lifetime.Singleton);
            }
            else
            {
                Debug.LogWarning("[RootScope] ModerationParameters not assigned on BilliardsGameLifetimeScope — avatar moderation will be unavailable.");
            }
            
            // Global Message Broker
            builder.Register<MessageBroker>(Lifetime.Singleton).As<IMessageBroker>();

            // Note: If your StoreCache also needs to be global, put it here too!
            //builder.Register<StoreCache>(Lifetime.Singleton);

            // Register Sentry telemetry system implementation as a persistent global singleton service
            builder.Register<SentryTelemetryWrapper>(Lifetime.Singleton)
                   .As<ITelemetryService>();

            // Ensure initialization execution parameters run immediately on app bootstrap sequences
            builder.RegisterBuildCallback(container =>
            {
                var telemetryService = container.Resolve<ITelemetryService>();
                telemetryService.Initialize();
            });

            // 2. Register Anti-Corruption Layer Backend Drivers
            builder.Register<ITelemetryBackend, PlayFabTelemetryBackend>(Lifetime.Singleton);
            builder.Register<ITelemetryBackend, FirebaseTelemetryBackend>(Lifetime.Singleton);

            // Platform Services Gate — coordinates Firebase init completion with GPGS auth.
            // GlobalAnalyticsTracker (producer) marks ready after backends init;
            // LoginEntryPoint (consumer) awaits before touching Google Play Services.
            builder.Register<PlatformServicesGate>(Lifetime.Singleton).As<IPlatformServicesGate>();

            // 3. Register the globally bound Passive Listeners Component
            // Injects directly as IAsyncStartable / IDisposable inside VContainer registration frames
            builder.RegisterEntryPoint<GlobalAnalyticsTracker>(Lifetime.Singleton);

            // Firebase Push Notifications (Global Scope)
            builder.Register<FirebasePushWrapper>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterEntryPoint<PushTokenSyncer>(Lifetime.Singleton);

        }
    }
}