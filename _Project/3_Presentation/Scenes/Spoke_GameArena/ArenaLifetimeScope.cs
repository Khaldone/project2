//// Assets/Scripts/Presentation/Spoke_GameArena/ArenaLifetimeScope.cs
//using VContainer;
//using VContainer.Unity;
//using UnityEngine;


//public class ArenaLifetimeScope : LifetimeScope
//{
//    //[SerializeField] private ArenaCameraRig _cameraRig;
//    //[SerializeField] private MatchUIView _matchUI;


//    protected override void Configure(IContainerBuilder builder)
//    {
//        // Register local Arena components
//        //builder.RegisterComponent(_cameraRig);
//        //builder.RegisterComponent(_matchUI);


//        // Register the Match logic (scoped only to this scene)
//        builder.Register<MatchCoordinator>(Lifetime.Scoped);
//    }
//}