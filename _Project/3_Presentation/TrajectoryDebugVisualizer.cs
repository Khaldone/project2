#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


// This script lives in your Presentation .asmdef
[ExecuteAlways] // Allows OnDrawGizmos to run even when the game isn't playing
public class TrajectoryDebugVisualizer : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("The layer mask for the table cushions.")]
    public LayerMask cushionLayer;


    [Header("Settings")]
    public float ballRadius = 0.5f;
    [Range(0.1f, 20f)] public float maxCastDistance = 10f;


    // We instantiate a local version of our pure C# calculator just for the Editor preview
    private TrajectoryCalculator _editorCalculator;


    private void OnEnable()
    {
        // 1. Setup the Humble Object specifically for this preview
        // var debugCaster = new UnityPhysicsCaster { CollisionLayers = cushionLayer };
        var debugCaster = new UnityPhysicsCaster { };

        // 2. Inject it into our protected IP math
        _editorCalculator = new TrajectoryCalculator(debugCaster);
    }


    private void OnDrawGizmos()
    {
        if (_editorCalculator == null) return;


        // 3. Ask the pure C# logic for the mathematically perfect path
        Vector3[] path = _editorCalculator.CalculatePath(transform.position, transform.forward, ballRadius);


        if (path == null || path.Length < 2) return;


        // 4. Draw the trajectory
        Gizmos.color = Color.cyan;


        for (int i = 0; i < path.Length - 1; i++)
        {
            // Draw the laser line connecting the points
            Gizmos.DrawLine(path[i], path[i + 1]);


            // Draw a wire sphere to simulate the ball at the bounce point
            Gizmos.DrawWireSphere(path[i], ballRadius);
        }


        // Draw the final resting position of the predicted path
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(path[path.Length - 1], ballRadius);
    }
}
#endif
