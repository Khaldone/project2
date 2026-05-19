// Attached to: IAPShopCanvas
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface IIAPShopView
{

}

public class IAPShopView : MonoBehaviour, IIAPShopView
{
    [Header("Core UI")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private GameObject _transactionBlocker;
    [SerializeField] private TextMeshProUGUI _transactionStatusText;

    [Header("Dynamic Content")]
    [SerializeField] private StoreItemWidget _itemPrefab;
    [SerializeField] private Transform _gemsGrid;
    [SerializeField] private Transform _coinsGrid;


    // The events our IAPShopPresenter listens to
    public event Action OnCloseClicked;
    //public event Action<StoreItemData> OnItemPurchaseClicked;


    private List<StoreItemWidget> _activeItems = new List<StoreItemWidget>();


    private void Awake()
    {
        _closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
    }


    // The Presenter calls this passing the parsed PlayFab Economy V2 Catalog
    //public void PopulateStore(IReadOnlyList<StoreItemData> catalogItems)
    //{
    //    ClearStore();


    //    foreach (var item in catalogItems)
    //    {
    //        Transform targetGrid = item.Category == "Gems" ? _gemsGrid : _coinsGrid;
    //        var widget = Instantiate(_itemPrefab, targetGrid);

    //        widget.Setup(item);

    //        // Wire the individual item's buy button up to the master View event
    //        widget.OnBuyClicked += () => OnItemPurchaseClicked?.Invoke(item);

    //        _activeItems.Add(widget);
    //    }
    //}


    // CRITICAL: Called the millisecond the player clicks buy to prevent double-charging
    public void SetTransactionState(bool isProcessing, string statusMessage = "")
    {
        _transactionBlocker.SetActive(isProcessing);
        _transactionStatusText.text = statusMessage;
    }


    public void UpdateWalletDisplays(int coins, int gems)
    {
        // Update the visual currency counters at the top of the shop
    }


    private void ClearStore() { /* Pool or destroy active items */ }

    private void OnDestroy() { _closeButton.onClick.RemoveAllListeners(); }
}