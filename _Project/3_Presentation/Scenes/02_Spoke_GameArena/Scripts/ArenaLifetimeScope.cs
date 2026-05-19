// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/ArenaLifetimeScope.cs
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Billiards.Presentation.Arena;

public class ArenaLifetimeScope : LifetimeScope
{
    [SerializeField] private ArenaVFXPoolService _vfxPool;
    [SerializeField] private BotPlayerController _botPlayerController;
    [SerializeField] private DragInputSurface _dragInputSurface;
    [SerializeField] private ArenaNavigationHandler _arenaNavHandler;
    protected override void Configure(IContainerBuilder builder)
    {
        //// 1. Register the local pool
        //builder.RegisterComponent(_vfxPool).As<IVisualEffectPool>();

        //// ... register other Arena-specific gameplay scripts

        //// Register the pure C# math brain
        //builder.Register<StandardBotBrain>(Lifetime.Scoped).As<IBotBrain>();

        //// Ensure the BotController gets injected
        //builder.RegisterComponent(_botPlayerController);

        //// Bind the dumb Unity view
        //builder.RegisterComponent(_dragInputSurface);

        //// Bind the Presenter as the interface the game uses AND as a Startable
        //builder.Register<CueInputPresenter>(Lifetime.Scoped)
        //       .As<ICueInputListener>()
        //       .AsImplementedInterfaces(); // Ensures IStartable/IDisposable run

        builder.RegisterEntryPoint<GameArenaEntryPoint>().AsSelf();

        Debug.Log("[DI_SUCCESS] Game Arena Scope Initialized! Physics sandbox ready.");

        if (_arenaNavHandler != null)
        {
            builder.RegisterComponent(_arenaNavHandler);
        }

        //if (matchData.Mode == "8_Ball")
        //    builder.Register<EightBallRules>(Lifetime.Scoped).As<IGameRuleset>();
        //else
        //    builder.Register<NineBallRules>(Lifetime.Scoped).As<IGameRuleset>();

        //// 1. Register the pure C# physics brain
        //builder.Register<BilliardsPhysicsEngine>(Lifetime.Scoped);

        //// 2. Register the Match Referee
        //builder.Register<MatchCoordinator>(Lifetime.Scoped);

        //// 3. Register the UI Presenter
        //builder.RegisterEntryPoint<ArenaHUDPresenter>();
    }
}