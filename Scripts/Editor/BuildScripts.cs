// Assets/Scripts/Editor/BuildScripts.cs
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;


public static class BuildScripts
{
    // This is the exact method name called in the Jenkinsfile:
    // -executeMethod BuildScripts.BuildAndroid
    public static void BuildAndroid()
    {
        Debug.Log("AAA Pipeline: Starting Android Production Build...");


        // 1. Define the scenes to include (matching your Bootstrap architecture)
        string[] scenes = {
            "Assets/Scenes/00_Bootstrap.unity",
            "Assets/Scenes/01_MainMenu.unity",
            "Assets/Scenes/02_GameArena.unity"
        };


        // 2. Configure Build Options
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = "Builds/Billiards_Release.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None; // For production, no 'Development' flags


        // 3. Set Android Specifics (Keystore, Versioning)
        PlayerSettings.Android.bundleVersionCode = GetJenkinsBuildNumber();
        PlayerSettings.bundleVersion = "1.0." + GetJenkinsBuildNumber();


        // 4. Execute the Build
        //BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        //BuildSummary summary = report.summary;


        //if (summary.result == BuildResult.Succeeded)
        //{
        //    Debug.Log("AAA Pipeline: Build Succeeded! Size: " + summary.totalSize + " bytes");
        //    EditorApplication.Exit(0); // Exit Unity with success code
        //}
        //else
        //{
        //    Debug.LogError("AAA Pipeline: Build Failed!");
        //    EditorApplication.Exit(1); // Exit Unity with failure code to stop Jenkins
        //}
    }


    private static int GetJenkinsBuildNumber()
    {
        // Jenkins passes the build number as an environment variable
        string buildNumber = System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        return int.TryParse(buildNumber, out int result) ? result : 1;
    }
}
