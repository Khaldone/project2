// Assets/_Project/1_CoreDomain/Settings/ISettingsDependencies.cs
using System.Threading.Tasks;


public interface ILocalSaveService_New
{
    Task SaveSettingsAsync(GameSettingsData data);
    Task<GameSettingsData> LoadSettingsAsync();
}


public interface ICloudDataService
{
    Task SyncSettingsToPlayFabAsync(GameSettingsData data);
}


public struct GameSettingsData
{
    public float MasterVolume;
    public bool IsLeftHandedMode;
}


public struct VolumeChangedMessage
{
    public float NewVolume;
}