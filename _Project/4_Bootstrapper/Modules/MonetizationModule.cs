// Assets/_Project/4_Bootstrapper/Modules/MonetizationModule.cs
using Billiards.CoreDomain.Monetization;
using Billiards.Infrastructure;
using Billiards.Infrastructure.Monetization;
using VContainer;

namespace Billiards.Bootstrapper
{
    public class MonetizationModule
    {
        public static void Register(IContainerBuilder builder)
        {
            // 1. Register the Infrastructure (The Unity connection)
            builder.Register<Billiards.Infrastructure.UnityIAPWrapper>(Lifetime.Singleton).As<IStorePlatform>();

            builder.Register<StoreCache>(Lifetime.Singleton);

            // The logic to fill that memory (Not using PlayFabStoreSyncService anymore)
            // Register PlayFab Validator (Assuming you have a script for this)
            builder.Register<Billiards.Infrastructure.Monetization.PlayFabReceiptValidator>(Lifetime.Singleton).As<IReceiptValidator>();

            // 2. Register the Core Domain (The Brain)
            builder.Register<StoreOrchestrator>(Lifetime.Singleton).As<IStoreOrchestrator>();

            // Use CBS for all Store data
            builder.Register<CbsStoreService>(Lifetime.Singleton)
                   .As<IStoreDataSource>()
                   .As<Billiards.CoreDomain.Assets.IStoreAssetProvider>()
                   .AsSelf();

            // Register the Player Inventory wrapper
            builder.Register<Billiards.Infrastructure.Progression.CbsPlayerInventoryService>(Lifetime.Singleton)
                   .As<Billiards.CoreDomain.Progression.IPlayerInventoryService>();
        }
    }
}