// Assets/_Project/1_CoreDomain/Metagame/GachaOrchestrator.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


public class GachaOrchestrator
{
    //private readonly IGachaCloudService _cloudService;
    private readonly IMessageBroker_New _broker;
    //private readonly ILocalInventoryCache _inventory;


    //public GachaOrchestrator(IGachaCloudService cloud, IMessageBroker broker, ILocalInventoryCache inventory)
    //{
    //    _cloudService = cloud;
    //    _broker = broker;
    //    _inventory = inventory;
    //}

    public GachaOrchestrator(IMessageBroker_New broker)
    {
        _broker = broker;
    }

    // Called when the user clicks "Start Unlock" on a box they just won
    public async Task StartBoxUnlockAsync(string boxInstanceId)
    {
        // Ensure they don't already have a box unlocking (most games restrict this to 1 at a time)
        //if (_inventory.IsAnyBoxUnlocking())
        //{
        //    _broker.Publish(new SystemNotificationMessage("Error", "Another box is already unlocking."));
        //    return;
        //}


        // Ask the server to stamp the CustomData with "UnlockTime = Now + 3 Hours"
        //bool success = await _cloudService.StartUnlockTimerAsync(boxInstanceId);

        //if (success)
        //{
        //    _inventory.MarkBoxAsUnlocking(boxInstanceId);
        //    _broker.Publish(new BoxStateChangedMessage(boxInstanceId));
        //}
    }


    // Called when the timer hits 0:00, or when the user clicks "Open with Gems"
    public async Task TryOpenBoxAsync(string boxInstanceId, bool useGems)
    {
        //_broker.Publish(new GachaProcessingStateMessage(isProcessing: true));


        // Let the Azure Function handle the time validation and RNG
        //var result = await _cloudService.TryOpenLootboxAsync(boxInstanceId, useGems);


        //if (result.Success)
        //{
        //    // The server successfully opened it and returned the shards!
        //    _inventory.RemoveBox(boxInstanceId);
        //    ProcessNewCuePieces(result.GrantedItems);

        //    // Tell the UI to play the massive fireworks "Loot Reveal" sequence
        //    _broker.Publish(new LootboxOpenedSequenceMessage(result.GrantedItems));
        //}
        //else
        //{
        //    _broker.Publish(new SystemNotificationMessage("Failed", result.ErrorMessage));
        //}


        //_broker.Publish(new GachaProcessingStateMessage(isProcessing: false));
    }


    private void ProcessNewCuePieces(List<string> grantedItemIds)
    {
        // Loop through the received pieces and update the local assembly cache
        //foreach (var pieceId in grantedItemIds)
        //{
        //    _inventory.AddCuePiece(pieceId);

        //    // Check if we just hit 4/4 pieces for a Legendary Cue!
        //    if (_inventory.CanCraftCue(pieceId))
        //    {
        //        _broker.Publish(new CueCraftingReadyMessage(pieceId));
        //    }
        //}
    }
}
