// Assets/_Project/2_Infrastructure/Backend/PlayFabInventoryWrapper.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;


public class PlayFabInventoryWrapper : IInventoryService
{
    public async Task<List<CueItem>> GetPlayerCuesAsync()
    {
        var tcs = new TaskCompletionSource<List<CueItem>>();


        // 1. Make the CBS SDK Call
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            result =>
            {
                var parsedCues = new List<CueItem>();


                // 2. Translate the PlayFab data into our CoreDomain format
                foreach (ItemInstance item in result.Inventory)
                {
                    if (item.ItemClass == "Cue") // Filter by item class
                    {
                        // Parse custom data (e.g., stats stored in PlayFab Economy V2)
                        int power = 0, spin = 0;
                        if (item.CustomData != null)
                        {
                            item.CustomData.TryGetValue("PowerStat", out string pStr);
                            item.CustomData.TryGetValue("SpinStat", out string sStr);
                            int.TryParse(pStr, out power);
                            int.TryParse(sStr, out spin);
                        }


                        // Determine if it's equipped by checking the custom ItemClass or tags
                        bool isEquipped = item.CustomData != null && item.CustomData.ContainsKey("Equipped");


                        parsedCues.Add(new CueItem
                        {
                            InstanceId = item.ItemInstanceId,
                            CatalogId = item.ItemId,
                            DisplayName = item.DisplayName,
                            PowerStat = power,
                            SpinStat = spin,
                            IsEquipped = isEquipped
                        });
                    }
                }


                tcs.SetResult(parsedCues);
            },
            error =>
            {
                Debug.LogError($"Inventory Fetch Failed: {error.ErrorMessage}");
                tcs.SetException(new Exception(error.ErrorMessage));
            });


        return await tcs.Task;
    }


    // This MUST go through CloudScript to prevent client-side hacking
    public async Task<bool> EquipCueAsync(string instanceId)
    {
        var tcs = new TaskCompletionSource<bool>();


        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "EquipCueSecure", // The Azure Function/CloudScript name
            FunctionParameter = new { TargetInstanceId = instanceId }
        };


        PlayFabClientAPI.ExecuteCloudScript(request,
            result =>
            {
                // Check if the server-side script successfully updated the database
                bool success = result.FunctionResult != null && (bool)result.FunctionResult;
                tcs.SetResult(success);
            },
            error => tcs.SetException(new Exception(error.ErrorMessage)));


        return await tcs.Task;
    }
}