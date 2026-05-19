// Assets/_Project/3_Presentation/Gameplay/BallVisualizer2D.cs
using UnityEngine;


public class BallVisualizer2D : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _ballSprite;
    [SerializeField] private Transform _shadowTransform; // A fake drop shadow sprite

    private Vector3 _targetPosition;

    // The visualizer listens to the Core Domain state every frame
    public void UpdateVisualState(PhysicsBall coreDomainState)
    {
        if (!coreDomainState.IsActive)
        {
            gameObject.SetActive(false);
            return;
        }


        // Translate the math coordinates to the 2D screen
        _targetPosition = CoordinateAdapter.MathToUnity2D(coreDomainState.Position);

        // Handle Ball Rotation (Rolling Effect)
        // Even in 2D, the ball must appear to roll. We rotate the sprite based on velocity.
        if (coreDomainState.Velocity.sqrMagnitude > 0.001f)
        {
            float speed = coreDomainState.Velocity.magnitude;
            Vector3 rollAxis = Vector3.Cross(coreDomainState.Velocity.normalized, Vector3.up);

            // Spin the 2D sprite to create the illusion of 3D rolling
            _ballSprite.transform.Rotate(rollAxis, speed * Time.deltaTime * 360f, Space.World);
        }
    }


    private void Update()
    {
        // Smooth interpolation so the 2D sprite glides smoothly between network ticks
        transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * 15f);
    }
}
