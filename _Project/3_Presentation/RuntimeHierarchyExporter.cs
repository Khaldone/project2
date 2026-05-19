using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using System.IO;
using System;

public class HierarchyNodeNote : MonoBehaviour
{
    public string AccessTag;
    public string Description;
}

public class RuntimeHierarchyExporter : MonoBehaviour
{
    [Header("Export Settings")]
    public string OutputFolderName = "HierarchyExports";
    public int AlignmentColumn = 70;

    [ContextMenu("Export Hierarchy (Separate Files)")]
    public void ExportHierarchy()
    {
        // 1. Ensure the output directory exists first
        string dirPath = Path.Combine(Application.dataPath, OutputFolderName);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        // 2. Loop through each loaded scene
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            // Create a NEW StringBuilder for each scene
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("```text");
            sb.AppendLine($"{scene.name}/");

            GameObject[] roots = scene.GetRootGameObjects();
            for (int j = 0; j < roots.Length; j++)
            {
                Traverse(roots[j].transform, "", (j == roots.Length - 1), sb);
            }

            sb.AppendLine("```");

            // 3. Name and save the file based on the CURRENT scene in the loop
            string filePath = Path.Combine(dirPath, $"{scene.name}_Hierarchy.md");
            File.WriteAllText(filePath, sb.ToString());

            Debug.Log($"[Hierarchy Exporter] Exported '{scene.name}' to: {filePath}");
        }
    }

    private void Traverse(Transform t, string prefix, bool isLast, StringBuilder sb)
    {
        string marker = isLast ? "└── " : "├── ";
        string nodeBase = $"{prefix}{marker}{t.name}";

        // Handle Note Component
        HierarchyNodeNote note = t.GetComponent<HierarchyNodeNote>();
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

        // List Individual Components with (C#) markers
        Component[] components = t.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp == null || comp is Transform) continue;

            string typeName = comp.GetType().Name;
            string compMarker = IsCustomScript(comp) ? " (C#)" : "";
            sb.AppendLine($"{childPrefix}  ● {typeName}{compMarker}");
        }

        // Recurse to Children
        int childCount = t.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Traverse(t.GetChild(i), childPrefix, (i == childCount - 1), sb);
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