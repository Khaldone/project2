// Assets/_Project/CoreDomain/Events/MessageBroker.cs
using System;
using System.Collections.Generic;
public class MessageBroker : IMessageBroker
{
    // A dictionary matching a specific Data Type (like MatchEndedMessage)
    // to a chain of Methods (Delegates) that want to hear about it.
    private readonly Dictionary<Type, Delegate> _subscribers = new Dictionary<Type, Delegate>();

    public void Subscribe<T>(Action<T> handler)
    {
        Type messageType = typeof(T);

        if (!_subscribers.ContainsKey(messageType))
        {
            _subscribers[messageType] = null;
        }

        // Add the new listener to the chain
        _subscribers[messageType] = (Action<T>)_subscribers[messageType] + handler;
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        Type messageType = typeof(T);

        if (_subscribers.ContainsKey(messageType))
        {
            // Remove the listener from the chain
            _subscribers[messageType] = (Action<T>)_subscribers[messageType] - handler;

            // Cleanup memory if nobody is listening anymore
            if (_subscribers[messageType] == null)
            {
                _subscribers.Remove(messageType);
            }
        }
    }

    public void Publish<T>(T message)
    {
        Type messageType = typeof(T);

        // If anyone is listening to this specific type of message, invoke their methods!
        if (_subscribers.TryGetValue(messageType, out Delegate handlers))
        {
            ((Action<T>)handlers)?.Invoke(message);
        }
    }
}