// 2. Assets/Scripts/CoreDomain/SaveSystem/ICloudSaveBackend.cs
using Cysharp.Threading.Tasks;

// The abstraction for PlayFab (or any future backend)
public interface ICloudSaveBackend
{
    UniTask<PlayerProfileSave> LoadProfileAsync();
    UniTask<bool> SaveProfileAsync(PlayerProfileSave profile);
}