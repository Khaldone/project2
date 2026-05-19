// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/UI/CanvasFadeTransition.cs
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CanvasFadeTransition : MonoBehaviour, ITransitionService
{
    [SerializeField] private CanvasGroup _fadeGroup;
    [SerializeField] private GameObject _loadingSpinner; // Optional graphic

    private void Awake()
    {
        // Ensure the screen starts clear
        _fadeGroup.alpha = 0f;
        _fadeGroup.blocksRaycasts = false;
        if (_loadingSpinner != null) _loadingSpinner.SetActive(false);
    }

    public async Task FadeOutAsync(float duration)
    {
        _fadeGroup.blocksRaycasts = true; // Block clicks immediately

        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            _fadeGroup.alpha = Mathf.Clamp01(time / duration);
            await Task.Yield();
        }


        _fadeGroup.alpha = 1f;
        if (_loadingSpinner != null) _loadingSpinner.SetActive(true); // Show spinner once dark
    }

    public async Task FadeInAsync(float duration)
    {
        if (_loadingSpinner != null) _loadingSpinner.SetActive(false); // Hide spinner


        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            // Reverse the alpha
            _fadeGroup.alpha = 1f - Mathf.Clamp01(time / duration);
            await Task.Yield();
        }

        _fadeGroup.alpha = 0f;
        _fadeGroup.blocksRaycasts = false; // Allow clicks again
    }
}