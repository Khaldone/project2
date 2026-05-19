// Assets/Scripts/Editor/BootstrapperAutoLoader.cs
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class BootstrapperAutoLoader
{
    // The exact path to your Hub scene
    private const string HUB_SCENE_PATH = "Assets/Scenes/00_Bootstrap.unity";

    // A temporary memory key to pass data between the Editor and the Runtime
    private const string TARGET_SPOKE_KEY = "AutoLoad_TargetSpokeScene";


    static BootstrapperAutoLoader()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Intercept right as the Editor tries to enter Play Mode
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();


            // If the developer is already in the Hub, do nothing.
            // If they are in a Spoke scene, we intervene.
            if (activeScene.path != HUB_SCENE_PATH && !string.IsNullOrEmpty(activeScene.path))
            {
                // 1. Save the path of the Spoke scene they are looking at
                EditorPrefs.SetString(TARGET_SPOKE_KEY, activeScene.path);


                // 2. Force the Editor to start from the Hub scene instead
                SceneAsset hubSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(HUB_SCENE_PATH);
                EditorSceneManager.playModeStartScene = hubSceneAsset;
            }
            else
            {
                // Clean up if they are starting normally from the Hub
                EditorPrefs.DeleteKey(TARGET_SPOKE_KEY);
                EditorSceneManager.playModeStartScene = null;
            }
        }
    }
}