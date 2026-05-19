using UnityEngine;


// This lives in your Presentation .asmdef
public class UnityPhysicsCaster : MonoBehaviour, IPhysicsCaster
{
    [SerializeField] private LayerMask collisionLayers; // { get; private set; }
    public LayerMask CollisionLayers { get; private set; }
    public PhysicsHitData CastSphere(Vector3 origin, Vector3 direction, float radius, float maxDistance)
    {
        PhysicsHitData data = new PhysicsHitData();


        // Use the actual Unity Physics engine
        if (Physics.SphereCast(origin, radius, direction, out RaycastHit hit, maxDistance, collisionLayers))
        {
            data.DidHit = true;
            data.HitPoint = hit.point;
            data.Normal = hit.normal;
        }


        return data;
    }
}
