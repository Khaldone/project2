// Assets/Scripts/Presentation/Hub_Bootstrap/BootstrapLifetimeScope.cs
using VContainer;
using VContainer.Unity;


public class BootstrapLifetimeScopeHubSpokes : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Register the Hub's own scope so the SceneLoader can use it
        builder.RegisterComponent(this);

        // Register the SceneLoader
        builder.Register<HubSceneLoader>(Lifetime.Singleton).AsImplementedInterfaces();


        // Register all global services (IAP, Save, Analytics)
        //new ProgressionModule().Register(builder);


        // Populate the Service Locator
        builder.RegisterBuildCallback(resolver =>
        {
            ServiceLocator.Clear();
            ServiceLocator.Register(resolver.Resolve<ISceneLoader>());
            ServiceLocator.Register(resolver.Resolve<IStoreOrchestrator>());
            // ...
        });
    }
}
