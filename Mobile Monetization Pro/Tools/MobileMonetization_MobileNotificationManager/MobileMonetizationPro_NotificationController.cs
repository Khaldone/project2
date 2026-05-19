using System.Collections;
using System.Collections.Generic;
using TMPro;
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif
#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#endif
using UnityEngine;

namespace MobileMonetizationPro
{
    public class MobileMonetizationPro_NotificationController : MonoBehaviour
    {
        public static MobileMonetizationPro_NotificationController instance;

        [TextArea(10, 20)]
        public string KeyNotes = "Here are some important points to keep in mind when setting up notifications:\n" +
        "1. **Notification Grouping on Android**: Android may group notifications if you send too many in a short period. " +
        "Ensure the intervals between notifications are well-defined to avoid this.\n" +
        "2. **Notification Bundles**: On newer Android versions, notifications that are related or excessive may be grouped into 'bundles'. " +
        "This is a design choice by Android OS to prevent clutter and improve user experience.\n" +
        "3. **Notification Channel Registration**: The game must be played for at least 20 seconds to register the notification channel. " +
        "Notifications may not work if the game is closed immediately after installation.\n" +
        "4. **Notification Limitations**: There is a limit on how many notifications you can send and register. Notifications cannot run in a continuous loop. " +
        "For example, if you schedule a notification every 24 hours for 10 days, after the last notification, it will not reset automatically. " +
        "This is because it is difficult to track whether the last notification was triggered without the game being opened.\n" +
        "5. **Notification Reset Option**: You can enable an option to reset notifications when the game is opened. If unchecked, notifications will continue as scheduled. " +
        "However, once the maximum loop count is reached, the player must open the game to start receiving new notifications.\n" +
        "6. **Sequential Notifications**: Notifications will, by default, be served sequentially, one after the other.\n" +
        "7. **Understanding Loop Notifications**: Unity schedules notifications while the app is running. Once scheduled, notifications will continue to fire at their respective times, even if the app is closed. " +
        "To restart the notification sequence after the last scheduled notification and the maximum loop number is reached, you must schedule notifications in advance. " +
        "Scheduling too many notifications may increase memory and battery usage, especially on lower-end devices.So Keep the Loop Notifications number to 1-2 Max";

        [HideInInspector]
        [Tooltip("Assign a TextMeshProUGUI component here to display debug messages specifically for iOS notification behavior during development. (Optional)")]
        public TextMeshProUGUI DebugTextForiOS;

        [Header("Specify the repeat cycles after the last notification is served")]
        [Tooltip("Specifies how many times the full notification schedule should loop after completing once. Example: If set to 2, the notifications will repeat twice after the first full cycle.")]
        [Range(1, 3)]
        public int NotificationCycle = 2;

        [Tooltip("Enable this to reset and re-schedule notifications every time the player starts the game. Disable to continue the original notification schedule without resetting.")]
        public bool ResetNotificationsWhenGameStart = true;

        [System.Serializable]
        public class Notifcenter
        {
            [Tooltip("Define the title, body text, and optional subtitle (for iOS) for this notification.")]
            public NotifDesc AboutNotification;

            [Tooltip("Set the delay after which this notification should appear. You can specify days, hours, minutes, and seconds.")]
            public Notiftime NotificationRecievingTime;

            [Tooltip("Assign custom small and large icons for this notification on Android.")]
            public NotifIcon NotificationIcons;

            [HideInInspector]
            public int totalSeconds;
        }

        [System.Serializable]
        public class NotifDesc
        {
            [Tooltip("The title text displayed on the notification.")]
            public string NotificationTitle;

            [Tooltip("The main body text of the notification.")]
            public string NotificationDescription;

            [Tooltip("A subtitle for the notification (used only on iOS devices).")]
            public string NotificationSubTitleForIOS;
        }

        [System.Serializable]
        public class Notiftime
        {
            [Tooltip("Delay in days before this notification is triggered.")]
            public int Days;

            [Tooltip("Delay in hours before this notification is triggered.")]
            public int Hours;

            [Tooltip("Delay in minutes before this notification is triggered.")]
            public int Minutes;

            [Tooltip("Delay in seconds before this notification is triggered.")]
            public int Seconds;
        }

        [System.Serializable]
        public class NotifIcon
        {
            [Tooltip("The name of the small icon file (must exist in the Android project Resources).")]
            public string SmallIconName;

            [Tooltip("The name of the large icon file (must exist in the Android project Resources).")]
            public string LargeIconName;
        }

