// Assets/_Project/CoreDomain/Services/IUIRouter.cs
using System.Threading.Tasks;

public interface IUIRouter
{
    Task OpenMenuAsync(string additiveSceneName);
    Task CloseMenuAsync(string additiveSceneName);
}