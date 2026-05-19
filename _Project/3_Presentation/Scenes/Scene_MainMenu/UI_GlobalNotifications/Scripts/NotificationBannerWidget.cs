// Attached to: Notification_Banner_Prefab
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotificationBannerWidget : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Animator _animator;


    public void SetupAndPlay(string title, string message, Sprite icon)
    {
        _titleText.text = title;
        _messageText.text = message;
        if (icon != null) _iconImage.sprite = icon;


        // Trigger the slide-down animation
        _animator.SetTrigger("SlideIn");


        // Automatically clean this up after 4 seconds
        Invoke(nameof(Dismiss), 4f);
    }


    // You can hook this to a Button component if you want the player to swipe it away early
    public void Dismiss()
    {
        _animator.SetTrigger("SlideOut");
        // Destroy the GameObject after the 0.5s slide out animation finishes
        Destroy(gameObject, 0.5f);
    }
}