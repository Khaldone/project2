// Assets/_Project/CoreDomain/Progression/IPlayerDataService.cs
using System;

public interface IPlayerDataServiceProgression
{
    int CurrentCoins { get; }
    string PlayerName { get; }

    // The UI will subscribe to these events!
    event Action<int> OnCoinsChanged;
    event Action<string> OnNameChanged;
}