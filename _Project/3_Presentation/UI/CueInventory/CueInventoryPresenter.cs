// Assets/_Project/3_Presentation/UI_Features/CueInventory/CueInventoryPresenter.cs
using PlayFab.EconomyModels;
using System;
using System.Collections.Generic;


public class CueInventoryPresenter : IDisposable
{
    //private readonly ICueInventoryView _view;
    private readonly IInventoryService _inventoryService;

    private List<CueItem> _cachedInventory;


    //public CueInventoryPresenter(ICueInventoryView view, IInventoryService inventoryService)
    //{
    //    _view = view;
    //    _inventoryService = inventoryService;


    //    Wire up UI events
    //    _view.OnEquipClicked += HandleEquipRequest;
    //}

    public CueInventoryPresenter(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;

    }


    public async void LoadInventory()
    {
        //_view.ShowLoadingSpinner(true);


        try
        {
            // The Presenter asks the CoreDomain interface for data
            _cachedInventory = await _inventoryService.GetPlayerCuesAsync();

            // Pass the pure data down to the visual layer
            //_view.PopulateGrid(_cachedInventory);
        }
        catch (Exception ex)
        {
            //_view.ShowErrorMessage("Failed to load collection.");
        }
        finally
        {
            //_view.ShowLoadingSpinner(false);
        }
    }


    private async void HandleEquipRequest(string instanceId)
    {
        //_view.SetEquipButtonProcessing(true);


        try
        {
            // Send the request to the CBS wrapper
            bool success = await _inventoryService.EquipCueAsync(instanceId);


            if (success)
            {
                // Refresh the UI to show the new equipped state
                LoadInventory();
            }
            else
            {
                //_view.ShowErrorMessage("Server rejected equip request.");
            }
        }
        catch
        {
            //_view.ShowErrorMessage("Network error.");
        }
        finally
        {
            //_view.SetEquipButtonProcessing(false);
        }
    }


    public void Dispose()
    {
        //_view.OnEquipClicked -= HandleEquipRequest;
    }
}
