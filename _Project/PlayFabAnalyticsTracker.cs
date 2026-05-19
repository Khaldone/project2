using System;
using VContainer.Unity;

public class PlayFabAnalyticsTracker : IStartable, IDisposable
{
    private IMessageBroker _messageBroker;

    public PlayFabAnalyticsTracker(IMessageBroker broker) => _messageBroker = broker;

    public void Start() => _messageBroker.Subscribe<MatchEndedMessage>(LogMatchData);
    public void Dispose() => _messageBroker.Unsubscribe<MatchEndedMessage>(LogMatchData);


    private void LogMatchData(MatchEndedMessage msg)
    {
        // Send data to the backend silently
        // Debug.Log($"Sending telemetry: Match took {msg.TotalTurns} turns.");
    }
}
