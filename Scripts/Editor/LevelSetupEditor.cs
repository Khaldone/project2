// Assets/Scripts/Editor/LevelSetupEditor.cs
using UnityEditor;
using UnityEngine;


// Tell Unity this script modifies how the LevelSetupAuthoring component looks
[CustomEditor(typeof(LevelSetupAuthoring))]
public class LevelSetupEditor : Editor
{
    private void OnSceneGUI()
    {
        LevelSetupAuthoring setup = (LevelSetupAuthoring)target;


        // 1. Draw a visual Cue Ball Handle in the 3D Scene
        Handles.color = Color.white;
        EditorGUI.BeginChangeCheck();

        // Creates a physical 3D handle the designer can drag with their mouse
        Vector3 newCuePos = Handles.PositionHandle(setup.cueBallStart, Quaternion.identity);

        // Draw a visual sphere to represent the ball
        Handles.DrawWireDisc(newCuePos, Vector3.up, 0.5f);
        Handles.Label(newCuePos + Vector3.up, "Cue Ball Start");


        if (EditorGUI.EndChangeCheck())
        {
            // Record the Undo state so the designer can press Ctrl+Z
            Undo.RecordObject(setup, "Move Cue Ball");
            setup.cueBallStart = newCuePos;
        }


        // 2. Draw the Rack Area Handle
        Handles.color = Color.yellow;
        EditorGUI.BeginChangeCheck();
        Vector3 newRackPos = Handles.PositionHandle(setup.rackCenter, Quaternion.identity);

        // Draw a triangle to represent the rack
        Vector3 p1 = newRackPos + new Vector3(0, 0, 1);
        Vector3 p2 = newRackPos + new Vector3(-1, 0, -1);
        Vector3 p3 = newRackPos + new Vector3(1, 0, -1);
        Handles.DrawPolyLine(p1, p2, p3, p1);
        Handles.Label(newRackPos + Vector3.up, "Rack Center");


        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(setup, "Move Rack");
            setup.rackCenter = newRackPos;
        }


        // 3. Real-time Mathematical Validation Feedback
        // We use the pure C# logic we tested earlier to turn the screen red if the designer makes a mistake!
        LevelDefinition tempDef = setup.ExportToCoreDomain("temp");
        if (!tempDef.IsSetupValid(10f, 5f))
        {
            Handles.color = Color.red;
            Handles.Label(setup.transform.position + (Vector3.up * 3), "INVALID SETUP: CHECK RULES");

            // Optionally, snap the handle back to a valid position here
        }
    }
}
