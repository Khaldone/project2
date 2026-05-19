// Assets/_Project/CoreDomain/Physics/AimPredictionService.cs
using UnityEngine; // Using Unity's Vector3 strictly for math structs

public struct AimPredictionResult
{
    public bool HitTarget;
    public Vector3 GhostBallPosition;
    public Vector3 TargetBallTrajectory;
    public Vector3 CueBallDeflection;
}

public class AimPredictionService
{
    private readonly float _ballRadius = 0.0285f; // Standard pool ball
    private readonly LayerMask _ballLayer;
    private readonly LayerMask _railLayer;

    public AimPredictionService(LayerMask ballLayer, LayerMask railLayer)
    {
        _ballLayer = ballLayer;
        _railLayer = railLayer;
    }

    public AimPredictionResult PredictTrajectory(Vector3 cueBallPos, float aimAngleDegrees)
    {
        var result = new AimPredictionResult();

        // 1. Convert Angle to a Forward Vector
        float angleRad = aimAngleDegrees * Mathf.Deg2Rad;
        Vector3 aimDirection = new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad));


        // 2. The SphereCast (Finding the Ghost Ball)
        // We cast a sphere the exact size of the cue ball forward along the aim line
        if (Physics.SphereCast(cueBallPos, _ballRadius, aimDirection, out RaycastHit hit, 5.0f, _ballLayer | _railLayer))
        {
            result.HitTarget = true;

            // The point in space where the cue ball will be the moment it touches the object
            result.GhostBallPosition = cueBallPos + (aimDirection * hit.distance);


            // Did we hit another ball, or a wall?
            if ((_ballLayer.value & (1 << hit.collider.gameObject.layer)) > 0)
            {
                // THE MATH: Target moves along the Normal, Cue deflects along the Tangent

                // Normal Vector (N): From Ghost Ball center to Target Ball center
                Vector3 targetBallCenter = hit.collider.transform.position;
                Vector3 normal = (targetBallCenter - result.GhostBallPosition).normalized;

                // Tangent Vector (T): Perpendicular to the Normal
                // In 2D/top-down 3D, we can find the tangent by crossing the normal with the UP vector
                Vector3 tangent = Vector3.Cross(normal, Vector3.up).normalized;


                // Ensure the tangent goes the "forward" direction relative to the shot
                if (Vector3.Dot(aimDirection, tangent) < 0)
                {
                    tangent = -tangent;
                }


                result.TargetBallTrajectory = result.GhostBallPosition + (normal * 0.5f); // Draw 0.5m out
                result.CueBallDeflection = result.GhostBallPosition + (tangent * 0.5f);
            }
            else
            {
                // We hit a rail. Calculate the bounce reflection.
                Vector3 reflected = Vector3.Reflect(aimDirection, hit.normal);
                result.CueBallDeflection = result.GhostBallPosition + (reflected * 0.5f);
                result.TargetBallTrajectory = Vector3.zero; // Walls don't move
            }
        }

        return result;
    }
}