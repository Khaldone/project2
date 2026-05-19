// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Social/PhotonChatWrapper.cs
using System;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

public class PhotonChatWrapper : MonoBehaviour, IChatBackend
{
    public event Action<ChatMessage> OnMessageReceivedFromNetwork;


    // Simulate connecting to the third-party chat SDK
    public async Task<bool> SendPrivateMessageAsync(string targetPlayerId, string message)
    {
        // e.g., PhotonChatClient.SendPrivateMessage(targetPlayerId, message);
        await Task.Delay(100); // Simulate network latency
        return true;
    }


    // A callback triggered by the third-party SDK when someone messages us
    public void OnPhotonMessageReceived(string sender, string message)
    {
        var newMsg = new ChatMessage
        {
            SenderId = sender,
            Text = message,
            TimestampTicks = DateTime.UtcNow.Ticks
        };

        OnMessageReceivedFromNetwork?.Invoke(newMsg);
    }
}
