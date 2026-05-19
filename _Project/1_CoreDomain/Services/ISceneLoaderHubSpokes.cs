// Assets/Scripts/CoreDomain/Services/ISceneLoaderHubSpokes.cs
using System.Threading.Tasks;

public interface ISceneLoaderHubSpokes
{
    Task LoadSpokeSceneAsync(string sceneName);
}