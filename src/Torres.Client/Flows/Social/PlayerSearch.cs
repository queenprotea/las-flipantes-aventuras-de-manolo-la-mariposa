using System;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class PlayerSearch
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<PlayerSearchResponse> _searchRequest = new PendingRequest<PlayerSearchResponse>();

        private string _searchedUsername = string.Empty;
        private bool _hasNewResult;

        internal PlayerSearch(GameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal PlayerSearchResponse? Result { get; private set; }

        internal string? Search(string typedUsername)
        {
            string username = typedUsername.Trim();
            string? errorKey = null;
            bool isOwnUsername = string.Equals(username, _playerSession.Player?.Username, StringComparison.OrdinalIgnoreCase);
            if (username.Length == 0)
            {
                errorKey = TextKeys.Common.RequiredField;
            }
            else if (isOwnUsername)
            {
                ShowNotFound();
            }
            else if (!_searchRequest.IsWaiting)
            {
                StartRequest(username);
            }

            return errorKey;
        }

        internal void Repeat()
        {
            StartRequest(_searchedUsername);
        }

        internal void ShowNotFound()
        {
            Result = new PlayerSearchResponse { Status = PlayerSearchStatus.NotFound };
            _hasNewResult = true;
        }

        internal void Clear()
        {
            Result = null;
        }

        internal void Forget()
        {
            _searchRequest.Forget();
        }

        internal PlayerSearchUpdate TakeUpdate()
        {
            PlayerSearchUpdate searchUpdate = _hasNewResult ? PlayerSearchUpdate.ResultChanged : PlayerSearchUpdate.Unchanged;
            _hasNewResult = false;
            FinishedRequest<PlayerSearchResponse>? finishedSearch = _searchRequest.TakeFinished();
            if ((finishedSearch is not null) && finishedSearch.IsSuccessful && (finishedSearch.Reply is PlayerSearchResponse foundPlayers))
            {
                Result = foundPlayers;
                searchUpdate = PlayerSearchUpdate.ResultChanged;
            }
            else if (finishedSearch is not null)
            {
                searchUpdate = PlayerSearchUpdate.Failed;
            }

            return searchUpdate;
        }

        private void StartRequest(string username)
        {
            if (_gameSessionConnection.IsSessionOpen)
            {
                _searchedUsername = username;
                _searchRequest.Forget();
                _searchRequest.Start(_gameSessionConnection.FindPlayerByUsernameAsync(username));
            }
        }
    }
}
