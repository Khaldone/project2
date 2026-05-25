using UnityEngine;
using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AndroidFingerprintReader : MonoBehaviour
{
    void Start()
    {
        Debug.Log("--- STARTING FINGERPRINT EXTRACTION ---");

#if UNITY_ANDROID && !UNITY_EDITOR
        // Running live on an Android device
        PrintDeviceBuildFingerprint();
        PrintDeviceSigningFingerprints();
#elif UNITY_EDITOR
        // Running inside the Unity Editor
        PrintEditorKeystoreFingerprints();
#else
        Debug.Log("[Fingerprint Log] Not running on Android or inside the Unity Editor.");
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void PrintDeviceBuildFingerprint()
    {
        try
        {
            using (AndroidJavaClass buildClass = new AndroidJavaClass("android.os.Build"))
            {
                string deviceFingerprint = buildClass.GetStatic<string>("FINGERPRINT");
                Debug.Log($"[Fingerprint Log] Device Build Hardware ID: {deviceFingerprint}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Fingerprint Log] Error getting hardware ID: {e.Message}");
        }
    }

    private void PrintDeviceSigningFingerprints()
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager"))
            {
                string packageName = currentActivity.Call<string>("getPackageName");
                int flags = 134217728; // PackageManager.GET_SIGNING_CERTIFICATES
                
                using (AndroidJavaObject packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, flags))
                using (AndroidJavaObject signingInfo = packageInfo.Get<AndroidJavaObject>("signingInfo"))
                {
                    AndroidJavaObject[] signatures;
                    if (signingInfo.Call<bool>("hasMultipleSigners"))
                    {
                        signatures = signingInfo.Call<AndroidJavaObject[]>("getApkContentsSigners");
                    }
                    else
                    {
                        signatures = signingInfo.Call<AndroidJavaObject[]>("getSigningCertificateHistory");
                    }

                    if (signatures != null && signatures.Length > 0)
                    {
                        byte[] certBytes = signatures[0].Call<byte[]>("toByteArray");
                        
                        using (var sha1 = System.Security.Cryptography.SHA1.Create())
                        using (var sha256 = System.Security.Cryptography.SHA256.Create())
                        {
                            string sha1Hex = BitConverter.ToString(sha1.ComputeHash(certBytes)).Replace("-", ":");
                            string sha256Hex = BitConverter.ToString(sha256.ComputeHash(certBytes)).Replace("-", ":");
                            
                            Debug.Log($"[Fingerprint Log] Device Runtime SHA-1: {sha1Hex}");
                            Debug.Log($"[Fingerprint Log] Device Runtime SHA-256: {sha256Hex}");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Fingerprint Log] Device Error: {e.Message}");
        }
    }
#endif

#if UNITY_EDITOR
    private void PrintEditorKeystoreFingerprints()
    {
        string keystorePath = PlayerSettings.Android.keystoreName;
        string keystorePass = PlayerSettings.Android.keystorePass;

        if (!PlayerSettings.Android.useCustomKeystore)
        {
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            keystorePath = Path.Combine(homePath, ".android", "debug.keystore");
            keystorePass = "android";
            Debug.Log("[Fingerprint Log] Editor: Scanning default Android debug keystore.");
        }

        if (!File.Exists(keystorePath))
        {
            Debug.LogError($"[Fingerprint Log] Keystore file not found at: '{keystorePath}'");
            return;
        }

        try
        {
            string keytoolBinary = Application.platform == RuntimePlatform.WindowsEditor ? "keytool.exe" : "keytool";
            string keytoolPath = "";

            // Step 1: Check Unity's modern automated tools path API
            string settingsJdkPath = UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath;
            if (!string.IsNullOrEmpty(settingsJdkPath))
            {
                string testPath = Path.Combine(settingsJdkPath, "bin", keytoolBinary);
                if (File.Exists(testPath)) keytoolPath = testPath;
            }

            // Step 2: Check modern embedded OpenJDK directory structure
            if (string.IsNullOrEmpty(keytoolPath))
            {
                string editorDir = Path.GetDirectoryName(EditorApplication.applicationPath);
                string testPath = Path.Combine(editorDir, "Data", "PlaybackEngines", "AndroidPlayer", "OpenJDK", "bin", keytoolBinary);
                if (File.Exists(testPath)) keytoolPath = testPath;
            }

            // Step 3: Check legacy embedded OpenJDK directory structure
            if (string.IsNullOrEmpty(keytoolPath))
            {
                string editorDir = Path.GetDirectoryName(EditorApplication.applicationPath);
                string testPath = Path.Combine(editorDir, "Data", "PlaybackEngines", "AndroidPlayer", "Tools", "OpenJDK", "Windows", "bin", keytoolBinary);
                if (File.Exists(testPath)) keytoolPath = testPath;
            }

            // Step 4: Check system environment fallback variables
            if (string.IsNullOrEmpty(keytoolPath))
            {
                string javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
                if (!string.IsNullOrEmpty(javaHome))
                {
                    string testPath = Path.Combine(javaHome, "bin", keytoolBinary);
                    if (File.Exists(testPath)) keytoolPath = testPath;
                }
            }

            // Step 5: Absolute structural safety baseline fallback
            if (string.IsNullOrEmpty(keytoolPath))
            {
                keytoolPath = keytoolBinary;
            }

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = keytoolPath,
                Arguments = $"-list -v -keystore \"{keystorePath}\" -storepass \"{keystorePass}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error) && process.ExitCode != 0)
                {
                    Debug.LogError($"[Fingerprint Log] Keytool execution error: {error}");
                    return;
                }

                string[] lines = output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                bool foundKeys = false;

                foreach (string line in lines)
                {
                    if (line.Trim().StartsWith("SHA1:") || line.Trim().StartsWith("SHA256:"))
                    {
                        Debug.Log($"[Fingerprint Log] Editor Keystore -> {line.Trim()}");
                        foundKeys = true;
                    }
                }

                if (!foundKeys)
                {
                    Debug.LogWarning("[Fingerprint Log] Keytool completed execution, but no SHA fields were found inside the generated logs.");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Fingerprint Log] CLI Execution Failure: {ex.Message}");
        }
    }
#endif
}