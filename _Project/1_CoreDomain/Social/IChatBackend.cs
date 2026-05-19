// 2. Assets/_Project/CoreDomain/Social/IChatBackend.cs
using System;
using System.Threading.Tasks;

// The raw networking capability (Photon, PlayFab, or custom WebSockets)
public interface IChatBackend
{
    Task<bool> SendPrivateMessageAsync(string targetPlayerId, string message);

    // We use a pure C# event so the CoreDomain can listen to the network
    event Action<ChatMessage> OnMessageReceivedFromNetwork;
}