        [Tooltip("List of notifications to schedule. Each entry defines its title, description, delay time, and icons.")]
        public List<Notifcenter> NotificationSetup = new List<Notifcenter>();

        int MaxID;

        int CurrentTime;

        int temp;

        int tempcheck;

        private void Awake()
        {
            if (instance == null)
            {
                PlayerPrefs.SetInt("NotificationStarted", 0);
                instance = this;
                DontDestroyOnLoad(gameObject); // Makes sure this instance persists across scenes
            }
            else
            {
                Destroy(gameObject); // Destroy duplicate instance
            }
        }
        private void Start()
        {
            for (int x = 0; x < NotificationSetup.Count; x++)
            {
                NotificationSetup[x].totalSeconds = NotificationSetup[x].NotificationRecievingTime.Days * 24 * 60 * 60 + NotificationSetup[x].NotificationRecievingTime.Hours * 60 * 60 + NotificationSetup[x].NotificationRecievingTime.Minutes * 60 + NotificationSetup[x].NotificationRecievingTime.Seconds;
            }

            for (int i = 0; i < NotificationCycle; i++)
            {
                for (int x = 0; x < NotificationSetup.Count; x++)
                {
                    tempcheck = NotificationSetup[x].totalSeconds + tempcheck;
                    temp = tempcheck + i + x;
                }
            }

            PlayerPrefs.SetString("LastScheduledNotificationForIOSServed", temp.ToString());

#if UNITY_ANDROID
            RequestAuthorization();
            if (ResetNotificationsWhenGameStart == true)
            {
                if (PlayerPrefs.GetInt("NotificationStarted") == 0)
                {
                    ServeNotificationsOnAndroid();
                }
            }
            else
            {
                var status = AndroidNotificationCenter.CheckScheduledNotificationStatus(temp);

                if (status == NotificationStatus.Delivered || status == NotificationStatus.Unknown)
                {
                    PlayerPrefs.SetInt("NotificationStartedWithoutReset", 0);
                }


                if (PlayerPrefs.GetInt("NotificationStarted") == 0 && PlayerPrefs.GetInt("NotificationStartedWithoutReset") == 0)
                {
                    if (PlayerPrefs.GetInt("NotificationStarted") == 0)
                    {
                        ServeNotificationsOnAndroid();
                    }
                }

            }
#endif
#if UNITY_IOS
        StartCoroutine(RequestAuthorizationForIOS());  
        if (ResetNotificationsWhenGameStart == true)
        {
            if (PlayerPrefs.GetInt("NotificationStarted") == 0)
            {
                ServeNotificationsOnIOS();
            }
        }
        else
        {
            var scheduledNotifications = iOSNotificationCenter.GetScheduledNotifications();
            if (scheduledNotifications.Length > 0)
            {
               // DebugTextForiOS.text = "Notification Already Exist";
            }
            else
            {
               // DebugTextForiOS.text = "Notification Resetted";
                PlayerPrefs.SetInt("NotificationStartedWithoutReset", 0);
            }

            // If notifications haven't started, serve them
            if (PlayerPrefs.GetInt("NotificationStarted") == 0 && PlayerPrefs.GetInt("NotificationStartedWithoutReset") == 0)
            {
                
                ServeNotificationsOnIOS(); // Serve notifications
            }


        }


#endif
        }
#if UNITY_ANDROID
        public void RequestAuthorization()
        {
            if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
                Debug.Log("Permission Granted for notifications");
            }
        }
        public void ServeNotificationsOnAndroid()
        {
            CurrentTime = 0;
            RegisterNotificationChannel();
            for (int i = 0; i < NotificationCycle; i++)
            {
                for (int x = 0; x < NotificationSetup.Count; x++)
                {
                    CurrentTime = NotificationSetup[x].totalSeconds + CurrentTime;
                    SendNotificationForAndroid(NotificationSetup[x].AboutNotification.NotificationTitle, NotificationSetup[x].AboutNotification.NotificationDescription, CurrentTime, NotificationSetup[x].NotificationIcons.SmallIconName, NotificationSetup[x].NotificationIcons.LargeIconName, CurrentTime + i + x);
                }
            }
            PlayerPrefs.SetInt("NotificationStarted", 1);
            PlayerPrefs.SetInt("NotificationStartedWithoutReset", 1);
        }
        public void RegisterNotificationChannel()
        {
            AndroidNotificationCenter.CancelAllScheduledNotifications();
            AndroidNotificationCenter.CancelAllDisplayedNotifications();

            var channel = new AndroidNotificationChannel()
            {
                Id = "default_channel",
                Name = "Default Channel",
                Importance = Importance.Default,
                Description = "Generic notification"
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);

            Debug.Log("Notification Registered");
        }

