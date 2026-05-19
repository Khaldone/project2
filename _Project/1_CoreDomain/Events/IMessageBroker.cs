// Assets/_Project/CoreDomain/Events/IMessageBroker.cs
using System;
public interface IMessageBroker
{
    void Publish<T>(T message);
    void Subscribe<T>(Action<T> handler);
    void Unsubscribe<T>(Action<T> handler);
}