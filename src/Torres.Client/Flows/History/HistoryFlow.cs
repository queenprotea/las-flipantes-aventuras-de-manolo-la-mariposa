using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class HistoryFlow
    {
        private const int SinglePage = 1;
        private const int PageStep = 1;

        private readonly IGameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<MatchHistoryResponse> _historyRequest = new PendingRequest<MatchHistoryResponse>();

        private int _requestedPageNumber = MatchHistorySelection.FirstPageNumber;

        internal HistoryFlow(IGameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal int ShownPageNumber { get; private set; } = MatchHistorySelection.FirstPageNumber;

        internal int PageCount { get; private set; }

        internal bool IsLoading => _historyRequest.IsWaiting;

        internal bool HasSeveralPages => PageCount > SinglePage;

        internal bool CanGoToPreviousPage => !IsLoading && (ShownPageNumber > MatchHistorySelection.FirstPageNumber);

        internal bool CanGoToNextPage => !IsLoading && (ShownPageNumber < PageCount);

        internal bool Open()
        {
            _historyRequest.Forget();
            PageCount = 0;
            int pageToOpen = _playerSession.MatchHistory.TakePageToResume();
            bool canOpen = _playerSession.HasAccount && _gameSessionConnection.IsSessionOpen;
            if (canOpen)
            {
                StartPage(pageToOpen);
            }

            return canOpen;
        }

        internal bool GoToPreviousPage()
        {
            bool isStarted = StartPage(ShownPageNumber - PageStep);

            return isStarted;
        }

        internal bool GoToNextPage()
        {
            bool isStarted = StartPage(ShownPageNumber + PageStep);

            return isStarted;
        }

        internal bool Retry()
        {
            bool isStarted = StartPage(_requestedPageNumber);

            return isStarted;
        }

        internal bool SelectMatch(int matchId)
        {
            bool isSelected = !IsLoading;
            if (isSelected)
            {
                _playerSession.MatchHistory.Select(matchId, ShownPageNumber);
            }

            return isSelected;
        }

        internal HistoryOutcome? TakeOutcome()
        {
            HistoryOutcome? outcome = null;
            FinishedRequest<MatchHistoryResponse>? finishedRequest = _historyRequest.TakeFinished();
            if ((finishedRequest is not null) && finishedRequest.IsSuccessful && (finishedRequest.Reply is MatchHistoryResponse historyPage))
            {
                ShownPageNumber = historyPage.PageNumber;
                PageCount = historyPage.PageCount;
                outcome = new HistoryOutcome(historyPage);
            }
            else if (finishedRequest is not null)
            {
                outcome = new HistoryOutcome(null);
            }

            return outcome;
        }

        private bool StartPage(int pageNumber)
        {
            _requestedPageNumber = pageNumber;
            bool isSessionOpen = _gameSessionConnection.IsSessionOpen;
            if (isSessionOpen && !_historyRequest.IsWaiting)
            {
                _historyRequest.Start(_gameSessionConnection.GetMatchHistoryAsync(pageNumber));
            }

            return isSessionOpen;
        }
    }
}