        public void SendNotificationForAndroid(string title, string text, int fireTimeInSeconds, string SmallIconName, string LargeIconName, int ID)
        {

            System.DateTime currentTime = System.DateTime.Now;


            // Schedule each notification at the correct interval
            var notification = new AndroidNotification
            {
                Title = title,
                Text = text,
                FireTime = currentTime.AddSeconds(fireTimeInSeconds), // Consistent interval
                SmallIcon = SmallIconName,
                LargeIcon = LargeIconName
            };


            AndroidNotificationCenter.SendNotificationWithExplicitID(notification, "default_channel", ID);

        }
#endif

#if UNITY_IOS
    public IEnumerator RequestAuthorizationForIOS()
    {
        using var req = new AuthorizationRequest(authorizationOption: AuthorizationOption.Alert | AuthorizationOption.Badge,
            registerForRemoteNotifications: true);
        while (!req.IsFinished)
        {
            yield return null;
        }
        if(PlayerPrefs.GetInt("FirstTimeSendingNotificationOniOS") == 0)
        {
            ServeNotificationsOnIOS(); // Serve notifications
            PlayerPrefs.SetInt("FirstTimeSendingNotificationOniOS", 1);
        }
       
    }
    public void ServeNotificationsOnIOS()
    {
        iOSNotificationCenter.RemoveAllScheduledNotifications();
        CurrentTime = 0;
        for (int i = 0; i < NotificationCycle; i++)
        {
            for (int x = 0; x < NotificationSetup.Count; x++)
            {
                CurrentTime = NotificationSetup[x].totalSeconds + CurrentTime;
                int ID = CurrentTime + i + x;
                SendNotificationIOS(NotificationSetup[x].AboutNotification.NotificationTitle, NotificationSetup[x].AboutNotification.NotificationDescription, NotificationSetup[x].AboutNotification.NotificationSubTitleForIOS, CurrentTime, ID.ToString());
            }
        }

        Debug.Log("Serving Started");
        PlayerPrefs.SetInt("NotificationStarted", 1);
        PlayerPrefs.SetInt("NotificationStartedWithoutReset", 1);
 
    }
    public void SendNotificationIOS(string title, string body, string subtitle, int fireTimeInSeconds, string notificationId)
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new System.TimeSpan(hours: 0, minutes: 0, fireTimeInSeconds),
            Repeats = false
        };

        var notification = new iOSNotification()
        {
            Identifier = notificationId, // Unique identifier
            Title = title,
            Body = body,
            Subtitle = subtitle,
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "default_category",
            ThreadIdentifier = "thread_" + notificationId,            
            Trigger = timeTrigger
        };

        iOSNotificationCenter.ScheduleNotification(notification);

        PlayerPrefs.Save();
    }
#endif

    }
}



























// OLD CODE WHICH DID NOT WORKED WELL//
//using System.Collections;
//using System.Collections.Generic;
//#if UNITY_IOS
//using Unity.Notifications.iOS;
//#endif
//#if UNITY_ANDROID
//using Unity.Notifications.Android;
//using UnityEngine.Android;
//#endif
//using UnityEngine;

//public class MobileMonetizationPro_NotificationController : MonoBehaviour
//{
//    [System.Serializable]
//    public class NotifDesc
//    {
//        public string NotificationTitle;
//        public string NotificationDescription;
//        public string NotificationSubTitleForIOS;
//    }

//    [System.Serializable]
//    public class Notiftime
//    {
//        public int Days;
//        public int Hours;
//        public int Minutes;
//        public int Seconds;
//    }

//    [System.Serializable]
//    public class NotifIcon
//    {
//        public string SmallIconName = "small";
//        public string LargeIconName = "large";
//    }

//    public NotifDesc AboutNotification;
//    public Notiftime NotificationRecievingTime;
//    public NotifIcon NotificationIcons;

//    private int totalSeconds;

//    private void Start()
//    {
//        totalSeconds = NotificationRecievingTime.Days * 24 * 60 * 60 +
//                       NotificationRecievingTime.Hours * 60 * 60 +
//                       NotificationRecievingTime.Minutes * 60 +
//                       NotificationRecievingTime.Seconds;

