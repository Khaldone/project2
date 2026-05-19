// Assets/_Project/3_Presentation/Gameplay/CueVisualizer.cs


using UnityEngine;

public class CueVisualizer : MonoBehaviour
{
    [SerializeField] private Transform _cueMeshTransform;
    [SerializeField] private float _maxPullBackDistance = 0.2f;

    // Called every frame by the UI input handler
    public void UpdateVisualPullback(float normalizedPower)
    {
        // Physically move the cue stick backward relative to its forward vector
        Vector3 localPos = _cueMeshTransform.localPosition;
        localPos.z = -normalizedPower * _maxPullBackDistance;
        _cueMeshTransform.localPosition = localPos;

        // Add a slight "vibration" effect if power is at 100%
        if (normalizedPower >= 0.98f)
        {
            _cueMeshTransform.localPosition += Random.insideUnitSphere * 0.002f;
        }
    }
}
