// Assets/Scripts/Presentation/Physics/PocketTrigger.cs
using UnityEngine;
// VContainer;


public class PocketTrigger : MonoBehaviour
{
    private TurnContext _turnContext;


    // We inject the shared ledger that the RulesEngine will eventually read
    //[Inject]
    public void Construct(TurnContext turnContext)
    {
        _turnContext = turnContext;
    }


    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the pocket is a ball
        if (other.TryGetComponent(out UnityBallIdentity ballIdentity))
        {
            // 1. Report the event to the pure C# domain
            _turnContext.RegisterPocketedBall(ballIdentity.BallType);


            // 2. Handle the Unity visual representation (e.g., hide the ball, play a sound)
            other.gameObject.SetActive(false);

            // Note: We do NOT evaluate the rules here. We wait for the table to stop moving.
        }
    }
}