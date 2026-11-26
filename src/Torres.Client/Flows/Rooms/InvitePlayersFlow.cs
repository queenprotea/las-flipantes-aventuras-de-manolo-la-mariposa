using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class InvitePlayersFlow
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly TimeSpan _invitationStateRefreshPeriod;
        private readonly HashSet<int> _invitedPlayerIds = new HashSet<int>();
        private readonly PendingRequest<FriendshipListResponse> _friendshipListRequest = new PendingRequest<FriendshipListResponse>();
        private readonly PendingRequest<RoomInviteesResponse> _inviteesRequest = new PendingRequest<RoomInviteesResponse>();
        private readonly PendingRequest<PlayerSearchResponse> _searchRequest = new PendingRequest<PlayerSearchResponse>();
        private readonly PendingRequest<SendInvitationResult> _invitationRequest = new PendingRequest<SendInvitationResult>();

        private int _invitedPlayerId;
        private TimeSpan _sinceRefresh;
        private bool _needsRender;
        private string? _errorKey;
        private ScreenId? _nextScreen;

        internal InvitePlayersFlow(GameSessionConnection gameSessionConnection, PlayerSession playerSession, TimeSpan invitationStateRefreshPeriod)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
            _invitationStateRefreshPeriod = invitationStateRefreshPeriod;
        }

        internal IReadOnlyList<PlayerSummary> Friends { get; private set; } = new List<PlayerSummary>();

        internal PlayerSearchResponse? SearchResult { get; private set; }

        internal string? RoomCode => _playerSession.CurrentRoom?.Code;

        internal ScreenId? Open()
        {
            ForgetRequests();
            Friends = new List<PlayerSummary>();
            _invitedPlayerIds.Clear();
            SearchResult = null;
            ScreenId? redirect = null;
            if (!_playerSession.HasAccount || !_gameSessionConnection.IsSessionOpen)
            {
                redirect = ScreenId.Login;
            }
            else if (_playerSession.CurrentRoom is null)
            {
                redirect = ScreenId.MainMenu;
            }
            else
            {
                StartRefresh();
            }

            return redirect;
        }

        internal void Advance(TimeSpan elapsedTime)
        {
            _sinceRefresh += elapsedTime;
            bool isRefreshDue = (_sinceRefresh >= _invitationStateRefreshPeriod)
                && !_friendshipListRequest.IsWaiting
                && !_inviteesRequest.IsWaiting;
            if (isRefreshDue)
            {
                StartRefresh();
            }
        }

        internal InvitePlayersOutcome? TakeOutcome()
        {
            FollowFriendshipListRequest();
            FollowInviteesRequest();
            FollowSearch();
            FollowInvitationRequest();
            InvitePlayersOutcome? outcome = null;
            if (_needsRender || (_errorKey is not null) || (_nextScreen is not null))
            {
                outcome = new InvitePlayersOutcome(_needsRender, _errorKey, _nextScreen);
            }

            _needsRender = false;
            _errorKey = null;
            _nextScreen = null;

            return outcome;
        }

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
                SearchResult = new PlayerSearchResponse { Status = PlayerSearchStatus.NotFound };
                _needsRender = true;
            }
            else if (!_searchRequest.IsWaiting && _gameSessionConnection.IsSessionOpen)
            {
                _searchRequest.Start(_gameSessionConnection.FindPlayerByUsernameAsync(username));
            }

            return errorKey;
        }

        internal bool Invite(int targetPlayerId, bool isByEmail)
        {
            bool canSend = _gameSessionConnection.IsSessionOpen && !_invitationRequest.IsWaiting;
            if (canSend)
            {
                _invitedPlayerId = targetPlayerId;
                Task<SendInvitationResult> invitation = isByEmail
                    ? _gameSessionConnection.SendInvitationByEmailAsync(targetPlayerId)
                    : _gameSessionConnection.SendInvitationAsync(targetPlayerId);
                _invitationRequest.Start(invitation);
            }

            return canSend;
        }

        internal bool IsInvited(int playerId)
        {
            bool isInvited = _invitedPlayerIds.Contains(playerId);

            return isInvited;
        }

        internal bool IsRoomMember(string username)
        {
            bool isRoomMember = _playerSession.CurrentRoom?.Members
                .Any(member => !member.IsGuest && string.Equals(member.DisplayName, username, StringComparison.OrdinalIgnoreCase))
                ?? false;

            return isRoomMember;
        }

        internal void ForgetRequests()
        {
            _friendshipListRequest.Forget();
            _inviteesRequest.Forget();
            _searchRequest.Forget();
            _invitationRequest.Forget();
        }

        private static bool ShowsSameFriends(IReadOnlyList<PlayerSummary> shownFriends, IReadOnlyList<PlayerSummary> receivedFriends)
        {
            bool hasSamePlayers = shownFriends.Select(friend => friend.PlayerId)
                .SequenceEqual(receivedFriends.Select(friend => friend.PlayerId));
            bool hasSameConnectionStatus = shownFriends.Select(friend => friend.IsOnline)
                .SequenceEqual(receivedFriends.Select(friend => friend.IsOnline));
            bool showsSameFriends = hasSamePlayers && hasSameConnectionStatus;

            return showsSameFriends;
        }

        private void StartRefresh()
        {
            _sinceRefresh = TimeSpan.Zero;
            if (_gameSessionConnection.IsSessionOpen)
            {
                _friendshipListRequest.Forget();
                _friendshipListRequest.Start(_gameSessionConnection.GetFriendshipListAsync());
                _inviteesRequest.Forget();
                _inviteesRequest.Start(_gameSessionConnection.GetRoomInviteesAsync());
            }
        }

        private void FollowFriendshipListRequest()
        {
            FinishedRequest<FriendshipListResponse>? finishedFriendshipList = _friendshipListRequest.TakeFinished();
            if ((finishedFriendshipList is not null) && finishedFriendshipList.IsSuccessful && (finishedFriendshipList.Reply is FriendshipListResponse receivedFriendshipList))
            {
                List<PlayerSummary> receivedFriends = receivedFriendshipList.Friends;
                _needsRender |= !ShowsSameFriends(Friends, receivedFriends);
                Friends = receivedFriends;
            }
            else if (finishedFriendshipList is not null)
            {
                Fail();
            }
        }

        private void FollowInviteesRequest()
        {
            FinishedRequest<RoomInviteesResponse>? finishedInvitees = _inviteesRequest.TakeFinished();
            if ((finishedInvitees is not null) && finishedInvitees.IsSuccessful && (finishedInvitees.Reply is RoomInviteesResponse receivedInvitees))
            {
                _needsRender |= !_invitedPlayerIds.SetEquals(receivedInvitees.InvitedPlayerIds);
                _invitedPlayerIds.Clear();
                _invitedPlayerIds.UnionWith(receivedInvitees.InvitedPlayerIds);
            }
            else if (finishedInvitees is not null)
            {
                Fail();
            }
        }

        private void FollowSearch()
        {
            FinishedRequest<PlayerSearchResponse>? finishedSearch = _searchRequest.TakeFinished();
            if ((finishedSearch is not null) && finishedSearch.IsSuccessful && (finishedSearch.Reply is PlayerSearchResponse foundPlayers))
            {
                SearchResult = foundPlayers;
                _needsRender = true;
            }
            else if (finishedSearch is not null)
            {
                Fail();
            }
        }

        private void FollowInvitationRequest()
        {
            FinishedRequest<SendInvitationResult>? finishedInvitation = _invitationRequest.TakeFinished();
            if ((finishedInvitation is not null) && finishedInvitation.IsSuccessful)
            {
                FollowInvitationResult(finishedInvitation.Reply);
            }
            else if (finishedInvitation is not null)
            {
                Fail();
            }
        }

        private void FollowInvitationResult(SendInvitationResult invitationResult)
        {
            switch (invitationResult)
            {
                case SendInvitationResult.Sent:
                case SendInvitationResult.InvitationAlreadyPending:
                    _invitedPlayerIds.Add(_invitedPlayerId);
                    _needsRender = true;
                    break;

                case SendInvitationResult.EmailNotSent:
                    _invitedPlayerIds.Add(_invitedPlayerId);
                    _needsRender = true;
                    _errorKey = TextKeys.Alerts.EmailFailedBody;
                    break;

                case SendInvitationResult.AlreadyInThisRoom:
                    StartRefresh();
                    _needsRender = true;
                    break;

                case SendInvitationResult.PlayerNotFound:
                    ForgetMissingInvitee();
                    break;

                case SendInvitationResult.NotInRoom:
                    ForgetRequests();
                    _nextScreen = ScreenId.MainMenu;
                    break;

                case SendInvitationResult.DatabaseUnavailable:
                    Fail();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(invitationResult), invitationResult, null);
            }
        }

        private void ForgetMissingInvitee()
        {
            if (SearchResult?.Player?.PlayerId == _invitedPlayerId)
            {
                SearchResult = new PlayerSearchResponse { Status = PlayerSearchStatus.NotFound };
            }

            StartRefresh();
            _needsRender = true;
        }

        private void Fail()
        {
            ForgetRequests();
            _errorKey = TextKeys.Common.ServerErrorTitle;
        }
    }
}
