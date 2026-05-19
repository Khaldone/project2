// Assets/_Project/CoreDomain/UI/ITransitionService.cs
using System.Threading.Tasks;
public interface ITransitionService
{
    // Fades the screen to black (or covers it).
    // Returns a Task so the Router can await its completion.
    Task FadeOutAsync(float durationInSeconds = 0.25f);

    // Reveals the scene underneath.
    Task FadeInAsync(float durationInSeconds = 0.25f);
}