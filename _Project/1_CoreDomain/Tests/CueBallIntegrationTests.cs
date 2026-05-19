using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


public class CueBallIntegrationTests
{
    [UnityTest]
    public IEnumerator CueBallPrefab_WhenVelocityApplied_PhysicallyMoves()
    {
        // ARRANGE: Load the actual prefab from your Resources or Addressables
        GameObject cueBallPrefab = Resources.Load<GameObject>("Prefabs/CueBall");
        GameObject instance = Object.Instantiate(cueBallPrefab, Vector3.zero, Quaternion.identity);

        CueBallController controller = instance.GetComponent<CueBallController>();

        // Assert the prefab is set up correctly
        Assert.IsNotNull(controller, "CueBall prefab is missing the CueBallController script.");


        // ACT: Apply velocity
        Vector3 testVelocity = new Vector3(0, 0, 5f);
        controller.ApplyVelocity(testVelocity);


        // Yield for one Unity physics frame so the Rigidbody updates
        yield return new WaitForFixedUpdate();


        // ASSERT: Verify the Unity physics engine actually moved the object
        Assert.IsTrue(instance.transform.position.z > 0f, "The ball did not move forward after velocity was applied.");


        // CLEANUP
        Object.Destroy(instance);
    }
}
