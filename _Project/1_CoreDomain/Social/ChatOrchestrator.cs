// 3. Assets/_Project/CoreDomain/Social/ChatOrchestrator.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


// The Brain. This is exposed to the UI via the Service Locator.
public interface IChatOrchestrator
{
    event Action<ChatMessage> OnMessageProcessed;
    IReadOnlyList<ChatMessage> GetRecentHistory(string playerId);
    Task<bool> SendMessageAsync(string targetPlayerId, string text);
}

public class ChatOrchestrator : IChatOrchestrator
{
    private readonly IChatBackend _chatBackend;
    // Memory cache to hold history so it persists when switching scenes
    private readonly Dictionary<string, List<ChatMessage>> _chatHistories = new Dictionary<string, List<ChatMessage>>();

    public event Action<ChatMessage> OnMessageProcessed;
    public ChatOrchestrator(IChatBackend chatBackend)
    {
        _chatBackend = chatBackend;
        _chatBackend.OnMessageReceivedFromNetwork += HandleIncomingMessage;
    }
    public async Task<bool> SendMessageAsync(string targetPlayerId, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        // Optionally add profanity filtering here before sending!

        bool success = await _chatBackend.SendPrivateMessageAsync(targetPlayerId, text);

        if (success)
        {
            // Log our own message into the history
            var msg = new ChatMessage { SenderId = "Me", Text = text, TimestampTicks = DateTime.UtcNow.Ticks };
            SaveToHistory(targetPlayerId, msg);
            OnMessageProcessed?.Invoke(msg);
        }
        return success;
    }

    private void HandleIncomingMessage(ChatMessage message)
    {
        // Save the incoming message to that specific friend's history log
        SaveToHistory(message.SenderId, message);

        // Notify the UI that a new message is ready to display
        OnMessageProcessed?.Invoke(message);
    }
    private void SaveToHistory(string contactId, ChatMessage msg)
    {
        if (!_chatHistories.ContainsKey(contactId))
        {
            _chatHistories[contactId] = new List<ChatMessage>();
        }

        _chatHistories[contactId].Add(msg);

        // Memory management: Keep only the last 50 messages per friend
        if (_chatHistories[contactId].Count > 50)
        {
            _chatHistories[contactId].RemoveAt(0);
        }
    }
    public IReadOnlyList<ChatMessage> GetRecentHistory(string playerId)
    {
        return _chatHistories.TryGetValue(playerId, out var history) ? history : new List<ChatMessage>();
    }
}