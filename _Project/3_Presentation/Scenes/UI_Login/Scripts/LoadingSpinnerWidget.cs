// Attached to: Spinner_Graphic
using UnityEngine;

public class LoadingSpinnerWidget : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = -200f; // Degrees per second

    private void Update()
    {
        // Simple, localized visual rotation
        transform.Rotate(0, 0, _rotationSpeed * Time.deltaTime);
    }
}