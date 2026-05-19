using NUnit.Framework;
using NSubstitute;
using UnityEngine;

public class TrajectoryCalculatorTests
{
    [Test]
    public void CalculatePath_StraightHitOnWall_BouncesDirectlyBack()
    {
        // ARRANGE: Setup the mathematical simulation
        var mockCaster = Substitute.For<IPhysicsCaster>();

        Vector3 startPos = new Vector3(0, 0, 0);
        Vector3 forward = new Vector3(0, 0, 1); // Moving straight forward on Z

        // We instruct the mock to pretend it hit a flat wall exactly 5 units away
        var wallHit = new PhysicsHitData
        {
            DidHit = true,
            HitPoint = new Vector3(0, 0, 5),
            Normal = new Vector3(0, 0, -1) // Wall is facing back at us
        };


        // Tell the mock to return our fake hit data when the first cast happens
        mockCaster.CastSphere(startPos, forward, 0.5f, 10f).Returns(wallHit);


        var calculator = new TrajectoryCalculator(mockCaster);


        // ACT: Run the custom physics logic
        Vector3[] path = calculator.CalculatePath(startPos, forward, 0.5f);


        // ASSERT: Prove the math works
        Assert.AreEqual(new Vector3(0, 0, 0), path[0]); // Start
        Assert.AreEqual(new Vector3(0, 0, 5), path[1]); // Hits the wall


        // Since we hit straight on, the reflection should bounce straight back.
        // We verify the math calculated a negative Z trajectory.
        Assert.IsTrue(path[2].z < path[1].z, "The reflection point should be moving backward on the Z axis.");
    }
}
