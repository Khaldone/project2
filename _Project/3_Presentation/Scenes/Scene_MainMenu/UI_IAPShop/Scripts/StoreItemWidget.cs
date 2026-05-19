// Attached to: Store_Item_Prefab
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class StoreItemWidget : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private Image _productIcon;
    [SerializeField] private GameObject _bonusTag; // e.g., "20% EXTRA!"

    [Header("Purchase Button")]
    [SerializeField] private Button _buyButton;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private Image _currencyIcon; // Hidden if Real Money

    public event Action OnBuyClicked;


    private void Awake()
    {
        //_buyButton.onClick.AddListener(() => OnBuyClicked?.Invoke());
    }


    //public void Setup(StoreItemData itemData)
    //{
    //    _titleText.text = itemData.Title;
    //    // _productIcon.sprite = ... (Load via Addressables based on itemData.Id)


    //    if (itemData.IsRealMoney)
    //    {
    //        // E.g., "$4.99" (The string comes from Apple/Google via the Presenter)
    //        _priceText.text = itemData.LocalizedRealMoneyPrice;
    //        _currencyIcon.gameObject.SetActive(false);
    //    }
    //    else
    //    {
    //        // E.g., "500" (Virtual Currency)
    //        _priceText.text = itemData.VirtualCurrencyCost.ToString();
    //        _currencyIcon.gameObject.SetActive(true);
    //        // Set currency icon to Gems or Coins sprite...
    //    }


    //    _bonusTag.SetActive(itemData.HasBonus);
    //}
}
