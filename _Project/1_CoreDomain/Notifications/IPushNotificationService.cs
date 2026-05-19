// 1. Assets/_Project/CoreDomain/Notifications/IPushNotificationService.cs
using System;
using System.Threading.Tasks;

public interface IPushNotificationService
{
    string CurrentDeviceToken { get; }

    // Fired when Firebase generates the unique device ID
    event Action<string> OnTokenReceived;

    Task InitializeAsync();
}