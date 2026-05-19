// Assets/_Project/3_Presentation/System/ApplicationLifecycleMonitor.cs
using UnityEngine;


public class ApplicationLifecycleMonitor : MonoBehaviour
{
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // The user just swiped to their home screen. The app is freezing NOW.
            //MessageBroker.Instance.Publish(new AppSuspendedMessage());
        }
        else
        {
            // The user just returned to the app 4 seconds later.
            //MessageBroker.Instance.Publish(new AppResumedMessage());
        }
    }
}