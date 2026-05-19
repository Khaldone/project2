// Assets/_Project/2_Infrastructure/Hardware/FirebasePushWrapper.cs
using System;
using System.Threading.Tasks;
using UnityEngine;
//using Firebase;
//using Firebase.Messaging;


public class FirebasePushWrapper : MonoBehaviour, IPushNotificationService_New
{
    private bool _isInitialized = false;
    public string CurrentDeviceToken { get; private set; }


    public event Action<RemoteNotificationPayload> OnNotificationReceivedForeground;


    public async Task<bool> InitializeAsync()
    {
        //if (_isInitialized) return true;


        //var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        //if (dependencyStatus == DependencyStatus.Available)
        //{
        //    // Firebase is ready to use!
        //    FirebaseMessaging.TokenReceived += OnTokenReceived;
        //    FirebaseMessaging.MessageReceived += OnMessageReceived;
        //    _isInitialized = true;
        //    Debug.Log("AAA Pipeline: Firebase initialized successfully.");
        //    return true;
        //}
        //else
        //{
        //    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        //    return false;
        //}

        return true; // for compilation only.
    }


    //private void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    //{
    //    // This is the unique ID for this specific phone.
    //    // We must cache this so the AppBootstrapper can send it to PlayFab later!
    //    CurrentDeviceToken = token.Token;
    //    Debug.Log($"FCM Token Received: {CurrentDeviceToken}");
    //}


    //private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    //{
    //    // 1. Check if the message actually has a visible notification payload
    //    if (e.Message.Notification != null)
    //    {
    //        // 2. Translate the Firebase specific object into our pure C# struct
    //        var payload = new RemoteNotificationPayload
    //        {
    //            Title = e.Message.Notification.Title,
    //            Body = e.Message.Notification.Body,
    //            // Parse custom key-value data sent from the server
    //            DeepLinkAction = e.Message.Data.ContainsKey("action") ? e.Message.Data["action"] : string.Empty
    //        };


    //        // 3. Fire our pure C# event
    //        OnNotificationReceivedForeground?.Invoke(payload);
    //    }
    //}


    private void OnDestroy()
    {
        //FirebaseMessaging.TokenReceived -= OnTokenReceived;
        //FirebaseMessaging.MessageReceived -= OnMessageReceived;
    }
}
