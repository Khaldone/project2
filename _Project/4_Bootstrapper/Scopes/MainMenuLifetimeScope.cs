using VContainer;
using VContainer.Unity;
using Billiards.Presentation.MainMenu;
using UnityEngine;
namespace Billiards.Bootstrapper.Scopes
{
    public class MainMenuLifetimeScope : LifetimeScope
    {

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<UIAddressablesLoader>(Lifetime.Scoped);

            // Register the Router so the EntryPoint and additive scenes can use it
            builder.Register<MainMenuRouter>(Lifetime.Scoped);

            builder.RegisterEntryPoint<MainMenuEntryPoint>().AsSelf();

            

        }
    }
}