//#if UNITY_ANDROID
//        RequestAuthorization();
//        RegisterNotificationChannel();
//        ScheduleConsistentNotificationsAndroid(AboutNotification.NotificationTitle, AboutNotification.NotificationDescription, totalSeconds);
//#endif
//#if UNITY_IOS
//        StartCoroutine(RequestAuthorizationForIOS());
//        iOSNotificationCenter.RemoveAllScheduledNotifications();
//        ScheduleRepeatingNotificationsIOS(AboutNotification.NotificationTitle, AboutNotification.NotificationDescription, AboutNotification.NotificationSubTitleForIOS, totalSeconds);
//#endif
//    }

//#if UNITY_ANDROID
//    public void RequestAuthorization()
//    {
//        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
//        {
//            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
//            Debug.Log("Permission Granted");
//        }
//    }

//    public void RegisterNotificationChannel()
//    {
//        AndroidNotificationCenter.CancelAllDisplayedNotifications();

//        var channel = new AndroidNotificationChannel
//        {
//            Id = "default_channel",
//            Name = "Default Channel",
//            Importance = Importance.Default,
//            Description = "Generic notification"
//        };
//        AndroidNotificationCenter.RegisterNotificationChannel(channel);
//        Debug.Log("Notification Channel Registered");
//    }

//    public void ScheduleConsistentNotificationsAndroid(string title, string text, int intervalSeconds)
//    {
//        AndroidNotificationCenter.CancelAllScheduledNotifications(); // Clear any previous notifications

//        int maxNotifications = 10; // Number of notifications to schedule
//        System.DateTime currentTime = System.DateTime.Now;

//        int currentseconds = 0;

//        for (int i = 1; i <= maxNotifications; i++)
//        {
//            // Schedule each notification at the correct interval
//            var notification = new AndroidNotification
//            {
//                Title = title,
//                Text = text,
//                FireTime = currentTime.AddSeconds(intervalSeconds + currentseconds), // Consistent interval
//                SmallIcon = NotificationIcons.SmallIconName,
//                LargeIcon = NotificationIcons.LargeIconName
//            };

//            int uniqueNotificationId = currentTime.GetHashCode() + i; // Generate a unique integer ID for each notification
//            AndroidNotificationCenter.SendNotificationWithExplicitID(notification, "default_channel", uniqueNotificationId);

//            Debug.Log($"Notification {i} scheduled for {notification.FireTime} with ID {uniqueNotificationId}.");

//            currentseconds = currentseconds + intervalSeconds;
//        }
//    }

//#endif

//#if UNITY_IOS
//    public IEnumerator RequestAuthorizationForIOS()
//    {
//        using var req = new AuthorizationRequest(
//            authorizationOption: AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound,
//            registerForRemoteNotifications: true
//        );
//        while (!req.IsFinished)
//        {
//            yield return null;
//        }

//        Debug.Log("iOS Notification Authorization Status: " + req.Granted);
//    }

//    public void ScheduleRepeatingNotificationsIOS(string title, string body, string subtitle, int fireTimeInSeconds)
//    {
//        iOSNotificationCenter.RemoveAllScheduledNotifications(); // Clear previous notifications

//        // Create the repeating notification trigger
//        var timeTrigger = new iOSNotificationTimeIntervalTrigger
//        {
//            TimeInterval = System.TimeSpan.FromSeconds(fireTimeInSeconds),
//            Repeats = true // Enable repeating notifications
//        };

//        var notification = new iOSNotification
//        {
//            Identifier = System.Guid.NewGuid().ToString(), // Unique identifier for each notification
//            Title = title,
//            Body = body,
//            Subtitle = subtitle,
//            ShowInForeground = true,
//            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
//            Trigger = timeTrigger
//        };

//        iOSNotificationCenter.ScheduleNotification(notification);
//        Debug.Log("Repeating Notification Scheduled for iOS");
//    }
//#endif
//}







//using System.Collections;
//using System.Collections.Generic;
//#if UNITY_IOS
//using Unity.Notifications.iOS;
//#endif
//#if UNITY_ANDROID
//using Unity.Notifications.Android;
//using UnityEngine.Android;
//#endif
//using UnityEngine;

//public class MobileMonetizationPro_NotificationController : MonoBehaviour
//{
//    [System.Serializable]
//    public class NotifDesc
//    {
//        public string NotificationTitle;
//        public string NotificationDescription;
//        public string NotificationSubTitleForIOS;
//    }

