// Attached to: MainMenuCanvas
using System;
using UnityEngine;
using UnityEngine.UI;

//public class MainMenuView : MonoBehaviour, IMainMenuView
public class MainMenuView : MonoBehaviour
{
    // These are assigned in the Unity Inspector
    [Header("Primary Buttons")]
    [SerializeField] private Button _play1v1Button;
    [SerializeField] private Button _tournamentButton;


    [Header("Sub-Widgets")]
    //[SerializeField] private PlayerProfileWidgetView _profileWidget;
    [SerializeField] private CurrencyWidgetView _coinsWidget;
    [SerializeField] private CurrencyWidgetView _gemsWidget;
    //[SerializeField] private LootboxDockView _lootboxDock;


    // Events the pure C# Presenter will subscribe to
    public event Action OnPlay1v1Clicked;
    public event Action OnTournamentClicked;


    private void Awake()
    {
        // Route Unity Button clicks to our C# events
        _play1v1Button.onClick.AddListener(() => OnPlay1v1Clicked?.Invoke());
        _tournamentButton.onClick.AddListener(() => OnTournamentClicked?.Invoke());
    }


    // Methods the Presenter calls to update the screen
    public void UpdateCoins(int amount) => _coinsWidget.SetAmount(amount);
    public void UpdateGems(int amount) => _gemsWidget.SetAmount(amount);
    //public void UpdateProfile(string name, int level) => _profileWidget.SetProfile(name, level);

    //public void RenderLootboxes(LootboxInstance[] boxes) => _lootboxDock.RenderBoxes(boxes);


    private void OnDestroy()
    {
        _play1v1Button.onClick.RemoveAllListeners();
        _tournamentButton.onClick.RemoveAllListeners();
    }
}
