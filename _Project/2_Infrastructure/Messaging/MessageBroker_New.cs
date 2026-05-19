// Assets/_Project/2_Infrastructure/Messaging/MessageBroker.cs
using System;
using System.Collections.Generic;


public class MessageBroker_New : IMessageBroker_New
{
    // The master ledger of who is listening to what
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();


    public void Publish<T>(T message)
    {
        Type messageType = typeof(T);


        // If nobody is listening for this message, do nothing
        if (!_subscribers.ContainsKey(messageType)) return;


        // If people are listening, loop through them and fire their methods
        var delegates = _subscribers[messageType];

        // We iterate backwards in case someone unsubscribes DURING the broadcast
        for (int i = delegates.Count - 1; i >= 0; i--)
        {
            var action = (Action<T>)delegates[i];
            action?.Invoke(message);
        }
    }


    public IDisposable Subscribe<T>(Action<T> onMessage)
    {
        Type messageType = typeof(T);


        if (!_subscribers.ContainsKey(messageType))
        {
            _subscribers[messageType] = new List<Delegate>();
        }


        _subscribers[messageType].Add(onMessage);


        // Return a custom object that handles the un-subscription automatically
        return new SubscriptionDisposer(() =>
        {
            if (_subscribers.ContainsKey(messageType))
            {
                _subscribers[messageType].Remove(onMessage);
            }
        });
    }


    // A private helper class to handle memory management cleanly
    private class SubscriptionDisposer : IDisposable
    {
        private readonly Action _unsubscribeAction;
        public SubscriptionDisposer(Action unsubscribeAction) => _unsubscribeAction = unsubscribeAction;
        public void Dispose() => _unsubscribeAction?.Invoke();
    }
}
