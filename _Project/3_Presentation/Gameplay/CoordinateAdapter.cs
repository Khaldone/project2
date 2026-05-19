// Assets/_Project/3_Presentation/Gameplay/CoordinateAdapter.cs
using UnityEngine;


public static class CoordinateAdapter
{
    // Converts Core Domain Math (X, 0, Z) to Unity 2D Sprites (X, Y, 0)
    public static Vector3 MathToUnity2D(Vector3 coreDomainPosition)
    {
        return new Vector3(coreDomainPosition.x, coreDomainPosition.z, 0f);
    }


    // Converts Unity 2D Mouse Clicks back to Core Domain Math
    public static Vector3 Unity2DToMath(Vector3 unityPosition)
    {
        return new Vector3(unityPosition.x, 0f, unityPosition.y);
    }
}
