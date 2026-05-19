// 1. Assets/Scripts/CoreDomain/Assets/IAssetLoader.cs
using System.Threading.Tasks;
public interface IAssetLoader
{
    // Requests an asset by its string address (e.g., "Cues/Cue_Dragon")
    Task<object> LoadAndInstantiateAsync(string address);

    // AAA Standard: Memory management. We must be able to release it!
    void Release(object instance);
}