// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Progression/BattlePassMatchListener.cs
using VContainer.Unity;
using System;


public class BattlePassMatchListener : IStartable, IDisposable
{
    private readonly IMessageBroker _broker;
    private readonly IBattlePassOrchestrator _battlePass;

    public BattlePassMatchListener(IMessageBroker broker, IBattlePassOrchestrator battlePass)
    {
        _broker = broker;
        _battlePass = battlePass;
    }

    public void Start()
    {
        // Listen to the pure match conclusion event we defined in our analytics phase
        _broker.Subscribe<MatchConcludedMessage>(HandleMatchEnded);
    }

    private void HandleMatchEnded(MatchConcludedMessage msg)
    {
        // Business Logic: Calculate XP based on the match result
        int xpToAward = 10; // Base participation XP

        if (msg.DidLocalPlayerWin) xpToAward += 40; // Win bonus
        if (!msg.WasBotOpponent) xpToAward += 10;   // PvP bonus


        // Command the wrapper to tell CBS
        _battlePass.AddExperienceAsync(xpToAward);
    }

    public void Dispose()
    {
        _broker.Unsubscribe<MatchConcludedMessage>(HandleMatchEnded);
    }
}