// Assets/Scripts/Presentation/State/UnityBallTracker.cs
using UnityEngine;
using System.Collections.Generic;
public class UnityBallTracker : MonoBehaviour, IBallStateTracker
{
    // Populated dynamically as balls are spawned or pocketed
    public List<Rigidbody> ActiveBalls = new List<Rigidbody>();


    public IEnumerable<float> GetActiveBallSpeeds()
    {
        foreach (var ball in ActiveBalls)
        {
            if (ball != null)
            {
                // We simply extract the velocity magnitude and pass it to the C# domain
                yield return ball.linearVelocity.magnitude;
            }
        }
    }
}
