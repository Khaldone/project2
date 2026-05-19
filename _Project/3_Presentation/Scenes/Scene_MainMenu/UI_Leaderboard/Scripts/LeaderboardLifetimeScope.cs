// Assets/_Project/Scenes/UI_Leaderboard/Scripts/LeaderboardLifetimeScope.cs
using UnityEngine;
using VContainer;
using VContainer.Unity;


public class LeaderboardLifetimeScope : LifetimeScope
{
    [Header("Scene Views")]
    [Tooltip("Drag the Unity Canvas script that handles the scrolling UI here.")]
    [SerializeField] private LeaderboardView _leaderboardView;


    protected override void Configure(IContainerBuilder builder)
    {
        // 1. Bind the Dumb View
        // We tell VContainer: "Whenever a C# script asks for an ILeaderboardView,
        // give them this exact Unity Canvas component."
        builder.RegisterComponent(_leaderboardView).As<ILeaderboardView>();


        // 2. Bind the Smart Presenter (The Entry Point)
        // We tell VContainer: "Create a new instance of this pure C# class.
        // Because it is an Entry Point, call its Start() method when the scene
        // loads, and call its Dispose() method when the scene unloads."
        builder.RegisterEntryPoint<LeaderboardPresenter>();
    }
}