using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

namespace Torres.Client.Session
{
    internal sealed class GameSessionConnection
    {
        private const string ClosedConnectionMessage = "The game session is not open.";

        private static readonly TimeSpan _sessionCloseTimeout = TimeSpan.FromSeconds(2);

        private readonly GameSessionNoticeQueue _noticeQueue = new GameSessionNoticeQueue();
        private readonly DuplexChannelFactory<IGameSessionService> _channelFactory;

        private IGameSessionService? _sessionChannel;
        private Task<ConnectionCheckResponse>? _pendingConnectionCheck;
        private TimeSpan _connectionCheckPeriod;
        private TimeSpan _sinceLastConnectionCheck;

        internal GameSessionConnection(ClientSettings clientSettings)
        {
            ArgumentNullException.ThrowIfNull(clientSettings);

            _channelFactory = new DuplexChannelFactory<IGameSessionService>(
                new InstanceContext(_noticeQueue),
                new NetTcpBinding(SecurityMode.None),
                clientSettings.CreateEndpointAddress(ServiceEndpointPaths.GameSession));
        }

        internal bool IsSessionOpen { get; private set; }

        internal bool WasConnectionLost { get; private set; }

        internal Task<OpenSessionResult> OpenAsync(string sessionToken)
        {
            ArgumentNullException.ThrowIfNull(sessionToken);

            AbortSessionChannel();
            IGameSessionService openingChannel = _channelFactory.CreateChannel();
            _sessionChannel = openingChannel;
            Task<OpenSessionResult> sessionOpening = new SessionCall<string, OpenSessionResult>(openingChannel.OpenSession, sessionToken).StartAsync();

            return sessionOpening;
        }

        internal void BeginConnectionChecks(OpenSessionResult openedSession)
        {
            ArgumentNullException.ThrowIfNull(openedSession);

            _connectionCheckPeriod = TimeSpan.FromSeconds(openedSession.ConnectionCheckPeriodSeconds);
            _sinceLastConnectionCheck = TimeSpan.Zero;
            IsSessionOpen = true;
            WasConnectionLost = false;
        }

        internal void CheckConnection(GameTime gameTime)
        {
            if (!IsSessionOpen || (_sessionChannel is null))
            {
                return;
            }

            _sinceLastConnectionCheck += gameTime.ElapsedGameTime;
            FollowConnectionCheck();
            bool isConnectionCheckDue = (_pendingConnectionCheck is null) && (_sinceLastConnectionCheck >= _connectionCheckPeriod);
            if (isConnectionCheckDue)
            {
                _sinceLastConnectionCheck = TimeSpan.Zero;
                _pendingConnectionCheck = Task.Run(_sessionChannel.IsAlive);
            }

            if (_sessionChannel is ICommunicationObject { State: CommunicationState.Faulted or CommunicationState.Closed })
            {
                WasConnectionLost = true;
            }
        }

        internal RoomChange? TakeRoomChange()
        {
            RoomChange? roomChange = _noticeQueue.TakeRoomChange();

            return roomChange;
        }

        internal SessionTerminationReason? TakeTerminationReason()
        {
            SessionTerminationReason? terminationReason = _noticeQueue.TakeTerminationReason();

            return terminationReason;
        }

        internal ChatMessage? TakeChatMessage()
        {
            ChatMessage? chatMessage = _noticeQueue.TakeChatMessage();

            return chatMessage;
        }

        internal MatchChange? TakeMatchChange()
        {
            MatchChange? matchChange = _noticeQueue.TakeMatchChange();

            return matchChange;
        }

        internal Task<CreateRoomResult> CreateRoomAsync(RoomVisibility roomVisibility)
        {
            Task<CreateRoomResult> roomCreation = RequireChannel().CreateRoomAsync(roomVisibility);

            return roomCreation;
        }

        internal Task<RoomListResponse> GetRoomsAsync()
        {
            Task<RoomListResponse> roomListing = Task.Run(RequireChannel().GetRooms);

            return roomListing;
        }

        internal Task<RoomListResponse> FindRoomAsync(string roomCode)
        {
            Task<RoomListResponse> roomSearch = new SessionCall<string, RoomListResponse>(RequireChannel().FindRoom, roomCode).StartAsync();

            return roomSearch;
        }

        internal Task<JoinRoomResult> JoinRoomAsync(int roomId)
        {
            Task<JoinRoomResult> roomJoining = RequireChannel().JoinRoomAsync(roomId);

            return roomJoining;
        }

        internal Task<LeaveRoomResult> LeaveRoomAsync()
        {
            Task<LeaveRoomResult> roomLeaving = RequireChannel().LeaveRoomAsync();

            return roomLeaving;
        }

        internal Task<KickMemberResult> KickMemberAsync(int targetMemberId)
        {
            Task<KickMemberResult> memberKick = new SessionCall<int, KickMemberResult>(RequireChannel().KickMember, targetMemberId).StartAsync();

            return memberKick;
        }

        internal Task<SendChatMessageResult> SendChatMessageAsync(string messageText)
        {
            Task<SendChatMessageResult> messageSending = new SessionCall<string, SendChatMessageResult>(RequireChannel().SendChatMessage, messageText).StartAsync();

            return messageSending;
        }

        internal Task<ChatHistoryResponse> GetChatHistoryAsync()
        {
            Task<ChatHistoryResponse> chatHistoryReading = Task.Run(RequireChannel().GetChatHistory);

            return chatHistoryReading;
        }

