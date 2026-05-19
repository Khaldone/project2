// 1. Assets/Scripts/CoreDomain/Simulation/IAnomalyDetector.cs
using System.Collections.Generic;
using UnityEngine;

public interface IAnomalyDetector
{
    // Checks the state of all balls and throws an exception if physics broke
    void ValidatePhysicsState(IEnumerable<Vector3> ballPositions, IEnumerable<Vector3> ballVelocities);
}