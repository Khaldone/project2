using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;

namespace MobileMonetizationPro
{
    public class MobileMonetizationPro_FirebaseAnalyticsInitializer : MonoBehaviour
    {
        private Firebase.FirebaseApp app;
        // Start is called before the first frame update
        void Start()
        {
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    app = Firebase.FirebaseApp.DefaultInstance;

                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                    UnityEngine.Debug.Log(System.String.Format(
                       "Success: ", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });
        }
    }
}
