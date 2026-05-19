// Attached to: GlobalNotificationCanvas
using System;
using UnityEngine;

public interface IGlobalNotificationView
{

}
public class UI_GlobalNotificationView : MonoBehaviour, IGlobalNotificationView
{
    [Header("Prefabs & Anchors")]
    [SerializeField] private NotificationBannerWidget _bannerPrefab;
    [SerializeField] private Transform _bannerAnchor;

    //[Header("Critical System Modals")]
    //[SerializeField] private SystemModalWidget _criticalModal;


    public event Action OnCriticalModalAcknowledged;


    private void Awake()
    {
        //_criticalModal.OnAcknowledged += () => OnCriticalModalAcknowledged?.Invoke();
    }


    // Called by the Presenter for non-interruptive toasts (e.g., "Friend online")
    public void ShowToastBanner(string title, string message, Sprite icon)
    {
        // In a live environment, use an Object Pool here instead of Instantiate
        var banner = Instantiate(_bannerPrefab, _bannerAnchor);
        banner.SetupAndPlay(title, message, icon);
    }


    // Called by the Presenter for hard interruptions (e.g., "Server Maintenance")
    public void ShowCriticalModal(string header, string body, string buttonText)
    {
        //_criticalModal.Show(header, body, buttonText);
    }
}
