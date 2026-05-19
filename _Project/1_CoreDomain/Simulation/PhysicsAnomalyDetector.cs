// 2. Assets/Scripts/CoreDomain/Simulation/PhysicsAnomalyDetector.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PhysicsAnomalyDetector : IAnomalyDetector
{
    private const float MAX_ALLOWED_VELOCITY = 50f;
    private const float TABLE_BOUNDS_X = 10f;

    public void ValidatePhysicsState(IEnumerable<Vector3> positions, IEnumerable<Vector3> velocities)
    {
        // Check 1: Did floating-point math break? (NaN or Infinity)
        if (velocities.Any(v => float.IsNaN(v.x) || float.IsInfinity(v.x)))
        {
            throw new Exception("SIMULATION FAILED: Velocity became NaN/Infinity.");
        }


        // Check 2: Did the collision resolution fail and launch a ball at lightspeed?
        if (velocities.Any(v => v.magnitude > MAX_ALLOWED_VELOCITY))
        {
            throw new Exception("SIMULATION FAILED: Velocity exceeded absolute maximum.");
        }


        // Check 3: Did a ball phase through the physical bounds of the table?
        if (positions.Any(p => Mathf.Abs(p.x) > TABLE_BOUNDS_X))
        {
            throw new Exception("SIMULATION FAILED: Ball escaped table boundaries.");
        }
    }
}