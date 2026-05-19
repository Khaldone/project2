#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class SFXLayerClipImporter : EditorWindow
{
    private SFXLayer _targetLayer;
    private List<AudioClip> _selectedClips = new List<AudioClip>();
    private Vector2 _scrollPos;
    
    private float _defaultVolume = 1f;
    private bool _defaultLoop = false;
    private Vector2 _defaultPitch = Vector2.zero;
    private bool _defaultBypassFade = false;
    private int _defaultPriority = 0;
    
    private enum ImportPreset
    {
        Custom,
        UIClick,
        Music,
    }
    private ImportPreset _selectedPreset = ImportPreset.Custom;
    
    private bool _useClipNames = true;
    private string _namePrefix = "";
    private string _nameSuffix = "";
    private bool _removeNumbers = false;
    private bool _removeUnderscores = false;
    
    private bool _showAdvancedOptions = false;
    private bool _clearExistingClips = false;
    private bool _randomizePitch = false;
    private float _pitchVariation = 0.1f;

    [MenuItem("Tools/Audio/SFX Layer Clip Importer")]
    public static void ShowWindow()
    {
        var window = GetWindow<SFXLayerClipImporter>("SFX Clip Importer");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }

    private void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        
        DrawHeader();
        DrawTargetLayer();
        DrawClipSelection();
        DrawPresetSelector();
        DrawCommonSettings();
        DrawNamingOptions();
        DrawAdvancedOptions();
        DrawImportButton();
        
        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("SFX Layer Clip Importer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Bulk import audio clips into an SFX Layer with common settings. " +
            "Select clips from your project, choose a preset or configure manually, then import.",
            MessageType.Info
        );
        EditorGUILayout.Space(5);
    }

    private void DrawTargetLayer()
    {
        EditorGUILayout.LabelField("Target Layer", EditorStyles.boldLabel);
        
        var newLayer = (SFXLayer)EditorGUILayout.ObjectField(
            "SFX Layer", 
            _targetLayer, 
            typeof(SFXLayer), 
            false
        );
        
        if (newLayer != _targetLayer)
        {
            _targetLayer = newLayer;
        }
        
        if (_targetLayer != null)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField($"Layer Name: {_targetLayer.LayerName}");
            EditorGUILayout.LabelField($"Current Clips: {_targetLayer.Clips.Count}");
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
    }

    private void DrawClipSelection()
    {
        EditorGUILayout.LabelField("Audio Clips", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Selected Assets", GUILayout.Height(25)))
        {
            AddSelectedAudioClips();
        }
        if (GUILayout.Button("Clear List", GUILayout.Height(25)))
        {
            _selectedClips.Clear();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.HelpBox(
            "Select AudioClip assets in the Project window, then click 'Add Selected Assets'.",
            MessageType.None
        );
        
        if (_selectedClips.Count > 0)
        {
            EditorGUILayout.LabelField($"Selected Clips: {_selectedClips.Count}", EditorStyles.miniLabel);
            
            EditorGUI.indentLevel++;
            for (int i = 0; i < Mathf.Min(_selectedClips.Count, 5); i++)
            {
                if (_selectedClips[i] != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"• {_selectedClips[i].name}", EditorStyles.miniLabel);
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        _selectedClips.RemoveAt(i);
                        i--;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            if (_selectedClips.Count > 5)
            {
                EditorGUILayout.LabelField($"... and {_selectedClips.Count - 5} more", EditorStyles.miniLabel);
            }
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
    }

    private void DrawPresetSelector()
    {
        EditorGUILayout.LabelField("Import Preset", EditorStyles.boldLabel);
        
        var newPreset = (ImportPreset)EditorGUILayout.EnumPopup("Preset", _selectedPreset);
        
        if (newPreset != _selectedPreset)
        {
            _selectedPreset = newPreset;
            ApplyPreset(_selectedPreset);
        }
        
        EditorGUILayout.Space(10);
    }

    private void DrawCommonSettings()
    {
        EditorGUILayout.LabelField("Common Settings", EditorStyles.boldLabel);
        
        _defaultVolume = EditorGUILayout.Slider("Volume", _defaultVolume, 0f, 1f);
        _defaultLoop = EditorGUILayout.Toggle("Loop", _defaultLoop);
        _defaultPriority = EditorGUILayout.IntField("Priority", _defaultPriority);
        
        EditorGUILayout.LabelField("Pitch Offset Range");
        EditorGUI.indentLevel++;
        _defaultPitch.x = EditorGUILayout.FloatField("Min", _defaultPitch.x);
        _defaultPitch.y = EditorGUILayout.FloatField("Max", _defaultPitch.y);
        EditorGUI.indentLevel--;
        
        _defaultBypassFade = EditorGUILayout.Toggle("Bypass Priority Fade", _defaultBypassFade);
        
        EditorGUILayout.Space(10);
    }

    private void DrawNamingOptions()
    {
        EditorGUILayout.LabelField("Naming Options", EditorStyles.boldLabel);
        
        _useClipNames = EditorGUILayout.Toggle("Use Clip Names", _useClipNames);
        
        if (_useClipNames)
        {
            EditorGUI.indentLevel++;
            _namePrefix = EditorGUILayout.TextField("Prefix", _namePrefix);
            _nameSuffix = EditorGUILayout.TextField("Suffix", _nameSuffix);
            _removeNumbers = EditorGUILayout.Toggle("Remove Numbers", _removeNumbers);
            _removeUnderscores = EditorGUILayout.Toggle("Remove Underscores", _removeUnderscores);
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
    }

    private void DrawAdvancedOptions()
    {
        _showAdvancedOptions = EditorGUILayout.Foldout(_showAdvancedOptions, "Advanced Options", true);
        
        if (_showAdvancedOptions)
        {
            EditorGUI.indentLevel++;
            
            _clearExistingClips = EditorGUILayout.Toggle("Clear Existing Clips", _clearExistingClips);
            
            _randomizePitch = EditorGUILayout.Toggle("Randomize Pitch Per Clip", _randomizePitch);
            if (_randomizePitch)
            {
                EditorGUI.indentLevel++;
                _pitchVariation = EditorGUILayout.Slider("Variation Amount", _pitchVariation, 0f, 0.5f);
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
    }

    private void DrawImportButton()
    {
        EditorGUI.BeginDisabledGroup(_targetLayer == null || _selectedClips.Count == 0);
        
        if (GUILayout.Button("Import Clips to Layer", GUILayout.Height(40)))
        {
            ImportClipsToLayer();
        }
        
        EditorGUI.EndDisabledGroup();
        
        if (_targetLayer == null)
        {
            EditorGUILayout.HelpBox("Select a target SFX Layer to continue.", MessageType.Warning);
        }
        else if (_selectedClips.Count == 0)
        {
            EditorGUILayout.HelpBox("Add audio clips to continue.", MessageType.Warning);
        }
    }

    private void AddSelectedAudioClips()
    {
        var selected = Selection.objects.OfType<AudioClip>().ToList();
        
        if (selected.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "No Audio Clips Selected",
                "Please select one or more AudioClip assets in the Project window.",
                "OK"
            );
            return;
        }
        
        foreach (var clip in selected)
        {
            if (!_selectedClips.Contains(clip))
            {
                _selectedClips.Add(clip);
            }
        }
        
        Debug.Log($"Added {selected.Count} audio clips. Total: {_selectedClips.Count}");
    }

    private void ImportClipsToLayer()
    {
        if (_targetLayer == null || _selectedClips.Count == 0) return;
        
        Undo.RecordObject(_targetLayer, "Import Audio Clips");
        
        if (_clearExistingClips)
        {
            _targetLayer.Clips.Clear();
        }
        
        int importedCount = 0;
        
        foreach (var clip in _selectedClips)
        {
            if (clip == null) continue;
            
            var clipData = new AudioClipData
            {
                Name = ProcessClipName(clip.name),
                Clip = clip,
                Volume = _defaultVolume,
                Loop = _defaultLoop,
                PitchOffsetMinMax = _randomizePitch ? GetRandomizedPitch() : _defaultPitch,
                BypassPriorityFade = _defaultBypassFade,
                Priority = _defaultPriority
            };
            
            _targetLayer.Clips.Add(clipData);
            importedCount++;
        }
        
        EditorUtility.SetDirty(_targetLayer);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Successfully imported {importedCount} clips to {_targetLayer.LayerName}");
        
        EditorUtility.DisplayDialog(
            "Import Complete",
            $"Successfully imported {importedCount} audio clips to {_targetLayer.LayerName}.",
            "OK"
        );
        
        _selectedClips.Clear();
    }

    private string ProcessClipName(string originalName)
    {
        string result = originalName;
        
        if (_removeNumbers)
        {
            result = new string(result.Where(c => !char.IsDigit(c)).ToArray());
        }
        
        if (_removeUnderscores)
        {
            result = result.Replace("_", " ");
        }
        
        result = _namePrefix + result + _nameSuffix;
        
        return result.Trim();
    }

    private Vector2 GetRandomizedPitch()
    {
        float variation = Random.Range(0, _pitchVariation);
        return new Vector2(-variation, variation);
    }

    private void ApplyPreset(ImportPreset preset)
    {
        switch (preset)
        {
            case ImportPreset.UIClick:
                _defaultVolume = 0.7f;
                _defaultLoop = false;
                _defaultPitch = new Vector2(-0.05f, 0.05f);
                _defaultBypassFade = true;
                _defaultPriority = 100;
                _randomizePitch = false;
                break;
            
            case ImportPreset.Music:
                _defaultVolume = 0.8f;
                _defaultLoop = true;
                _defaultPitch = Vector2.zero;
                _defaultBypassFade = true;
                _defaultPriority = 0;
                _randomizePitch = false;
                break;
                
            case ImportPreset.Custom:
            default:
                break;
        }
    }
}
#endif