//    [System.Serializable]
//    public class Notiftime
//    {
//        public int Days;
//        public int Hours;
//        public int Minutes;
//        public int Seconds;
//    }

//    [System.Serializable]
//    public class NotifIcon
//    {
//        public string SmallIconName = "small";
//        public string LargeIconName = "large";
//    }

//    public NotifDesc AboutNotification;
//    public Notiftime NotificationRecievingTime;
//    public NotifIcon NotificationIcons;

//    private int totalSeconds;

//    private void Start()
//    {
//        totalSeconds = NotificationRecievingTime.Days * 24 * 60 * 60 +
//                       NotificationRecievingTime.Hours * 60 * 60 +
//                       NotificationRecievingTime.Minutes * 60 +
//                       NotificationRecievingTime.Seconds;

//#if UNITY_ANDROID
//        RequestAuthorization();
//        RegisterNotificationChannel();
//        ScheduleRepeatingNotificationsAndroid(AboutNotification.NotificationTitle, AboutNotification.NotificationDescription, totalSeconds);
//#endif
//#if UNITY_IOS
//        StartCoroutine(RequestAuthorizationForIOS());
//        iOSNotificationCenter.RemoveAllScheduledNotifications();
//        ScheduleRepeatingNotificationsIOS(AboutNotification.NotificationTitle, AboutNotification.NotificationDescription, AboutNotification.NotificationSubTitleForIOS, totalSeconds);
//#endif
//    }

//#if UNITY_ANDROID
//    public void RequestAuthorization()
//    {
//        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
//        {
//            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
//            Debug.Log("Permission Granted");
//        }
//    }

//    public void RegisterNotificationChannel()
//    {
//        AndroidNotificationCenter.CancelAllDisplayedNotifications();

//        var channel = new AndroidNotificationChannel
//        {
//            Id = "default_channel",
//            Name = "Default Channel",
//            Importance = Importance.Default,
//            Description = "Generic notification"
//        };
//        AndroidNotificationCenter.RegisterNotificationChannel(channel);
//        Debug.Log("Notification Channel Registered");
//    }

//    public void ScheduleRepeatingNotificationsAndroid(string title, string text, int fireTimeInSeconds)
//    {
//        AndroidNotificationCenter.CancelAllScheduledNotifications(); // Cancel previous notifications to avoid duplicates

//        // Schedule the repeating notification
//        var notification = new AndroidNotification
//        {
//            Title = title,
//            Text = text,
//            FireTime = System.DateTime.Now.AddSeconds(fireTimeInSeconds),
//            RepeatInterval = System.TimeSpan.FromSeconds(fireTimeInSeconds), // Repeats every "totalSeconds"
//            SmallIcon = NotificationIcons.SmallIconName,
//            LargeIcon = NotificationIcons.LargeIconName
//        };

//        var id = AndroidNotificationCenter.SendNotification(notification, "default_channel");

//        var status = AndroidNotificationCenter.CheckScheduledNotificationStatus(id);
//        if (status == NotificationStatus.Scheduled)
//        {
//            Debug.Log("Repeating Notification Scheduled Successfully");
//        }
//        else
//        {
//            Debug.LogWarning("Failed to Schedule Notification");
//        }
//    }
//#endif

//#if UNITY_IOS
//    public IEnumerator RequestAuthorizationForIOS()
//    {
//        using var req = new AuthorizationRequest(
//            authorizationOption: AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound,
//            registerForRemoteNotifications: true
//        );
//        while (!req.IsFinished)
//        {
//            yield return null;
//        }

//        Debug.Log("iOS Notification Authorization Status: " + req.Granted);
//    }

//    public void ScheduleRepeatingNotificationsIOS(string title, string body, string subtitle, int fireTimeInSeconds)
//    {
//        iOSNotificationCenter.RemoveAllScheduledNotifications(); // Cancel previous notifications

//        // Create the repeating notification trigger
//        var timeTrigger = new iOSNotificationTimeIntervalTrigger
//        {
//            TimeInterval = System.TimeSpan.FromSeconds(fireTimeInSeconds),
//            Repeats = true // Enable repeating notifications
//        };

//        var notification = new iOSNotification
//        {
//            Identifier = System.Guid.NewGuid().ToString(), // Unique identifier for each notification
//            Title = title,
//            Body = body,
//            Subtitle = subtitle,
//            ShowInForeground = true,
//            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
//            Trigger = timeTrigger
//        };

//        iOSNotificationCenter.ScheduleNotification(notification);
//        Debug.Log("Repeating Notification Scheduled for iOS");
//    }
//#endif
//}




