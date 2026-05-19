using Billiards.Presentation;
using Billiards.Presentation.Login;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Billiards.Bootstrapper.Scopes
{
    public class LoginLifetimeScope : LifetimeScope
    {
        [SerializeField] private LoginUIHandler _loginUIHandler;
        protected override void Configure(IContainerBuilder builder)
        {
            // LOG 4: Check Parentage
            if (this.Parent == null)
            {
                Debug.LogWarning("[DI_TEST] LoginScope: Parent is NULL. Searching for Global Root...");
            }
            else
            {
                Debug.Log($"[DI_TEST] LoginScope: Parent found! Name: {this.Parent.name}");
            }

            // Register the UI Handler so the [Inject] tag works
            if (_loginUIHandler != null) builder.RegisterComponent(_loginUIHandler);
            builder.RegisterEntryPoint<LoginEntryPoint>().AsSelf();

            Debug.Log("All components registered in the Login Scene");
        }
    }
}