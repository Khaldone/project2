// Assets/_Project/1_CoreDomain/Social/ChatOrchestrator.cs
using System;
using UnityEngine;


public enum ChatPhraseId { GoodGame = 0, NiceShot = 1, Oops = 2, LaughingEmoji = 3, AngryEmoji = 4 }


public struct ChatMessageEvent
{
    public string SenderId;
    public ChatPhraseId PhraseId;
}


public class ChatOrchestrator_New
{
    private readonly IMessageBroker_New _broker;
    //private readonly IMultiplayerNetworkService _network; // Maps to Photon Fusion

    private float _lastSentChatTime;
    private const float CHAT_COOLDOWN_SECONDS = 2.0f; // Anti-spam throttle


    //public ChatOrchestrator_New(IMessageBroker_New broker, IMultiplayerNetworkService network)
    //{
    //    _broker = broker;
    //    _network = network;
    //}

    public ChatOrchestrator_New(IMessageBroker_New broker)
    {
        _broker = broker;
    }


    // Called by the UI when the local player taps a chat button
    public void TrySendLocalChat(ChatPhraseId phrase)
    {
        // 1. ANTI-SPAM CHECK
        if (Time.time - _lastSentChatTime < CHAT_COOLDOWN_SECONDS)
        {
            // Silently ignore the spam attempt. Do not send to network.
            return;
        }


        _lastSentChatTime = Time.time;


        // 2. SEND TO NETWORK: Tell Photon Fusion to RPC this to the opponent
        //_network.SendChatPhraseToOpponent(phrase);


        // 3. SHOW LOCALLY: Tell the UI to render our own chat bubble immediately
        _broker.Publish(new ChatMessageEvent { SenderId = "LocalPlayer", PhraseId = phrase });
    }


    // Called by the Infrastructure layer when Photon receives an RPC from the opponent
    public void OnOpponentChatReceived(ChatPhraseId phrase)
    {
        // Tell the UI to render the opponent's chat bubble
        _broker.Publish(new ChatMessageEvent { SenderId = "Opponent", PhraseId = phrase });

        // Trigger a subtle UI sound effect based on the phrase type
        //_broker.Publish(new PlayAudioMessage("SFX_Chat_Pop"));
    }
}