        internal Task<MatchHistoryResponse> GetMatchHistoryAsync(int pageNumber)
        {
            Task<MatchHistoryResponse> matchHistoryReading = RequireChannel().GetMatchHistoryAsync(pageNumber);

            return matchHistoryReading;
        }

        internal Task<MatchDetailResponse> GetMatchDetailAsync(int matchId)
        {
            Task<MatchDetailResponse> matchDetailReading = RequireChannel().GetMatchDetailAsync(matchId);

            return matchDetailReading;
        }

        internal Task<FriendshipListResponse> GetFriendshipListAsync()
        {
            Task<FriendshipListResponse> friendshipListReading = Task.Run(RequireChannel().GetFriendshipList);

            return friendshipListReading;
        }

        internal Task<PlayerSearchResponse> FindPlayerByUsernameAsync(string username)
        {
            Task<PlayerSearchResponse> playerSearch = new SessionCall<string, PlayerSearchResponse>(RequireChannel().FindPlayerByUsername, username).StartAsync();

            return playerSearch;
        }

        internal Task<FriendRequestResult> SendFriendRequestAsync(int targetPlayerId)
        {
            Task<FriendRequestResult> friendRequestSending = new SessionCall<int, FriendRequestResult>(RequireChannel().SendFriendRequest, targetPlayerId).StartAsync();

            return friendRequestSending;
        }

        internal Task<FriendRequestResponseResult> AcceptFriendRequestAsync(int requesterPlayerId)
        {
            Task<FriendRequestResponseResult> friendRequestAcceptance = new SessionCall<int, FriendRequestResponseResult>(AcceptFriendRequest, requesterPlayerId).StartAsync();

            return friendRequestAcceptance;
        }

        internal Task<FriendRequestResponseResult> RejectFriendRequestAsync(int requesterPlayerId)
        {
            Task<FriendRequestResponseResult> friendRequestRejection = new SessionCall<int, FriendRequestResponseResult>(RejectFriendRequest, requesterPlayerId).StartAsync();

            return friendRequestRejection;
        }

        internal Task<FriendRemovalResult> RemoveFriendAsync(int friendPlayerId)
        {
            Task<FriendRemovalResult> friendRemoval = new SessionCall<int, FriendRemovalResult>(RequireChannel().RemoveFriend, friendPlayerId).StartAsync();

            return friendRemoval;
        }

        internal Task<StartMatchResult> StartMatchAsync()
        {
            Task<StartMatchResult> matchStart = RequireChannel().StartMatchAsync();

            return matchStart;
        }

        internal Task<EndMatchResult> EndMatchAsync()
        {
            Task<EndMatchResult> matchEnding = Task.Run(RequireChannel().EndMatch);

            return matchEnding;
        }

        internal Task<PlacementResult> PlaceCaterpillarAsync(MatchCell destinationCell)
        {
            Task<PlacementResult> caterpillarPlacement = new SessionCall<MatchCell, PlacementResult>(PlaceCaterpillar, destinationCell).StartAsync();

            return caterpillarPlacement;
        }

        internal Task<PlacementResult> PlaceButterflyAsync(MatchCell destinationCell)
        {
            Task<PlacementResult> butterflyPlacement = new SessionCall<MatchCell, PlacementResult>(PlaceButterfly, destinationCell).StartAsync();

            return butterflyPlacement;
        }

        internal void CloseSession()
        {
            if (IsSessionOpen && (_sessionChannel is not null))
            {
                Task<CloseSessionResult> sessionClosing = _sessionChannel.CloseSessionAsync();
                Task.WaitAny(new Task[] { sessionClosing }, _sessionCloseTimeout);
            }

            AbortSessionChannel();
        }

        internal void CloseSessionAndFactory()
        {
            CloseSession();
            _channelFactory.Abort();
        }

        internal void AbortSessionChannel()
        {
            if (_sessionChannel is ICommunicationObject abandonedChannel)
            {
                abandonedChannel.Abort();
            }

            _sessionChannel = null;
            _pendingConnectionCheck = null;
            IsSessionOpen = false;
            WasConnectionLost = false;
            _noticeQueue.Clear();
        }

        private void FollowConnectionCheck()
        {
            if ((_pendingConnectionCheck is null) || !_pendingConnectionCheck.IsCompleted)
            {
                return;
            }

            WasConnectionLost = !_pendingConnectionCheck.IsCompletedSuccessfully;
            _pendingConnectionCheck = null;
        }

        private FriendRequestResponseResult AcceptFriendRequest(int requesterPlayerId)
        {
            FriendRequestResponseResult acceptanceResult = RequireChannel().RespondFriendRequest(requesterPlayerId, true);

            return acceptanceResult;
        }

        private FriendRequestResponseResult RejectFriendRequest(int requesterPlayerId)
        {
            FriendRequestResponseResult rejectionResult = RequireChannel().RespondFriendRequest(requesterPlayerId, false);

            return rejectionResult;
        }

        private PlacementResult PlaceCaterpillar(MatchCell destinationCell)
        {
            PlacementResult caterpillarPlacementResult = RequireChannel().PlaceCaterpillar(destinationCell.Row, destinationCell.Column);

            return caterpillarPlacementResult;
        }

        private PlacementResult PlaceButterfly(MatchCell destinationCell)
        {
            PlacementResult butterflyPlacementResult = RequireChannel().PlaceButterfly(destinationCell.Row, destinationCell.Column);

            return butterflyPlacementResult;
        }

        private IGameSessionService RequireChannel()
        {
            if (!IsSessionOpen || (_sessionChannel is null))
            {
                throw new InvalidOperationException(ClosedConnectionMessage);
            }

            return _sessionChannel;
        }
    }
}
