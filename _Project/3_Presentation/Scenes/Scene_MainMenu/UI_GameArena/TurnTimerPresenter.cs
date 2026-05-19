// Assets/_Project/3_Presentation/UI_GameArena/TurnTimerPresenter.cs
using UnityEngine;
using UnityEngine.UI;


public class TurnTimerPresenter : MonoBehaviour
{
    [SerializeField] private Image _timerFillBar;
    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _panicColor;


    private void Start()
    {
        // Listen to the Pure C# Coordinator
        //MessageBroker.Instance.Subscribe<TurnTimerSyncMessage>(OnTimerSync);
    }

    private void OnTimerSync(TurnTimerSyncMessage msg)
    {
        // Update the visual bar
        _timerFillBar.fillAmount = msg.TimeRemaining / msg.TimeTotal;


        // Flash red if panic mode is active
        _timerFillBar.color = msg.IsPanicMode ? _panicColor : _normalColor;
    }
}