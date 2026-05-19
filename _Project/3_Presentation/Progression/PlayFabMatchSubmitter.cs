// Assets/Scripts/Presentation/Progression/PlayFabMatchSubmitter.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;


public class PlayFabMatchSubmitter : MonoBehaviour, IMatchResultSubmitter
{
    public async Task<bool> SubmitMatchDataAsync(string opponentId, bool didWin, int shotsTaken)
    {
        var tcs = new TaskCompletionSource<bool>();


        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "ValidateAndRecordMatch", // This lives on the PlayFab servers
            FunctionParameter = new
            {
                Opponent = opponentId,
                Won = didWin,
                Shots = shotsTaken
            },
            GeneratePlayStreamEvent = true
        };


        PlayFabClientAPI.ExecuteCloudScript(request,
            result =>
            {
                // Optionally check result.FunctionResult for server-side validation errors
                tcs.SetResult(true);
            },
            error =>
            {
                Debug.LogError($"PlayFab Error: {error.GenerateErrorReport()}");
                tcs.SetResult(false);
            });


        return await tcs.Task;
    }
}
