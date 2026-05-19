// Assets/_Project/3_Presentation/UI_GameArena/TrajectoryVisualizer.cs
using UnityEngine;


public class TrajectoryVisualizer : MonoBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private LineRenderer _mainAimLine;
    [SerializeField] private LineRenderer _targetDeflectionLine;
    [SerializeField] private LineRenderer _cueTangentLine;
    [SerializeField] private Transform _ghostBallMesh;


    public void UpdateVisuals(AimTrajectoryData data)
    {
        // 1. Draw the line from the real cue ball to the ghost ball
        _mainAimLine.SetPosition(0, data.CueBallStart);
        _mainAimLine.SetPosition(1, data.GhostBallPosition);


        if (data.DidHitTarget)
        {
            _ghostBallMesh.gameObject.SetActive(true);
            _ghostBallMesh.position = data.GhostBallPosition;


            // 2. Draw the path the target ball will take
            _targetDeflectionLine.gameObject.SetActive(true);
            _targetDeflectionLine.SetPosition(0, data.GhostBallPosition); // Starts at the target ball
            _targetDeflectionLine.SetPosition(1, data.TargetBallDeflectionEnd);


            // 3. Draw the path the cue ball will take after impact
            _cueTangentLine.gameObject.SetActive(true);
            _cueTangentLine.SetPosition(0, data.GhostBallPosition);
            _cueTangentLine.SetPosition(1, data.CueBallTangentEnd);
        }
        else
        {
            _ghostBallMesh.gameObject.SetActive(false);
            _targetDeflectionLine.gameObject.SetActive(false);
            _cueTangentLine.gameObject.SetActive(false);
        }
    }
}