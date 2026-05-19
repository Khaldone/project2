// Inside your LobbyLifetimeScope.cs
using VContainer;
using VContainer.Unity;
using UnityEngine;

public class LobbyLifetimeScope : LifetimeScope
{
    [SerializeField] FusionMatchmakingService fusionMatchmakingService;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // 1. Bind the Presenters/Humble Objects
        builder.RegisterComponent<IMatchmakingService>(fusionMatchmakingService);
        //builder.RegisterComponent<ILobbyUIView>(unityLobbyUIView);
        //builder.RegisterComponent<ISceneLoader>(unitySceneLoader);


        // 2. Bind the Core Brain
        builder.Register<LobbyController>(Lifetime.Scoped);
    }


}





