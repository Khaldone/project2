// 1. Assets/_Project/CoreDomain/Notifications/NotificationData.cs
using System;

public enum enNotificationType
{
    Achievement,
    MatchChallenge,
    SystemWarning
}

public enum NotificationClassification
{
    Info,       // e.g. Blue
    Success,    // e.g. Green
    Error,      // e.g. Red
    Timeout     // e.g. Brown
}

public enum NotificationLayout
{
    Standard,   // 1 button (Ok)
    Actionable, // 2 buttons (Accept/Decline)
    StatusOverlay // No buttons, no close (e.g. login loading)
}

public enum NotificationSlideDirection
{
    Immediate,
    Top,
    Bottom,
    Left,
    Right
}

public struct NotificationData
{
    public enNotificationType Type;
    public NotificationClassification Classification;
    public NotificationLayout Layout;
    
    // Animation Directions
    public NotificationSlideDirection SlideIn;
    public NotificationSlideDirection SlideOut;
    
    public string Title;
    public string Message;
    public int DisplayDurationSeconds;

    // The pure C# callback.
    // True = Accepted/Ok. False = Declined/Ignored/Timeout.
    public Action<bool> OnInteractionResolved;
}
