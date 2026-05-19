// Assets/_Project/Scenes/01_Spoke_MainMenu/Scripts/UI/ChatWindowPresenter.cs
using UnityEngine;
using TMPro;


public class ChatWindowPresenter : MonoBehaviour
{
    [SerializeField] private TMP_InputField _chatInputField;
    [SerializeField] private TextMeshProUGUI _chatHistoryText;

    private IChatOrchestrator _chatService;
    private string _currentFriendId = "Player_99"; // Example target

    private void Start()
    {
        // 1. Grab the Global Service safely
        _chatService = ServiceLocator.Get<IChatOrchestrator>();


        // 2. Subscribe to live updates
        _chatService.OnMessageProcessed += UpdateChatUI;


        // 3. Load the history (in case we just switched scenes!)
        RefreshHistoryDisplay();
    }

    private void OnDestroy()
    {
        // Always unsubscribe to prevent UI memory leaks
        if (_chatService != null)
            _chatService.OnMessageProcessed -= UpdateChatUI;
    }

    public async void OnSendButtonClicked()
    {
        string text = _chatInputField.text;
        _chatInputField.text = ""; // Clear immediately for UX


        bool success = await _chatService.SendMessageAsync(_currentFriendId, text);

        if (!success)
        {
            Debug.LogWarning("Failed to send message.");
            // Show red exclamation mark next to message, etc.
        }
    }

    private void UpdateChatUI(ChatMessage newMessage)
    {
        // Only update if it's the friend we are currently looking at
        if (newMessage.SenderId == _currentFriendId || newMessage.SenderId == "Me")
        {
            _chatHistoryText.text += $"\n[{newMessage.SenderId}]: {newMessage.Text}";
        }
    }

    private void RefreshHistoryDisplay()
    {
        _chatHistoryText.text = "";
        var history = _chatService.GetRecentHistory(_currentFriendId);

        foreach (var msg in history)
        {
            _chatHistoryText.text += $"\n[{msg.SenderId}]: {msg.Text}";
        }
    }
}