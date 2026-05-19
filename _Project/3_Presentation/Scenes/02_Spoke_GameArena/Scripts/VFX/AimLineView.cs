using UnityEngine;


public class AimLineView : MonoBehaviour
{
    [SerializeField] private LineRenderer _mainLine;
    [SerializeField] private LineRenderer _deflectionLine;
    [SerializeField] private GameObject _ghostBallVisual;

    public void RenderPrediction(AimPredictionResult result)
    {
        if (result.HitTarget)
        {
            _ghostBallVisual.SetActive(true);
            _ghostBallVisual.transform.position = result.GhostBallPosition;

            // Draw lines
            _mainLine.SetPosition(1, result.GhostBallPosition);
            _deflectionLine.SetPositions(new[] { result.GhostBallPosition, result.CueBallDeflection });
        }
    }
}