// Assets/_Project/Scenes/UI_Tutorial/Scripts/TutorialLifetimeScope.cs
using UnityEngine;
using VContainer;
using VContainer.Unity;


public class TutorialLifetimeScope : LifetimeScope
{
    [Header("Scene Views")]
    [Tooltip("Drag the Unity Canvas script that handles the Tutorial UI here.")]
    [SerializeField] private ITutorialView _tutorialView;


    protected override void Configure(IContainerBuilder builder)
    {
        // 1. Bind the Dumb View
        // We tell VContainer: "When the Presenter asks for an ITutorialView, give it this exact Canvas."
        builder.RegisterComponent(_tutorialView).As<ITutorialView>();


        // 2. Bind the Smart Presenter (The Entry Point)
        // We tell VContainer: "Create this pure C# class, run its Start() method now,
        // and run its Dispose() method when the scene unloads."
        builder.RegisterEntryPoint<TutorialPresenter>();

        // Notice we do NOT register IMessageBroker or IUIRouter here!
        // VContainer automatically climbs the hierarchy to the Hub scene to find them.
    }
}
