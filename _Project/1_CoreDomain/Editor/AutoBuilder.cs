// Assets/_Project/CoreDomain/Editor/AutoBuilder.cs
//using UnityEditor;
using UnityEngine;
using System;


namespace BuildPipeline
{
    public static class AutoBuilder
    {
        // This is the method Jenkins calls via '-executeMethod'
        public static void PerformBuild()
        {
            // ... (Previous environment setup logic) ...


            // 1. Check if we are building Android
            //if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            //{
            //    // 2. Read the secure arguments passed by Jenkins
            //    string keystorePath = GetCommandLineArg("-keystorePath");
            //    string keystorePass = GetCommandLineArg("-keystorePass");
            //    string keyaliasName = GetCommandLineArg("-keyaliasName"); // e.g., "billiards_release"
            //    string keyaliasPass = GetCommandLineArg("-keyaliasPass");


            //    // 3. Apply them to PlayerSettings ONLY if they were provided
            //    if (!string.IsNullOrEmpty(keystorePath))
            //    {
            //        PlayerSettings.Android.useCustomKeystore = true;
            //        PlayerSettings.Android.keystoreName = keystorePath;
            //        PlayerSettings.Android.keystorePass = keystorePass;
            //        PlayerSettings.Android.keyaliasName = keyaliasName;
            //        PlayerSettings.Android.keyaliasPass = keyaliasPass;

            //        Debug.Log("AAA Pipeline: Android Keystore securely injected.");
            //    }
            //}


            // ... (Define scenes, build path, and execute BuildPlayer) ...

            // 1. Read the command line arguments passed by Jenkins
            string buildEnv = GetCommandLineArg("-customBuildEnv");

            // 2. Set the Bundle Identifier based on the environment
            //if (buildEnv == "Staging")
            //{
            //    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.yourstudio.billiards.dev");
            //    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.yourstudio.billiards.dev");
            //    PlayerSettings.bundleVersion = "1.0.0-beta";
            //}
            //else if (buildEnv == "Production")
            //{
            //    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.yourstudio.billiards");
            //    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.yourstudio.billiards");
            //    // (In a real setup, Jenkins would pass the version number dynamically)
            //}


            // 3. Define the scenes to build (Hub first, then Spokes)
            string[] scenes = {
                "Assets/_Project/Scenes/00_Hub_Bootstrap/00_Hub_Bootstrap.unity",
                "Assets/_Project/Scenes/01_Spoke_MainMenu/01_Spoke_MainMenu.unity",
                "Assets/_Project/Scenes/02_Spoke_GameArena/02_Spoke_GameArena.unity",
                // ... all additive UI scenes ...
            };


            //// 4. Determine target (Android APK or iOS Xcode project)
            //BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            //string extension = target == BuildTarget.Android ? ".apk" : "";
            //string buildPath = $"Builds/{buildEnv}_Build{extension}";


            //// 5. Execute the build!
            //var buildOptions = new BuildPlayerOptions
            //{
            //    scenes = scenes,
            //    locationPathName = buildPath,
            //    target = target,
            //    options = BuildOptions.None
            //};


            //var report = BuildPipeline.BuildPlayer(buildOptions);


            // 6. Tell Jenkins if we succeeded or failed
            //if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            //{
            //    Debug.Log($"AAA Pipeline: Build succeeded at {report.summary.outputPath}");
            //    EditorApplication.Exit(0);
            //}
            //else
            //{
            //    Debug.LogError("AAA Pipeline: Build failed!");
            //    EditorApplication.Exit(1); // Exit code 1 fails the Jenkins job
            //}
        }

        // Helper to parse Jenkins arguments
        private static string GetCommandLineArg(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    }
}
