// Assets/_Project/CoreDomain/Settings/SettingsData.cs
public struct DeviceSettings
{
    public float MusicVolume;
    public float SfxVolume;
    public bool IsPowerMeterOnLeft; // true = Left, false = Right
    public bool HapticsEnabled;
}

public struct AccountSettings
{
    public bool AllowFriendRequests;
    public bool AllowPrivateChats;
}