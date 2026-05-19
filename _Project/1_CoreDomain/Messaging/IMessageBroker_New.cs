// Assets/_Project/1_CoreDomain/Messaging/IMessageBroker.cs
using System;


public interface IMessageBroker_New
{
    // "Hey everyone, this just happened!"
    void Publish<T>(T message);


    // "Let me know when this specific thing happens."
    // It returns an IDisposable so we can safely unsubscribe later.
    IDisposable Subscribe<T>(Action<T> onMessage);
}