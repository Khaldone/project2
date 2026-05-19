// Assets/_Project/3_Presentation/UI_Features/Matchmaking/MatchmakingPresenter.cs
using PlayFab.EconomyModels;
using System;
using System.Threading.Tasks;


public class MatchmakingPresenter : IDisposable
{
    //private readonly IMatchmakingView _view;
    private readonly IMatchmakingService _matchmaker; // The pure interface!
    private readonly IUIRouter _router;

    private bool _isSearching;


    //public MatchmakingPresenter(IMatchmakingView view, IMatchmakingService matchmaker, IUIRouter router)
    //{
    //    _view = view;
    //    _matchmaker = matchmaker;
    //    _router = router;


    //    _view.OnCancelClicked += CancelSearch;
    //}

    public MatchmakingPresenter(IMatchmakingService matchmaker, IUIRouter router)
    {
        _matchmaker = matchmaker;
        _router = router;


    }


    //public async void StartSearch(string queueName, int localTrophies)
    //{
    //    _isSearching = true;
    //    //_view.UpdateStatus("Contacting Matchmaker...");


    //    try
    //    {
    //        // 1. Send the pure request
    //        MatchmakingTicket ticket = await _matchmaker.RequestMatchAsync(queueName, localTrophies);

    //        // 2. Poll the CBS every 3 seconds while searching
    //        while (_isSearching)
    //        {
    //            await Task.Delay(3000);

    //            MatchResult result = await _matchmaker.CheckTicketStatusAsync(ticket.TicketId);


    //            if (result.Status == MatchStatus.Found)
    //            {
    //                _isSearching = false;
    //                _view.UpdateStatus("Server Allocated! Connecting...");

    //                // Route to the Arena and hand it the Server IP so Photon can connect!
    //                _router.LoadArenaWithServerData(result.ServerIp, result.ServerPort);
    //                break;
    //            }
    //        }
    //    }
    //    catch (Exception)
    //    {
    //        _view.UpdateStatus("Matchmaking Failed.");
    //        _isSearching = false;
    //    }
    //}


    private void CancelSearch()
    {
        _isSearching = false;
        // Tell the interface to cancel, let the wrapper figure out how to tell PlayFab
        //_matchmaker.CancelMatchmaking("current_ticket_id");
    }


    public void Dispose()
    {// _view.OnCancelClicked -= CancelSearch; }
    }
}
