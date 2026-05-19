// Assets/_Project/CoreDomain/Networking/ChallengeListener.cs
using System;
using System.Threading.Tasks;


public class ChallengeListener
{
    private readonly INotificationQueue _queue;
    private readonly IMatchmakingService _networkService;


    public ChallengeListener(INotificationQueue queue, IMatchmakingService networkService)
    {
        _queue = queue;
        _networkService = networkService;
    }


    // Triggered by your backend SDK when a friend sends an invite
    public void OnIncomingChallengeReceived(string challengerName, string photonRoomId)
    {
        // 1. Build the envelope
        var challengeData = new NotificationData
        {
            Type = enNotificationType.MatchChallenge,
            Title = "Match Challenge!",
            Message = $"{challengerName} wants to play billiards.",
            DisplayDurationSeconds = 8,

            // 2. Wire the callback directly to our network logic
            OnInteractionResolved = (accepted) => HandlePlayerDecision(accepted, photonRoomId)
        };


        // 3. Drop it in the global queue
        _queue.Enqueue(challengeData);
    }


    private async void HandlePlayerDecision(bool accepted, string roomId)
    {
        if (accepted)
        {
            // Transition out of current state and connect to the friend's room
            //await _networkService.JoinOrCreateMatchAsync(roomId);
        }
        else
        {
            // Silently decline via backend so the friend knows we are busy
            // _backendService.DeclineInvite(roomId);
        }
    }
}
