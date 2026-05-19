// Assets/_Project/CoreDomain/SaveSystem/ILocalSaveService.cs
using Cysharp.Threading.Tasks;

public interface ILocalSaveService
{
    UniTask SaveDataAsync<T>(string key, T data);
    UniTask<T> LoadDataAsync<T>(string key, T defaultValue = default);
    void DeleteData(string key);
}

// Example Data Struct
public struct PlayerSettingsData
{
    public float MasterVolume;
    public bool HapticFeedbackEnabled;
    public string LastEquippedCueId;
}