// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Notifications/NotificationRouter.cs
using System;
using VContainer.Unity;


public class NotificationRouter : IStartable, IDisposable
{
    private readonly IMessageBroker _broker;
    private readonly INotificationQueue _queue;


    public NotificationRouter(IMessageBroker broker, INotificationQueue queue)
    {
        _broker = broker;
        _queue = queue;
    }


    public void Start()
    {
        // Listen to various global systems
        //_broker.Subscribe<AchievementUnlockedMessage>(RouteAchievement);
        //_broker.Subscribe<MatchChallengeMessage>(RouteChallenge);
        _broker.Subscribe<RemotePushMessage>(RouteRemotePush);
    }

    private void RouteRemotePush(RemotePushMessage msg)
    {
        UnityEngine.Debug.Log($"[NotificationRouter] Received RemotePushMessage: {msg.Title}. Enqueuing to NotificationQueueService...");
        // Convert the external Push Message into our internal UI format
        _queue.Enqueue(new NotificationData
        {
            Type = enNotificationType.SystemWarning, // Or create a new type "RemotePush"
            Classification = NotificationClassification.Info,
            Layout = NotificationLayout.Standard,
            SlideIn = NotificationSlideDirection.Top,
            SlideOut = NotificationSlideDirection.Top,
            Title = msg.Title,
            Message = msg.Body,
            DisplayDurationSeconds = 6
        });


        // Optional: If msg.DeepLinkAction == "open_shop", tell your UIRouter to load the shop!
    }


    //private void RouteAchievement(AchievementUnlockedMessage msg)
    //{
    //    _queue.Enqueue(new NotificationData
    //    {
    //        Type = enNotificationType.Achievement,
    //        Title = "Achievement Unlocked!",
    //        Message = msg.Name,
    //        DisplayDurationSeconds = 4
    //    });
    //}


    //private void RouteChallenge(MatchChallengeMessage msg)
    //{
    //    _queue.Enqueue(new NotificationData
    //    {
    //        Type = NotificationType.MatchChallenge,
    //        Title = "New Challenger!",
    //        Message = $"{msg.ChallengerName} wants to play.",
    //        DisplayDurationSeconds = 8
    //    });
    //}


    public void Dispose()
    {
        //_broker.Unsubscribe<AchievementUnlockedMessage>(RouteAchievement);
        //_broker.Unsubscribe<MatchChallengeMessage>(RouteChallenge);
    }
}
