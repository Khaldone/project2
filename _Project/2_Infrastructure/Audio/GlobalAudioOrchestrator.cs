// 2_Infrastructure/Audio/GlobalAudioOrchestrator.cs
using System;
using VContainer.Unity;


public class GlobalAudioOrchestrator : IStartable, IDisposable
{
    private readonly IAudioService _audioWrapper;
    private readonly IMessageBroker_New _messageBroker;
    private IDisposable _pocketSubscription; // Holds the memory token!


    public GlobalAudioOrchestrator(IAudioService audioWrapper, IMessageBroker_New messageBroker)
    {
        _audioWrapper = audioWrapper;
        _messageBroker = messageBroker;
    }


    public void Start()
    {
        // Start listening
        //_pocketSubscription = _messageBroker.Subscribe<BallPocketedMessage>(OnBallPocketed);
    }


    //private void OnBallPocketed(BallPocketedMessage msg)
    //{
    //    // Play the heavy thud of a ball hitting the leather pocket
    //    _audioWrapper.PlaySFX("sfx_ball_pocketed");
    //}


    public void Dispose()
    {
        // CRITICAL: Clean up the memory when the game closes
        _pocketSubscription?.Dispose();
    }
}