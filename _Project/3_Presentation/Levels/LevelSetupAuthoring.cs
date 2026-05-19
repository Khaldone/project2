// Assets/Scripts/Presentation/Levels/LevelSetupAuthoring.cs
using UnityEngine;

// This sits on a GameObject in your scene to hold the designer's visual tweaks
public class LevelSetupAuthoring : MonoBehaviour
{
    public Vector3 cueBallStart = new Vector3(0, 0, -5);
    public Vector3 rackCenter = new Vector3(0, 0, 5);


    // Converts the visual Unity data into our pure C# data structure
    public LevelDefinition ExportToCoreDomain(string levelId)
    {
        return new LevelDefinition
        {
            LevelId = levelId,
            CueBallStartPosition = cueBallStart,
            RackCenterPosition = rackCenter
        };
    }
}