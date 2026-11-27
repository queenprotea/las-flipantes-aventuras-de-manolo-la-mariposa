using System;
using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class FriendsFlow
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly TimeSpan _friendshipListRefreshPeriod;
        private readonly PendingRequest<FriendshipListResponse> _friendshipListRequest = new PendingRequest<FriendshipListResponse>();
        private readonly PlayerSearch _playerSearch;
        private readonly PendingRequest<FriendRequestResult> _friendRequest = new PendingRequest<FriendRequestResult>();
        private readonly PendingRequest<FriendRequestResponseResult> _responseRequest = new PendingRequest<FriendRequestResponseResult>();
        private readonly PendingRequest<FriendRemovalResult> _removalRequest = new PendingRequest<FriendRemovalResult>();

        private TimeSpan _sinceFriendshipList;
        private bool _needsRender;
        private bool _hasFailed;
        private bool _showsSentRequests;

        internal FriendsFlow(GameSessionConnection gameSessionConnection, PlayerSession playerSession, TimeSpan friendshipListRefreshPeriod)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
            _playerSearch = new PlayerSearch(gameSessionConnection, playerSession);
            _friendshipListRefreshPeriod = friendshipListRefreshPeriod;
        }

        internal FriendshipListResponse FriendshipList { get; private set; } = CreateEmptyFriendshipList();

        internal PlayerSearchResponse? SearchResult => _playerSearch.Result;

        internal bool Open()
        {
            ForgetRequests();
            FriendshipList = CreateEmptyFriendshipList();
            _playerSearch.Clear();
            bool canOpen = _playerSession.HasAccount && _gameSessionConnection.IsSessionOpen;
            if (canOpen)
            {
                StartFriendshipListRequest();
            }

            return canOpen;
        }

        internal void Advance(TimeSpan elapsedTime)
        {
            _sinceFriendshipList += elapsedTime;
            bool isRefreshDue = (_sinceFriendshipList >= _friendshipListRefreshPeriod) && !_friendshipListRequest.IsWaiting;
            if (isRefreshDue)
            {
                StartFriendshipListRequest();
            }
        }

        internal FriendsOutcome? TakeOutcome()
        {
            FollowFriendshipListRequest();
            FollowSearch();
            FollowFriendRequest();
            FollowResponse();
            FollowRemoval();
            FriendsOutcome? outcome = null;
            if (_needsRender || _hasFailed || _showsSentRequests)
            {
                outcome = new FriendsOutcome(_needsRender, _hasFailed, _showsSentRequests);
            }

            _needsRender = false;
            _hasFailed = false;
            _showsSentRequests = false;

            return outcome;
        }

        internal string? Search(string typedUsername)
        {
            string? errorKey = _playerSearch.Search(typedUsername);

            return errorKey;
        }

        internal void ClearSearch()
        {
            _playerSearch.Clear();
            if (!_friendshipListRequest.IsWaiting)
            {
                StartFriendshipListRequest();
            }
        }

        internal void SendFriendRequest(int targetPlayerId)
        {
            bool canSend = _gameSessionConnection.IsSessionOpen && !_friendRequest.IsWaiting;
            if (canSend)
            {
                _friendRequest.Start(_gameSessionConnection.SendFriendRequestAsync(targetPlayerId));
            }
        }

        internal void AcceptFriendRequest(int requesterPlayerId)
        {
            bool canAccept = _gameSessionConnection.IsSessionOpen && !_responseRequest.IsWaiting;
            if (canAccept)
            {
                _responseRequest.Start(_gameSessionConnection.AcceptFriendRequestAsync(requesterPlayerId));
            }
        }

        internal void RejectFriendRequest(int requesterPlayerId)
        {
            bool canReject = _gameSessionConnection.IsSessionOpen && !_responseRequest.IsWaiting;
            if (canReject)
            {
                _responseRequest.Start(_gameSessionConnection.RejectFriendRequestAsync(requesterPlayerId));
            }
        }

        internal void RemoveFriend(int friendPlayerId)
        {
            bool canRemove = _gameSessionConnection.IsSessionOpen && !_removalRequest.IsWaiting;
            if (canRemove)
            {
                _removalRequest.Start(_gameSessionConnection.RemoveFriendAsync(friendPlayerId));
            }
        }

        private static FriendshipListResponse CreateEmptyFriendshipList()
        {
            var emptyFriendshipList = new FriendshipListResponse
            {
                Friends = new List<PlayerSummary>(),
                ReceivedRequests = new List<PlayerSummary>(),
                SentRequests = new List<PlayerSummary>(),
            };

            return emptyFriendshipList;
        }

        private static bool ShowsSameRows(FriendshipListResponse shownFriendshipList, FriendshipListResponse receivedFriendshipList)
        {
            bool showsSameRows = ShowsSameRows(shownFriendshipList.Friends, receivedFriendshipList.Friends)
                && ShowsSameRows(shownFriendshipList.ReceivedRequests, receivedFriendshipList.ReceivedRequests)
                && ShowsSameRows(shownFriendshipList.SentRequests, receivedFriendshipList.SentRequests);

            return showsSameRows;
        }

        private static bool ShowsSameRows(IEnumerable<PlayerSummary> shownPlayers, IEnumerable<PlayerSummary> receivedPlayers)
        {
            bool hasSamePlayers = shownPlayers.Select(player => player.PlayerId)
                .SequenceEqual(receivedPlayers.Select(player => player.PlayerId));
            bool hasSameConnectionStatus = shownPlayers.Select(player => player.IsOnline)
                .SequenceEqual(receivedPlayers.Select(player => player.IsOnline));
            bool showsSameRows = hasSamePlayers && hasSameConnectionStatus;

            return showsSameRows;
        }

        private void FollowFriendshipListRequest()
        {
            FinishedRequest<FriendshipListResponse>? finishedFriendshipList = _friendshipListRequest.TakeFinished();
            if (finishedFriendshipList is null)
            {
                return;
            }

            if (finishedFriendshipList.IsSuccessful && (finishedFriendshipList.Reply is FriendshipListResponse receivedFriendshipList))
            {
                _needsRender |= !ShowsSameRows(FriendshipList, receivedFriendshipList);
                FriendshipList = receivedFriendshipList;
            }
            else
            {
                Fail();
            }
        }

        private void FollowSearch()
        {
            PlayerSearchUpdate searchUpdate = _playerSearch.TakeUpdate();
            if (searchUpdate == PlayerSearchUpdate.ResultChanged)
            {
                _needsRender = true;
            }
            else if (searchUpdate == PlayerSearchUpdate.Failed)
            {
                Fail();
            }
        }

        private void FollowFriendRequest()
        {
            FinishedRequest<FriendRequestResult>? finishedRequest = _friendRequest.TakeFinished();
            if (finishedRequest is null)
            {
                return;
            }

            if (finishedRequest.IsSuccessful)
            {
                FollowFriendRequestResult(finishedRequest.Reply);
            }
            else
            {
                Fail();
            }
        }

        private void FollowFriendRequestResult(FriendRequestResult requestResult)
        {
            switch (requestResult)
            {
                case FriendRequestResult.Sent:
                    _playerSearch.Clear();
                    _showsSentRequests = true;
                    StartFriendshipListRequest();
                    break;

                case FriendRequestResult.AlreadyFriends:
                case FriendRequestResult.RequestAlreadyPending:
                    _playerSearch.Repeat();
                    StartFriendshipListRequest();
                    break;

                case FriendRequestResult.PlayerNotFound:
                    _playerSearch.ShowNotFound();
                    _needsRender = true;
                    break;

                case FriendRequestResult.DatabaseUnavailable:
                    Fail();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(requestResult), requestResult, null);
            }
        }

        private void FollowResponse()
        {
            FinishedRequest<FriendRequestResponseResult>? finishedResponse = _responseRequest.TakeFinished();
            if (finishedResponse is null)
            {
                return;
            }

            bool isAnswered = finishedResponse.IsSuccessful
                && (finishedResponse.Reply != FriendRequestResponseResult.DatabaseUnavailable);
            if (isAnswered)
            {
                StartFriendshipListRequest();
            }
            else
            {
                Fail();
            }
        }

        private void FollowRemoval()
        {
            FinishedRequest<FriendRemovalResult>? finishedRemoval = _removalRequest.TakeFinished();
            if (finishedRemoval is null)
            {
                return;
            }

            bool isRemoved = finishedRemoval.IsSuccessful
                && (finishedRemoval.Reply != FriendRemovalResult.DatabaseUnavailable);
            if (isRemoved)
            {
                StartFriendshipListRequest();
            }
            else
            {
                Fail();
            }
        }

        private void StartFriendshipListRequest()
        {
            _sinceFriendshipList = TimeSpan.Zero;
            if (_gameSessionConnection.IsSessionOpen)
            {
                _friendshipListRequest.Forget();
                _friendshipListRequest.Start(_gameSessionConnection.GetFriendshipListAsync());
            }
        }

        private void Fail()
        {
            ForgetRequests();
            _hasFailed = true;
        }

        internal void ForgetRequests()
        {
            _friendshipListRequest.Forget();
            _playerSearch.Forget();
            _friendRequest.Forget();
            _responseRequest.Forget();
            _removalRequest.Forget();
        }
    }
}
