// Inside Assets/Scripts/CoreDomain/Physics/ITrajectoryRaycaster.cs

using UnityEngine;
public interface ITrajectoryRaycaster
{
    Vector3[] CalculatePredictedPath(Vector3 origin, Vector3 direction, float force);
}
