/*#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SFXLayer))]
public class SFXLayerEditor : Editor
{
    private Dictionary<int, bool> _clipFoldouts = new Dictionary<int, bool>();
    private bool _showPriorityGroups = true;
    private Vector2 _scrollPos;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultProperties();
        EditorGUILayout.Space(10);
        DrawClipsSection();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawDefaultProperties()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("LayerName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Bus"));
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Priority Fade Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("PriorityFadeSteps"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("FadeSpeed"));
    }

    private void DrawClipsSection()
    {
        var clipsProperty = serializedObject.FindProperty("Clips");
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Audio Clips ({clipsProperty.arraySize})", EditorStyles.boldLabel);
        
        _showPriorityGroups = GUILayout.Toggle(_showPriorityGroups, "Group by Priority", GUILayout.Width(130));
        
        if (GUILayout.Button("+", GUILayout.Width(30)))
        {
            clipsProperty.InsertArrayElementAtIndex(clipsProperty.arraySize);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        if (_showPriorityGroups)
        {
            DrawClipsGroupedByPriority(clipsProperty);
        }
        else
        {
            DrawClipsList(clipsProperty);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawClipsGroupedByPriority(SerializedProperty clipsProperty)
    {
        var layer = target as SFXLayer;
        if (layer == null || layer.Clips.Count == 0)
        {
            EditorGUILayout.HelpBox("No clips added yet", MessageType.Info);
            return;
        }

        var grouped = layer.Clips
            .Select((clip, index) => new { clip, index })
            .GroupBy(x => x.clip.Priority)
            .OrderByDescending(g => g.Key);

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.MaxHeight(600));

        foreach (var group in grouped)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Priority {group.Key} ({group.Count()} clips)", EditorStyles.boldLabel);
            bool breakOut = false;
            
            foreach (var item in group)
            {
                if (!DrawClipElement(clipsProperty, item.index))
                {
                    breakOut = true;
                    break;
                }
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
            
            if (breakOut)
                break;
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawClipsList(SerializedProperty clipsProperty)
    {
        if (clipsProperty.arraySize == 0)
        {
            EditorGUILayout.HelpBox("No clips added yet", MessageType.Info);
            return;
        }

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.MaxHeight(600));

        for (int i = 0; i < clipsProperty.arraySize; i++)
        {
            if(!DrawClipElement(clipsProperty, i))
                break;
        }

        EditorGUILayout.EndScrollView();
    }

    private bool DrawClipElement(SerializedProperty clipsProperty, int index)
    {
        var clipProperty = clipsProperty.GetArrayElementAtIndex(index);
        
        if (!_clipFoldouts.ContainsKey(index))
            _clipFoldouts[index] = false;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        
        _clipFoldouts[index] = EditorGUILayout.Foldout(_clipFoldouts[index], 
            clipProperty.FindPropertyRelative("Name").stringValue, true);
        
        if (GUILayout.Button("×", GUILayout.Width(20)))
        {
            clipsProperty.DeleteArrayElementAtIndex(index);
            return false;
        }
        
        EditorGUILayout.EndHorizontal();

        if (_clipFoldouts[index])
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(clipProperty.FindPropertyRelative("Name"));
            EditorGUILayout.PropertyField(clipProperty.FindPropertyRelative("Clip"));
            EditorGUILayout.PropertyField(clipProperty.FindPropertyRelative("Volume"));
            EditorGUILayout.PropertyField(clipProperty.FindPropertyRelative("Loop"));
            EditorGUILayout.PropertyField(clipProperty.FindPropertyRelative("Priority"));
            EditorGUILayout.PropertyField(clipProperty.FindPropertyRelative("PitchOffsetMinMax"));
            EditorGUILayout.PropertyField(clipProperty.FindPropertyRelative("BypassPriorityFade"));
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        return true;
    }
}
#endif*/