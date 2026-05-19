// Attached to: Lootbox_Slot_1 (and 2, 3, 4)
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LootboxSlotView : MonoBehaviour
{
    [SerializeField] private GameObject _emptyStateRoot;
    [SerializeField] private GameObject _filledStateRoot;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private Image _boxIcon;

    public void SetEmpty()
    {
        _emptyStateRoot.SetActive(true);
        _filledStateRoot.SetActive(false);
    }

    public void SetBox(LootboxInstance boxData, Sprite icon)
    {
        _emptyStateRoot.SetActive(false);
        _filledStateRoot.SetActive(true);
        _boxIcon.sprite = icon;


        if (boxData.IsReadyToOpen)
        {
            _timerText.text = "Tap to Open!";
            _timerText.color = Color.green;
        }
        else
        {
            // The Presenter will call a method to update this string every second
            _timerText.text = FormatTime(boxData.TimeRemaining);
            _timerText.color = Color.white;
        }
    }

    private string FormatTime(System.TimeSpan time) { /* ... */ return "02:15:30"; }
}