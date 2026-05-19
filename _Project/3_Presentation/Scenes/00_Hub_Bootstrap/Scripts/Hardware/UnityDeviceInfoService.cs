// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Hardware/UnityDeviceInfoService.cs
using UnityEngine;

public class UnityDeviceInfoService : MonoBehaviour, IDeviceInfoService
{
    public DeviceFormFactor CurrentFormFactor { get; private set; }
    public bool IsNotchDevice { get; private set; }

    private void Awake()
    {
        CalculateFormFactor();
        CheckForNotch();
    }

    private void CalculateFormFactor()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        // In the editor, you might want to read a debug toggle to test tablet UI
        CurrentFormFactor = DeviceFormFactor.Phone;
#else
        // Calculate physical screen size in inches
        float screenWidth = Screen.width / Screen.dpi;
        float screenHeight = Screen.height / Screen.dpi;
        float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));


        // Industry standard: Anything larger than 7 inches is typically a tablet
        if (diagonalInches > 7.0f || DetermineByAspectRatio())
        {
            CurrentFormFactor = DeviceFormFactor.Tablet;
        }
        else
        {
            CurrentFormFactor = DeviceFormFactor.Phone;
        }
#endif
        Debug.Log($"AAA Pipeline: Hardware detected as {CurrentFormFactor}");
    }

    private bool DetermineByAspectRatio()
    {
        // iPads are 4:3 (1.33). Phones are usually 16:9 (1.77) or wider.
        float aspect = (float)Mathf.Max(Screen.width, Screen.height) / Mathf.Min(Screen.width, Screen.height);
        return aspect < 1.5f;
    }


    private void CheckForNotch()
    {
        // Unity's safe area API detects notches and dynamic islands
        IsNotchDevice = Screen.safeArea.y > 0 || Screen.safeArea.height < Screen.height;
    }
}