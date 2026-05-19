#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioBus))]
public class AudioBusEditor : Editor
{
    private SerializedProperty _busName;
    private SerializedProperty _mixerGroup;
    private SerializedProperty _baseVolume;
    private SerializedProperty _duckingSpeed;
    private SerializedProperty _priority;
    private SerializedProperty _duckingTargets;

    private void OnEnable()
    {
        _busName = serializedObject.FindProperty("_busName");
        _mixerGroup = serializedObject.FindProperty("_mixerGroup");
        _baseVolume = serializedObject.FindProperty("_baseVolume");
        _duckingSpeed = serializedObject.FindProperty("_duckingSpeed");
        _priority = serializedObject.FindProperty("_priority");
        _duckingTargets = serializedObject.FindProperty("_duckingTargets");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_busName);
        EditorGUILayout.PropertyField(_mixerGroup);
        EditorGUILayout.PropertyField(_baseVolume);
        EditorGUILayout.PropertyField(_priority);
        EditorGUILayout.PropertyField(_duckingSpeed);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Ducking Configuration", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "When this bus has active audio, it will duck the target buses by the specified amounts. Higher priority buses duck first.",
            MessageType.Info
        );

        DrawDuckingTargets();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawDuckingTargets()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Ducking Targets ({_duckingTargets.arraySize})", EditorStyles.boldLabel);
        
        if (GUILayout.Button("+", GUILayout.Width(30)))
        {
            _duckingTargets.InsertArrayElementAtIndex(_duckingTargets.arraySize);
        }
        EditorGUILayout.EndHorizontal();

        if (_duckingTargets.arraySize == 0)
        {
            EditorGUILayout.HelpBox("No ducking targets configured", MessageType.None);
        }

        for (int i = 0; i < _duckingTargets.arraySize; i++)
        {
            DrawDuckingTarget(_duckingTargets.GetArrayElementAtIndex(i), i);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawDuckingTarget(SerializedProperty targetProp, int index)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Target {index + 1}", EditorStyles.boldLabel);
        
        if (GUILayout.Button("×", GUILayout.Width(20)))
        {
            _duckingTargets.DeleteArrayElementAtIndex(index);
            return;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel++;

        var busNameProp = targetProp.FindPropertyRelative("BusName");
        var duckAmountProp = targetProp.FindPropertyRelative("DuckAmount");
        var reductionModeProp = targetProp.FindPropertyRelative("ReductionMode");
        var useFilterProp = targetProp.FindPropertyRelative("UseNameFilter");
        var filterNamesProp = targetProp.FindPropertyRelative("FilterNames");
        var isWhitelistProp = targetProp.FindPropertyRelative("IsWhitelist");

        EditorGUILayout.PropertyField(busNameProp, new GUIContent("Target Bus"));
        EditorGUILayout.Slider(duckAmountProp, 0f, 1f, new GUIContent("Duck Amount"));
        EditorGUILayout.PropertyField(reductionModeProp, new GUIContent("Reduction Mode"));

        if (reductionModeProp.enumValueIndex == (int)DuckingReductionMode.Relative)
        {
            EditorGUILayout.HelpBox("Relative: Reduces by percentage of current volume", MessageType.None);
        }
        else
        {
            EditorGUILayout.HelpBox("Absolute: Reduces by fixed amount", MessageType.None);
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(useFilterProp, new GUIContent("Use Name Filter"));

        if (useFilterProp.boolValue)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(isWhitelistProp, new GUIContent("Is Whitelist"));
            EditorGUILayout.HelpBox(
                isWhitelistProp.boolValue 
                    ? "Only duck when sound name contains these strings" 
                    : "Duck EXCEPT when sound name contains these strings",
                MessageType.None
            );

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter Names", EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                filterNamesProp.InsertArrayElementAtIndex(filterNamesProp.arraySize);
            }
            EditorGUILayout.EndHorizontal();

            for (int j = 0; j < filterNamesProp.arraySize; j++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(filterNamesProp.GetArrayElementAtIndex(j), GUIContent.none);
                if (GUILayout.Button("×", GUILayout.Width(20)))
                {
                    filterNamesProp.DeleteArrayElementAtIndex(j);
                    j--;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }
}
#endif