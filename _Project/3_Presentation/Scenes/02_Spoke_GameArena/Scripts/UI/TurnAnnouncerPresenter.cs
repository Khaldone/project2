// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/UI/TurnAnnouncerPresenter.cs
using PlayFab.EconomyModels;
using System;
using UnityEngine;
using VContainer.Unity;


// IStartable runs when the scene loads. IDisposable runs when the scene unloads.
public class TurnAnnouncerPresenter : IStartable, IDisposable
{
    private readonly ITurnStateMachine _turnState;
    //private readonly ITurnAnnouncerView _view; // The dumb Unity Canvas script

    //public TurnAnnouncerPresenter(ITurnStateMachine turnState, ITurnAnnouncerView view)
    //{
    //    _turnState = turnState;
    //    _view = view;
    //}

    public TurnAnnouncerPresenter(ITurnStateMachine turnState)
    {
        _turnState = turnState;
    }

    public void Start()
    {
        // 1. SUBSCRIBE to the pure C# event
        _turnState.OnTurnStarted += HandleTurnStarted;

        // 2. Set initial state just in case we loaded mid-turn
        HandleTurnStarted(_turnState.CurrentPlayerId);
    }

    private void HandleTurnStarted(string activePlayerId)
    {
        // Tell the dumb View to play its animation
        if (activePlayerId == "Me")
        {
            //_view.ShowYourTurnAnimation();
        }
        else
        {
            //_view.ShowOpponentTurnAnimation(activePlayerId);
        }
    }

    public void Dispose()
    {
        // THE GOLDEN RULE: UNSUBSCRIBE
        // If the Spoke scene unloads, the Presenter dies. If we don't unsubscribe,
        // the global TurnStateMachine will try to call HandleTurnStarted next time,
        // crashing the game because the View was destroyed.
        if (_turnState != null)
        {
            _turnState.OnTurnStarted -= HandleTurnStarted;
        }
    }
}