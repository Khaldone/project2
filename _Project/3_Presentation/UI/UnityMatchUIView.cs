// Assets/Scripts/Presentation/UI/UnityMatchUIView.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UnityMatchUIView : MonoBehaviour, IMatchUIView
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI turnIndicatorText;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private Button endTurnButton;


    public void UpdateTurnIndicator(int playerId)
    {
        turnIndicatorText.text = $"Player {playerId}'s Turn";

        // You could also trigger Unity Animator states here
        // e.g., GetComponent<Animator>().SetTrigger("SlideIn");
    }


    public void ShowNotification(string message)
    {
        notificationText.text = message;
        // Trigger fade-out coroutine, etc.
    }


    public void SetEndTurnButtonInteractable(bool isInteractable)
    {
        endTurnButton.interactable = isInteractable;
    }


    // Pass user input back to the pure C# logic (e.g., via the Coordinator or a separate Input interface)
    public void OnEndTurnButtonClicked()
    {
        // This button's OnClick event in the Inspector points here.
        // You would typically route this to an IPlayerInput abstraction.
    }
}
