using UnityEngine;
using Firebase.Messaging;
using System.Collections;

//#if UNITY_IOS
//using Unity.Notifications.iOS;
//#endif

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace MobileMonetizationPro
{
    public class RemoteNotifications : MonoBehaviour
    {
        public void Start()
        {
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                //// ✅ Permissions
                RequestNotificationPermission();

                // ✅ Subscribe to Firebase events
                FirebaseMessaging.TokenReceived += OnTokenReceived;
                    FirebaseMessaging.MessageReceived += OnMessageReceived;
                }
                else
                {
                    UnityEngine.Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                }
            });
        }

        void RequestNotificationPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Debug.Log("Requesting POST_NOTIFICATIONS permission...");
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
        else
        {
            Debug.Log("POST_NOTIFICATIONS permission already granted.");
        }
#endif

            //#if UNITY_IOS
            //var authOptions = AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound;
            //var request = new AuthorizationRequest(authOptions, true);
            //StartCoroutine(CheckAuthorization(request));
            //#endif
        }

        //#if UNITY_IOS
        //    IEnumerator CheckAuthorization(AuthorizationRequest request)
        //    {
        //        while (!request.IsFinished)
        //        {
        //            yield return null;
        //        }

        //        Debug.Log($"🔔 iOS Notification Permission Granted: {request.Granted}");
        //    }
        //#endif

        public void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            UnityEngine.Debug.Log("✅ Received FCM Registration Token: " + token.Token);
        }

        public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            UnityEngine.Debug.Log("📨 Received a new message from: " + e.Message.From);
        }
    }






    //using UnityEngine;
    //using Firebase.Messaging;

    //#if UNITY_ANDROID && !UNITY_EDITOR
    //using UnityEngine.Android;
    //#endif

    //public class PushNotifications_Firebase : MonoBehaviour
    //{
    //    public void Start()
    //    {
    //        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
    //        {
    //            var dependencyStatus = task.Result;
    //            if (dependencyStatus == Firebase.DependencyStatus.Available)
    //            {
    //                // Firebase is ready
    //                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

    //                // ✅ Ask for POST_NOTIFICATIONS permission on Android 13+
    //                RequestNotificationPermission();
    //            }
    //            else
    //            {
    //                UnityEngine.Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
    //            }
    //        });

    //        FirebaseMessaging.TokenReceived += OnTokenReceived;
    //        FirebaseMessaging.MessageReceived += OnMessageReceived;
    //    }

    //    void RequestNotificationPermission()
    //    {
    //#if UNITY_ANDROID && !UNITY_EDITOR
    //        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
    //        {
    //            Debug.Log("Requesting POST_NOTIFICATIONS permission...");
    //            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
    //        }
    //        else
    //        {
    //            Debug.Log("POST_NOTIFICATIONS permission already granted.");
    //        }
    //#endif
    //    }

    //    public void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    //    {
    //        UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
    //    }

    //    public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    //    {
    //        UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
    //    }
    //}
}