//using System.Collections;
//using System.Collections.Generic;
//#if UNITY_IOS
//using Unity.Notifications.iOS;
//#endif
//#if UNITY_ANDROID
//using Unity.Notifications.Android;
//using UnityEngine.Android;
//#endif
//using UnityEngine;

//public class MobileMonetizationPro_NotificationController : MonoBehaviour
//{
//    [System.Serializable]
//    public class NotifDesc
//    {
//        public string NotificationTitle;
//        public string NotificationDescription;
//        public string NotificationSubTitleForIOS;
//    }

//    [System.Serializable]
//    public class Notiftime
//    {
//        public int Days;
//        public int Hours;
//        public int Minutes;
//        public int Seconds;
//    }

//    [System.Serializable]
//    public class NotifIcon
//    {
//        public string SmallIconName = "small";
//        public string LargeIconName = "large";
//    }

//    public NotifDesc AboutNotification;
//    public Notiftime NotificationRecievingTime;
//    public NotifIcon NotificationIcons;

//    int totalSeconds;

//    private void Start()
//    {
//        totalSeconds = NotificationRecievingTime.Days * 24 * 60 * 60 + NotificationRecievingTime.Hours * 60 * 60 + NotificationRecievingTime.Minutes * 60 + NotificationRecievingTime.Seconds;

//#if UNITY_ANDROID
//        RequestAuthorization();
//        RegisterNotificationChannel();
//        SendNotificationForAndroid(AboutNotification.NotificationTitle, AboutNotification.NotificationDescription, totalSeconds);

//#endif
//#if UNITY_IOS
//        StartCoroutine(RequestAuthorizationForIOS());
//        iOSNotificationCenter.RemoveAllScheduledNotifications();
//        SendNotificationIOS(NotificationTitle, NotificationDescription, NotificationSubTitleForIOS, totalSeconds);
//#endif
//    }
//#if UNITY_ANDROID
//    public void RequestAuthorization()
//    {
//        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
//        {
//            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
//            Debug.Log("Permission Granted");
//        }
//    }
//    public void RegisterNotificationChannel()
//    {
//        AndroidNotificationCenter.CancelAllDisplayedNotifications();

//        var channel = new AndroidNotificationChannel()
//        {
//            Id = "default_channel",
//            Name = "Default Channel",
//            Importance = Importance.Default,
//            Description = "Generic notification"
//        };
//        AndroidNotificationCenter.RegisterNotificationChannel(channel);
//        Debug.Log("Notification Registered");
//    }

//    public void SendNotificationForAndroid(string title, string text, int fireTimeInSeconds)
//    {
//        var notification = new AndroidNotification();
//        notification.Title = title;
//        notification.Text = text;
//        notification.FireTime = System.DateTime.Now.AddSeconds(fireTimeInSeconds);

//        notification.SmallIcon = NotificationIcons.SmallIconName;
//        notification.LargeIcon = NotificationIcons.LargeIconName;

//        var id = AndroidNotificationCenter.SendNotification(notification, "default_channel");

//        if (AndroidNotificationCenter.CheckScheduledNotificationStatus(id) == NotificationStatus.Scheduled)
//        {
//            AndroidNotificationCenter.CancelAllNotifications();
//            AndroidNotificationCenter.SendNotification(notification, "default_channel");
//            Debug.Log("Notification Sent");
//        }
//    }
//#endif

//#if UNITY_IOS
//    public IEnumerator RequestAuthorizationForIOS()
//    {
//        using var req = new AuthorizationRequest(authorizationOption: AuthorizationOption.Alert | AuthorizationOption.Badge,
//            registerForRemoteNotifications: true);
//        while (!req.IsFinished)
//        {
//            yield return null;
//        }
//    }
//    public void SendNotificationIOS(string title, string body, string subtitle, int fireTimeInSeconds)
//    {
//        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
//        {
//            TimeInterval = new System.TimeSpan(hours: 0, minutes: 0, fireTimeInSeconds),
//            Repeats = false
//        };

//        var notification = new iOSNotification()
//        {
//            Identifier = "Hello",
//            Title = title,
//            Body = body,
//            Subtitle = subtitle,
//            ShowInForeground = true,
//            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
//            CategoryIdentifier = "default_category",
//            ThreadIdentifier = "thread1",            
//            Trigger = timeTrigger
//        };

//        iOSNotificationCenter.ScheduleNotification(notification);
//    }
//#endif

//}
