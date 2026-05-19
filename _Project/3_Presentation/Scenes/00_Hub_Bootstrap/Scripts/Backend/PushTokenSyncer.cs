// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Backend/PushTokenSyncer.cs
using PlayFab;
using PlayFab.ClientModels;
using VContainer.Unity;
using System;

public class PushTokenSyncer : IStartable, IDisposable
{
    private readonly IPushNotificationService _pushService;

    public PushTokenSyncer(IPushNotificationService pushService)
    {
        _pushService = pushService;
    }

    public void Start()
    {
        // Listen for the moment Firebase gets the token from Apple/Google
        _pushService.OnTokenReceived += SendTokenToPlayFab;
    }

    private void SendTokenToPlayFab(string fcmToken)
    {
        if (!PlayFabClientAPI.IsClientLoggedIn()) return;


#if UNITY_ANDROID
        var request = new AndroidDevicePushNotificationRegistrationRequest
        {
            DeviceToken = fcmToken,
            SendPushNotificationConfirmation = false
        };
        PlayFabClientAPI.AndroidDevicePushNotificationRegistration(request,
            result => UnityEngine.Debug.Log("AAA Pipeline: Android Token registered to PlayFab!"),
            error => UnityEngine.Debug.LogError(error.GenerateErrorReport())
        );
#elif UNITY_IOS
        // Note: iOS requires additional APNs setup in PlayFab dashboard
        var request = new RegisterForIOSPushNotificationRequest
        {
            DeviceToken = fcmToken,
            SendPushNotificationConfirmation = false
        };
        PlayFabClientAPI.RegisterForIOSPushNotification(request,
            result => UnityEngine.Debug.Log("AAA Pipeline: iOS Token registered to PlayFab!"),
            error => UnityEngine.Debug.LogError(error.GenerateErrorReport())
        );
#endif
    }

    public void Dispose()
    {
        _pushService.OnTokenReceived -= SendTokenToPlayFab;
    }
}