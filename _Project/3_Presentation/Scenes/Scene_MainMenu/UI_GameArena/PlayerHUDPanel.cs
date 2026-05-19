// Assets/_Project/3_Presentation/UI_GameArena/PlayerHUDPanel.cs
using UnityEngine;
using UnityEngine.UI;


public class PlayerHUDPanel : MonoBehaviour
{
    [SerializeField] private bool _isLocalPanel; // Set to True for the Left HUD, False for the Right HUD in the Unity Inspector

    [Header("UI Elements")]
    [SerializeField] private Image _suitIcon;
    [SerializeField] private Text _ballsRemainingText;
    [SerializeField] private CanvasGroup _activeTurnHighlight;


    private void Awake()
    {
        //MessageBroker.Instance.Subscribe<MappedHUDUpdateMessage>(OnHUDUpdate);
        //MessageBroker.Instance.Subscribe<MappedTurnStateMessage>(OnTurnStateChanged);
    }


    private void OnHUDUpdate(MappedHUDUpdateMessage msg)
    {
        // If this message isn't meant for this specific panel, ignore it.
        if (msg.IsLocalHUD != _isLocalPanel) return;


        _ballsRemainingText.text = msg.BallsRemaining.ToString();

        // Update suit icon (Solids, Stripes, etc.)
        //UpdateSuitVisuals(msg.AssignedSuit);
    }


    private void OnTurnStateChanged(MappedTurnStateMessage msg)
    {
        // If it's my turn, highlight the Local Panel. If not, highlight the Remote Panel.
        bool shouldHighlight = _isLocalPanel ? msg.IsMyTurn : !msg.IsMyTurn;

        // Smoothly fade the highlight in or out (could use a Coroutine or Tweening library here)
        _activeTurnHighlight.alpha = shouldHighlight ? 1.0f : 0.0f;
    }
}
