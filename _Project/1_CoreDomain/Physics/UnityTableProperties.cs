using UnityEngine;


// This script lives on your 3D Pool Table GameObject in the Unity Editor.
public class UnityTableProperties : MonoBehaviour, ITableProperties
{
    [Tooltip("Adjustable via the Unity Inspector by level designers.")]
    [SerializeField] private float tableFriction = 0.2f;


    // Fulfilling the interface contract
    public float GetFrictionCoefficient()
    {
        // In a real scenario, this might read data from a PhysicMaterial
        // or a ScriptableObject depending on the cloth type.
        return tableFriction;
    }
}
