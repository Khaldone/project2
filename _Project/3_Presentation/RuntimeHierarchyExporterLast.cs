using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using System.IO;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HierarchyNodeNote_New : MonoBehaviour
{
    public string AccessTag;
    public string Description;
}

public class RuntimeHierarchyExporterLast : MonoBehaviour
{
    [Header("Export Settings")]
    public int AlignmentColumn = 70;

    [ContextMenu("Export Hierarchy & Copy Scripts")]
    public void ExportHierarchyAndScripts()
    {
#if UNITY_EDITOR
        // 1. Prompt the user to choose a destination folder on their computer
        string selectedExportDirectory = EditorUtility.SaveFolderPanel("Choose Export Location", "", "HierarchyExports");

        if (string.IsNullOrEmpty(selectedExportDirectory))
        {
            Debug.Log("[Hierarchy Exporter] Export cancelled by user.");
            return;
        }

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;

        // 2. Loop through each loaded scene
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            // 3. Create a dedicated folder for this specific scene
            string sceneFolderPath = Path.Combine(selectedExportDirectory, scene.name);
            if (!Directory.Exists(sceneFolderPath))
            {
                Directory.CreateDirectory(sceneFolderPath);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("```text");
            sb.AppendLine($"{scene.name}/");

            // Track unique script paths so we don't copy the same script multiple times per scene
            HashSet<string> scriptsToCopy = new HashSet<string>();

            GameObject[] roots = scene.GetRootGameObjects();
            for (int j = 0; j < roots.Length; j++)
            {
                Traverse(roots[j].transform, "", (j == roots.Length - 1), sb, scriptsToCopy, projectRoot);
            }

            sb.AppendLine("```");

            // 4. Save the .MD file inside the scene's folder
            string mdFilePath = Path.Combine(sceneFolderPath, $"{scene.name}_Hierarchy.md");
            File.WriteAllText(mdFilePath, sb.ToString());

            // 5. Copy the collected C# scripts into the scene's folder
            int copiedCount = 0;
            foreach (string scriptSourcePath in scriptsToCopy)
            {
                if (File.Exists(scriptSourcePath))
                {
                    string fileName = Path.GetFileName(scriptSourcePath);
                    string destinationPath = Path.Combine(sceneFolderPath, fileName);

                    // True allows overwriting if the file already exists in the destination
                    File.Copy(scriptSourcePath, destinationPath, true);
                    copiedCount++;
                }
            }

            Debug.Log($"[Hierarchy Exporter] Exported '{scene.name}' MD and copied {copiedCount} scripts to: {sceneFolderPath}");
        }
#else
        Debug.LogError("[Hierarchy Exporter] Copying source scripts requires the Unity Editor. This cannot be run in a standalone build.");
#endif
    }

    private void Traverse(Transform t, string prefix, bool isLast, StringBuilder sb, HashSet<string> scriptsToCopy, string projectRoot)
    {
        string marker = isLast ? "└── " : "├── ";
        string nodeBase = $"{prefix}{marker}{t.name}";

        // Handle Note Component
        HierarchyNodeNote_New note = t.GetComponent<HierarchyNodeNote_New>();
        if (note != null)
        {
            int paddingNeeded = Mathf.Max(1, AlignmentColumn - nodeBase.Length);
            sb.AppendLine($"{nodeBase}{new string(' ', paddingNeeded)}{note.AccessTag} {note.Description}");
        }
        else
        {
            sb.AppendLine(nodeBase);
        }

        string childPrefix = prefix + (isLast ? "    " : "│   ");

        // List Components and Collect Script Paths
        Component[] components = t.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp == null || comp is Transform) continue;

            bool isCustom = IsCustomScript(comp);
            string typeName = comp.GetType().Name;
            string compMarker = isCustom ? " (C#)" : "";

            sb.AppendLine($"{childPrefix}  ● {typeName}{compMarker}");

            // If it is a custom script, find its physical file path
            if (isCustom)
            {
#if UNITY_EDITOR
                MonoBehaviour mb = comp as MonoBehaviour;
                if (mb != null)
                {
                    // Locate the script in the Asset Database
                    MonoScript scriptAsset = MonoScript.FromMonoBehaviour(mb);
                    if (scriptAsset != null)
                    {
                        string assetPath = AssetDatabase.GetAssetPath(scriptAsset);

                        // Ensure it's a valid .cs file (ignores DLLs)
                        if (!string.IsNullOrEmpty(assetPath) && assetPath.EndsWith(".cs"))
                        {
                            string absoluteSourcePath = Path.Combine(projectRoot, assetPath);
                            scriptsToCopy.Add(absoluteSourcePath);
                        }
                    }
                }
#endif
            }
        }

        // Recurse to Children
        int childCount = t.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Traverse(t.GetChild(i), childPrefix, (i == childCount - 1), sb, scriptsToCopy, projectRoot);
        }
    }

    private bool IsCustomScript(Component comp)
    {
        if (comp == null || comp is Transform) return false;

        Type t = comp.GetType();
        string ns = t.Namespace;

        bool isUnityNamespace = !string.IsNullOrEmpty(ns) &&
                               (ns.StartsWith("UnityEngine") ||
                                ns.StartsWith("UnityEditor") ||
                                ns.StartsWith("Unity."));

        return !isUnityNamespace;
    }
}