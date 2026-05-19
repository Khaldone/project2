// Assets/_Project/3_Presentation/Scene_IAP/Scripts/AchievementRowView.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Billiards.CoreDomain.Player;

namespace Billiards.Presentation.Shop
{
    public class AchievementRowView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _progressText; // e.g., "7/10"
        [SerializeField] private Image _progressBarFill;        // A UI Image set to "Filled" type
        [SerializeField] private GameObject _completedCheckmark;

        public void Setup(AchievementData data)
        {
            if (_titleText != null) _titleText.text = data.Title;
            if (_descriptionText != null) _descriptionText.text = data.Description;
            if (_progressText != null) _progressText.text = $"{data.CurrentProgress} / {data.MaxProgress}";

            if (_progressBarFill != null)
            {
                // Smoothly fill the bar based on the math property we wrote in the struct
                _progressBarFill.fillAmount = data.ProgressNormalized;
            }

            // Toggle visual state based on completion
            if (_completedCheckmark != null)
            {
                _completedCheckmark.SetActive(data.IsClaimed);
            }
        }
    }
}