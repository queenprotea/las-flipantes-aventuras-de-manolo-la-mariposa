using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Torres.Client.Avatars;

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
        private Task? _pendingUploadCancellation;
        private TimeSpan _connectionCheckPeriod;
        private TimeSpan _sinceLastConnectionCheck;

        internal GameSessionConnection(ClientSettings clientSettings)
        {
            ArgumentNullException.ThrowIfNull(clientSettings);

            _channelFactory = new DuplexChannelFactory<IGameSessionService>(
                new InstanceContext(_noticeQueue),
                CreateSessionBinding(),
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
            FollowUploadCancellation();
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

        internal RoomInvitation? TakeRoomInvitation()
        {
            RoomInvitation? roomInvitation = _noticeQueue.TakeRoomInvitation();

            return roomInvitation;
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

        internal Task<ReportChatMessageResult> ReportChatMessageAsync(ChatMessage reportedMessage)
        {
            Task<ReportChatMessageResult> messageReporting = RequireChannel().ReportChatMessageAsync(reportedMessage.RoomId, reportedMessage.MessageId);

            return messageReporting;
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

        internal Task<PendingInvitationsResponse> GetPendingInvitationsAsync()
        {
            Task<PendingInvitationsResponse> invitationsReading = RequireChannel().GetPendingInvitationsAsync();

            return invitationsReading;
        }

        internal Task<RoomInviteesResponse> GetRoomInviteesAsync()
        {
            Task<RoomInviteesResponse> inviteesReading = RequireChannel().GetRoomInviteesAsync();

            return inviteesReading;
        }

        internal Task<SendInvitationResult> SendInvitationAsync(int targetPlayerId)
        {
            Task<SendInvitationResult> invitationSending = RequireChannel().SendInvitationAsync(targetPlayerId);

            return invitationSending;
        }

        internal Task<RespondInvitationResult> AcceptInvitationAsync(int roomId)
        {
            Task<RespondInvitationResult> invitationAcceptance = RequireChannel().RespondInvitationAsync(roomId, true);

            return invitationAcceptance;
        }

        internal Task<RespondInvitationResult> RejectInvitationAsync(int roomId)
        {
            Task<RespondInvitationResult> invitationRejection = RequireChannel().RespondInvitationAsync(roomId, false);

            return invitationRejection;
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

        internal Task<ProfileResponse> GetProfileAsync()
        {
            Task<ProfileResponse> profileReading = RequireChannel().GetProfileAsync();

            return profileReading;
        }

        internal Task<BeginAvatarUploadResult> BeginAvatarUploadAsync(PreparedAvatar preparedAvatar)
        {
            Task<BeginAvatarUploadResult> uploadBeginning = new SessionCall<PreparedAvatar, BeginAvatarUploadResult>(BeginAvatarUpload, preparedAvatar).StartAsync();

            return uploadBeginning;
        }

        internal Task<SendAvatarPartResult> SendAvatarPartAsync(AvatarPart avatarPart)
        {
            Task<SendAvatarPartResult> partSending = new SessionCall<AvatarPart, SendAvatarPartResult>(SendAvatarPart, avatarPart).StartAsync();

            return partSending;
        }

        internal void RequestAvatarUploadCancellation()
        {
            _pendingUploadCancellation = Task.Run(CancelAvatarUpload);
        }

        internal Task<UpdateProfileResult> UpdateProfileAsync(string username, bool includesNewAvatar)
        {
            Task<UpdateProfileResult> profileUpdate = RequireChannel().UpdateProfileAsync(username, includesNewAvatar);

            return profileUpdate;
        }

        internal Task<AvatarResponse> GetAvatarThumbnailAsync(string avatarReference)
        {
            Task<AvatarResponse> thumbnailReading = RequireChannel().GetAvatarThumbnailAsync(avatarReference);

            return thumbnailReading;
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
            _pendingUploadCancellation = null;
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

        private void FollowUploadCancellation()
        {
            if ((_pendingUploadCancellation is not null) && _pendingUploadCancellation.IsCompleted)
            {
                _pendingUploadCancellation = null;
            }
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

        private static NetTcpBinding CreateSessionBinding()
        {
            var sessionBinding = new NetTcpBinding(SecurityMode.None);
            sessionBinding.ReaderQuotas.MaxArrayLength = ProfileLimits.LargestArrayBytes;

            return sessionBinding;
        }

        private BeginAvatarUploadResult BeginAvatarUpload(PreparedAvatar preparedAvatar)
        {
            BeginAvatarUploadResult beginResult = RequireChannel().BeginAvatarUpload(preparedAvatar.Original.Length, preparedAvatar.Thumbnail.Length);

            return beginResult;
        }

        private SendAvatarPartResult SendAvatarPart(AvatarPart avatarPart)
        {
            SendAvatarPartResult partResult = RequireChannel().SendAvatarPart(avatarPart.FileKind, avatarPart.Offset, avatarPart.Bytes);

            return partResult;
        }

        private void CancelAvatarUpload()
        {
            RequireChannel().CancelAvatarUpload();
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
