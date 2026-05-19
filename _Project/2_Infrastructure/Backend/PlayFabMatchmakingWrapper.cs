// Assets/_Project/2_Infrastructure/Backend/PlayFabMatchmakingWrapper.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.MultiplayerModels;
using UnityEngine;


public class PlayFabMatchmakingWrapper : IMatchmakingService_New
{
    //private readonly IPlayFabAuthService _auth; // Injected via VContainer


    //public PlayFabMatchmakingWrapper(IPlayFabAuthService auth)
    //{
    //    _auth = auth;
    //}


    // Translates our pure C# request into a PlayFab API call
    public async Task<MatchmakingTicket> RequestMatchAsync(string queueName, int playerTrophies)
    {
        var tcs = new TaskCompletionSource<MatchmakingTicket>();


        var request = new CreateMatchmakingTicketRequest
        {
            //Creator = new MatchmakingPlayer
            //{
            //    Entity = new EntityKey { Id = _auth.EntityId, Type = _auth.EntityType },
            //    Attributes = new MatchmakingPlayerAttributes
            //    {
            //        // Pass the pure C# integer into the CBS JSON payload
            //        DataObject = new { Trophies = playerTrophies }
            //    }
            //},
            //GiveUpAfterSeconds = 120,
            //QueueName = queueName
        };


        PlayFabMultiplayerAPI.CreateMatchmakingTicket(request,
            result =>
            {
                // Translate the PlayFab result back into our pure C# struct
                tcs.SetResult(new MatchmakingTicket
                {
                    TicketId = result.TicketId,
                    // Fallback to 30s if PlayFab doesn't provide an estimate
                    EstimatedWaitTimeSeconds = result.TicketId != null ? 30 : 0
                });
            },
            error =>
            {
                Debug.LogError($"CBS Error: {error.ErrorMessage}");
                tcs.SetException(new System.Exception(error.ErrorMessage));
            });


        return await tcs.Task;
    }


    // Polls the CBS to see if a server has been allocated
    public async Task<MatchResult> CheckTicketStatusAsync(string ticketId)
    {
        var tcs = new TaskCompletionSource<MatchResult>();


        var request = new GetMatchmakingTicketRequest { QueueName = "TokyoTier", TicketId = ticketId };


        PlayFabMultiplayerAPI.GetMatchmakingTicket(request,
            result =>
            {
                var matchResult = new enMatchResult();


                if (result.Status == "Matched")
                {
                    matchResult.Status = MatchStatus.Found;
                    matchResult.MatchId = result.MatchId;
                    // The CBS will return the IP/Port of the dedicated server it just spun up
                    // We parse that here so the CoreDomain doesn't have to know how PlayFab formats IPs
                }
                else if (result.Status == "Canceled")
                {
                    matchResult.Status = MatchStatus.Canceled;
                }
                else
                {
                    matchResult.Status = MatchStatus.Searching;
                }


                //tcs.SetResult(matchResult);
            },
            error => tcs.SetException(new System.Exception(error.ErrorMessage)));


        return await tcs.Task;
    }


    public void CancelMatchmaking(string ticketId)
    {
        // Executes PlayFabMultiplayerAPI.CancelMatchmakingTicket(...)
    }

    UniTask<MatchmakingTicket> IMatchmakingService_New.RequestMatchAsync(string queueName, int playerTrophies)
    {
        throw new System.NotImplementedException();
    }

    UniTask<MatchResult> IMatchmakingService_New.CheckTicketStatusAsync(string ticketId)
    {
        throw new System.NotImplementedException();
    }
}
