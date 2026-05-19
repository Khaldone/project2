// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Notifications/FirebasePushWrapper.cs
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

//using Firebase;
//using Firebase.Messaging;
using VContainer.Unity;

public class FirebasePushWrapper : MonoBehaviour, IPushNotificationService, IDisposable
{
    private readonly IMessageBroker _broker;
    private readonly IDeepLinkOrchestrator _deepLinkOrchestrator;

    public string CurrentDeviceToken { get; private set; }
    public event Action<string> OnTokenReceived;


    // We inject the Broker so we can route foreground messages
    //[Inject]
    //public void Construct(IMessageBroker broker)
    //{
    //    _broker = broker;
    //}

    // VContainer will automatically use this.
    public FirebasePushWrapper(IMessageBroker broker, IDeepLinkOrchestrator deepLinkOrchestrator)
    {
        _broker = broker;
        _deepLinkOrchestrator = deepLinkOrchestrator;
    }


    public async Task InitializeAsync()
    {
        // 1. Ensure the device can actually run Firebase
        //var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        //if (dependencyStatus == DependencyStatus.Available)
        //{
        //    // 2. Subscribe to Firebase events
        //    FirebaseMessaging.TokenReceived += HandleTokenReceived;
        //    FirebaseMessaging.MessageReceived += HandleMessageReceived;

        //    Debug.Log("AAA Pipeline: Firebase Messaging Initialized.");
        //}
        //else
        //{
        //    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        //}

        //var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        //if (dependencyStatus == DependencyStatus.Available)
        //{
        //    // ... (Token subscription logic) ...

        //    // CHECK FOR COLD BOOT DEEP LINKS
        //    CheckForInitialPushPayload();
        //}
    }


    //private void HandleTokenReceived(object sender, TokenReceivedEventArgs token)
    //{
    //    Debug.Log($"AAA Pipeline: FCM Token Received: {token.Token}");
    //    CurrentDeviceToken = token.Token;
    //    OnTokenReceived?.Invoke(CurrentDeviceToken);
    //}


    //private void HandleMessageReceived(object sender, MessageReceivedEventArgs e)
    //{
    //    // THIS ONLY FIRES IF THE APP IS CURRENTLY OPEN AND RUNNING!
    //    // If the app is closed, iOS/Android shows the standard system popup.

    //    var notification = e.Message.Notification;
    //    if (notification != null)
    //    {
    //        // Parse custom data if you sent a deep link from your server
    //        e.Message.Data.TryGetValue("action", out string actionKey);


    //        // Drop it into our pure C# Message Broker!
    //        _broker.Publish(new RemotePushMessage
    //        {
    //            Title = notification.Title,
    //            Body = notification.Body,
    //            DeepLinkAction = actionKey
    //        });
    //    }
    //}



    private void CheckForInitialPushPayload()
    {
        // Firebase caches the message that woke the app up in the Unity intent/dynamic links
        // (This is a simplified conceptual representation of Firebase's dynamic link/intent catching)
        //Firebase.Messaging.FirebaseMessaging.TokenReceived += (sender, args) =>
        //{
        //    // Note: In a production Android/iOS app, you often read this from Application.absoluteURL
        //    // or Firebase Dynamic Links API if using universal links.
        //};


        // If we detect a payload dictionary containing an "action" key:
        string incomingAction = "open_shop"; // Extracted from the notification payload


        // 1. Store it in the vault!
        _deepLinkOrchestrator.SetPendingDeepLink(incomingAction);

        // The wrapper's job is now done. It does NOT try to load the scene here.
    }



    public void Dispose()
    {
        //FirebaseMessaging.TokenReceived -= HandleTokenReceived;
        //FirebaseMessaging.MessageReceived -= HandleMessageReceived;
    }

}
