// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/Gameplay/BallCollisionHandler.cs
using UnityEngine;
using VContainer;

public class BallCollisionHandler : MonoBehaviour
{
    private IAudioService _audioService;

    // VContainer automatically looks up to the Hub and passes the Audio Service down
    [Inject]
    public void Construct(IAudioService audioService)
    {
        _audioService = audioService;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PoolBall"))
        {
            // Calculate collision magnitude here...

            // Play the sound!
            _audioService.PlaySFX("ball_clack_hard");
        }
    }
}