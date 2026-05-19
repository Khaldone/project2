// Assets/_Project/2_Infrastructure/Hardware/HardwareOrchestrator.cs
using System;
using VContainer.Unity;


public class HardwareOrchestrator : IStartable, IDisposable
{
    private readonly IPushNotificationService_New _pushService;
    private readonly IMessageBroker_New _broker;


    public HardwareOrchestrator(IPushNotificationService_New pushService, IMessageBroker_New broker)
    {
        _pushService = pushService;
        _broker = broker;
    }


    public void Start()
    {
        // Listen to the pure interface (we don't care that it's actually Firebase)
        _pushService.OnNotificationReceivedForeground += HandleForegroundPush;
    }


    private void HandleForegroundPush(RemoteNotificationPayload payload)
    {
        // We received a push while the player is actively holding their phone!
        // Publish it to the Broker.
        //_broker.Publish(new SystemNotificationMessage(payload.Title, payload.Body));
    }


    public void Dispose()
    {
        _pushService.OnNotificationReceivedForeground -= HandleForegroundPush;
    }
}