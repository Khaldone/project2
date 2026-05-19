// 1. The Pure C# Data Payload
// Notice we use our own struct, not Unity's RaycastHit, to protect our IP layer.
using UnityEngine;
public struct PhysicsHitData
{
    public bool DidHit;
    public Vector3 HitPoint;
    public Vector3 Normal; // Crucial for calculating bounce angles
}


// 2. The Abstraction
public interface IPhysicsCaster
{
    PhysicsHitData CastSphere(Vector3 origin, Vector3 direction, float radius, float maxDistance);
}


// 3. The Core IP Logic (The target of our test)
public class TrajectoryCalculator
{
    private readonly IPhysicsCaster _physicsCaster;


    public TrajectoryCalculator(IPhysicsCaster physicsCaster)
    {
        _physicsCaster = physicsCaster;
    }


    // Calculates a simple 1-bounce trajectory
    public Vector3[] CalculatePath(Vector3 startPos, Vector3 direction, float radius)
    {
        var points = new Vector3[3];
        points[0] = startPos;


        // Step 1: Cast the first ray
        var firstHit = _physicsCaster.CastSphere(startPos, direction, radius, 10f);

        if (firstHit.DidHit)
        {
            points[1] = firstHit.HitPoint;


            // Step 2: Calculate the reflection angle based on the cushion's normal
            // Vector reflection formula: R = V - 2(V * N)N
            float dotProduct = (direction.x * firstHit.Normal.x) + (direction.y * firstHit.Normal.y) + (direction.z * firstHit.Normal.z);
            Vector3 reflection = new Vector3(
                direction.x - 2 * dotProduct * firstHit.Normal.x,
                direction.y - 2 * dotProduct * firstHit.Normal.y,
                direction.z - 2 * dotProduct * firstHit.Normal.z
            );


            // Step 3: Cast the second ray from the bounce point
            var secondHit = _physicsCaster.CastSphere(firstHit.HitPoint, reflection, radius, 10f);
            points[2] = secondHit.DidHit ? secondHit.HitPoint : firstHit.HitPoint + (reflection * 5f);
        }


        return points;
    }
}
