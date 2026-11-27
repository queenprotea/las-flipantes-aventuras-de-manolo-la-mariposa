using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Torres.Client.Avatars;
using Torres.Client.Contracts;
using Torres.Client.Threading;

namespace Torres.Client.Connection
{
    internal sealed class GameSessionConnection
    {
        private const string ClosedConnectionMessage = "The game session is not open.";
        private const string NoSessionTokenMessage = "There is no session to reopen.";

        private readonly GameSessionNoticeQueue _noticeQueue = new GameSessionNoticeQueue();
        private readonly DuplexChannelFactory<IGameSessionService> _channelFactory;
        private readonly TimeSpan _sessionCloseTimeout;

        private IGameSessionService? _sessionChannel;
        private IGameSessionService? _closingChannel;
        private Task<CloseSessionResult>? _pendingSessionClose;
        private TimeSpan _sinceSessionCloseStarted;
        private string? _sessionToken;
        private Task<ConnectionCheckResponse>? _pendingConnectionCheck;
        private Task? _pendingUploadCancellation;
        private TimeSpan _connectionCheckPeriod;
        private TimeSpan _sinceLastConnectionCheck;

        internal GameSessionConnection(ClientSettings clientSettings)
        {
            _channelFactory = new DuplexChannelFactory<IGameSessionService>(
                new InstanceContext(_noticeQueue),
                CreateSessionBinding(),
                clientSettings.CreateEndpointAddress(clientSettings.ServicePaths.GameSession));
            _sessionCloseTimeout = clientSettings.SessionCloseTimeout;
        }

        internal bool IsSessionOpen { get; private set; }

        internal bool WasConnectionLost { get; private set; }

        internal TimeSpan ConnectionCheckPeriod => _connectionCheckPeriod;

        internal Task<OpenSessionResult> OpenAsync(string sessionToken)
        {
            AbortSessionChannel();
            _sessionToken = sessionToken;
            IGameSessionService openingChannel = _channelFactory.CreateChannel();
            _sessionChannel = openingChannel;
            Task<OpenSessionResult> sessionOpening = new BackgroundCall<string, OpenSessionResult>(openingChannel.OpenSession, sessionToken).StartAsync();

            return sessionOpening;
        }

        internal Task<OpenSessionResult> ReopenAsync()
        {
            string sessionToken = _sessionToken ?? throw new InvalidOperationException(NoSessionTokenMessage);
            Task<OpenSessionResult> sessionOpening = OpenAsync(sessionToken);

            return sessionOpening;
        }

        internal void BeginConnectionChecks(OpenSessionResult openedSession)
        {
            _connectionCheckPeriod = TimeSpan.FromSeconds(openedSession.ConnectionCheckPeriodSeconds);
            _sinceLastConnectionCheck = TimeSpan.Zero;
            IsSessionOpen = true;
            WasConnectionLost = false;
        }

        internal void CheckConnection(GameTime gameTime)
        {
            FollowSessionClose(gameTime.ElapsedGameTime);
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
            Task<RoomListResponse> roomSearch = new BackgroundCall<string, RoomListResponse>(RequireChannel().FindRoom, roomCode).StartAsync();

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
            Task<KickMemberResult> memberKick = new BackgroundCall<int, KickMemberResult>(RequireChannel().KickMember, targetMemberId).StartAsync();

            return memberKick;
        }

        internal Task<SendChatMessageResult> SendChatMessageAsync(string messageText)
        {
            Task<SendChatMessageResult> messageSending = new BackgroundCall<string, SendChatMessageResult>(RequireChannel().SendChatMessage, messageText).StartAsync();

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
            Task<PlayerSearchResponse> playerSearch = new BackgroundCall<string, PlayerSearchResponse>(RequireChannel().FindPlayerByUsername, username).StartAsync();

            return playerSearch;
        }

        internal Task<FriendRequestResult> SendFriendRequestAsync(int targetPlayerId)
        {
            Task<FriendRequestResult> friendRequestSending = new BackgroundCall<int, FriendRequestResult>(RequireChannel().SendFriendRequest, targetPlayerId).StartAsync();

            return friendRequestSending;
        }

        internal Task<FriendRequestResponseResult> AcceptFriendRequestAsync(int requesterPlayerId)
        {
            Task<FriendRequestResponseResult> friendRequestAcceptance = new BackgroundCall<int, FriendRequestResponseResult>(AcceptFriendRequest, requesterPlayerId).StartAsync();

            return friendRequestAcceptance;
        }

        internal Task<FriendRequestResponseResult> RejectFriendRequestAsync(int requesterPlayerId)
        {
            Task<FriendRequestResponseResult> friendRequestRejection = new BackgroundCall<int, FriendRequestResponseResult>(RejectFriendRequest, requesterPlayerId).StartAsync();

            return friendRequestRejection;
        }

        internal Task<FriendRemovalResult> RemoveFriendAsync(int friendPlayerId)
        {
            Task<FriendRemovalResult> friendRemoval = new BackgroundCall<int, FriendRemovalResult>(RequireChannel().RemoveFriend, friendPlayerId).StartAsync();

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

        internal Task<SendInvitationResult> SendInvitationByEmailAsync(int targetPlayerId)
        {
            Task<SendInvitationResult> invitationSending = RequireChannel().SendInvitationByEmailAsync(targetPlayerId);

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

        internal Task<ForfeitResult> ForfeitAsync()
        {
            Task<ForfeitResult> forfeitRequest = Task.Run(RequireChannel().Forfeit);

            return forfeitRequest;
        }

        internal Task<ReturnToMatchResult> ReturnToMatchAsync()
        {
            Task<ReturnToMatchResult> returnRequest = Task.Run(RequireChannel().ReturnToMatch);

            return returnRequest;
        }

        internal Task<MatchActionResult> PlacePieceAsync(PiecePlacement piecePlacement)
        {
            Task<MatchActionResult> pieceRequest = new BackgroundCall<PiecePlacement, MatchActionResult>(PlacePiece, piecePlacement).StartAsync();

            return pieceRequest;
        }

        internal Task<MatchActionResult> MoveCaterpillarAsync(MatchMove move)
        {
            Task<MatchActionResult> moveRequest = new BackgroundCall<MatchMove, MatchActionResult>(MoveCaterpillar, move).StartAsync();

            return moveRequest;
        }

        internal Task<MatchActionResult> AddFlowerLevelAsync(MatchCell buildCell)
        {
            Task<MatchActionResult> buildRequest = new BackgroundCall<MatchCell, MatchActionResult>(AddFlowerLevel, buildCell).StartAsync();

            return buildRequest;
        }

        internal Task<MatchActionResult> DrawCardAsync()
        {
            Task<MatchActionResult> drawRequest = Task.Run(RequireChannel().DrawCard);

            return drawRequest;
        }

        internal Task<MatchActionResult> PlayCardAsync(MatchCardPlay cardPlay)
        {
            Task<MatchActionResult> playRequest = new BackgroundCall<MatchCardPlay, MatchActionResult>(PlayCard, cardPlay).StartAsync();

            return playRequest;
        }

        internal Task<MatchActionResult> EndTurnAsync()
        {
            Task<MatchActionResult> endTurnRequest = Task.Run(RequireChannel().EndTurn);

            return endTurnRequest;
        }

        internal Task<ProfileResponse> GetProfileAsync()
        {
            Task<ProfileResponse> profileReading = RequireChannel().GetProfileAsync();

            return profileReading;
        }

        internal Task<BeginAvatarUploadResult> BeginAvatarUploadAsync(PreparedAvatar preparedAvatar)
        {
            Task<BeginAvatarUploadResult> uploadBeginning = new BackgroundCall<PreparedAvatar, BeginAvatarUploadResult>(BeginAvatarUpload, preparedAvatar).StartAsync();

            return uploadBeginning;
        }

        internal Task<SendAvatarPartResult> SendAvatarPartAsync(AvatarPart avatarPart)
        {
            Task<SendAvatarPartResult> partSending = new BackgroundCall<AvatarPart, SendAvatarPartResult>(SendAvatarPart, avatarPart).StartAsync();

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

        internal Task<DeleteAccountResult> DeleteAccountAsync()
        {
            Task<DeleteAccountResult> accountDeletion = RequireChannel().DeleteAccountAsync();

            return accountDeletion;
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
                AbortClosingChannel();
                _closingChannel = _sessionChannel;
                _pendingSessionClose = _sessionChannel.CloseSessionAsync();
                _sinceSessionCloseStarted = TimeSpan.Zero;
                _sessionChannel = null;
            }

            AbandonSession();
        }

        internal void AbandonSession()
        {
            AbortSessionChannel();
            _sessionToken = null;
        }

        internal void CloseSessionAndFactory()
        {
            if (IsSessionOpen && (_sessionChannel is not null))
            {
                Task<CloseSessionResult> sessionClosing = _sessionChannel.CloseSessionAsync();
                Task.WaitAny(new Task[] { sessionClosing }, _sessionCloseTimeout);
            }

            AbandonSession();
            AbortClosingChannel();
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

        private void FollowSessionClose(TimeSpan elapsedTime)
        {
            if (_pendingSessionClose is null)
            {
                return;
            }

            _sinceSessionCloseStarted += elapsedTime;
            if (_pendingSessionClose.IsCompleted || (_sinceSessionCloseStarted >= _sessionCloseTimeout))
            {
                AbortClosingChannel();
            }
        }

        private void AbortClosingChannel()
        {
            if (_closingChannel is ICommunicationObject closingChannel)
            {
                closingChannel.Abort();
            }

            _closingChannel = null;
            _pendingSessionClose = null;
        }

        private void FollowConnectionCheck()
        {
            if ((_pendingConnectionCheck is null) || !_pendingConnectionCheck.IsCompleted)
            {
                return;
            }

            WasConnectionLost = !_pendingConnectionCheck.IsCompletedSuccessfully;
            if (!WasConnectionLost && !string.IsNullOrEmpty(_pendingConnectionCheck.Result.SessionToken))
            {
                _sessionToken = _pendingConnectionCheck.Result.SessionToken;
            }

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

        private MatchActionResult PlayCard(MatchCardPlay cardPlay)
        {
            MatchActionResult playResult = RequireChannel().PlayCard(cardPlay);

            return playResult;
        }

        private MatchActionResult PlacePiece(PiecePlacement piecePlacement)
        {
            MatchActionResult placementResult = RequireChannel().PlacePiece(piecePlacement.Piece, piecePlacement.DestinationCell);

            return placementResult;
        }

        private MatchActionResult MoveCaterpillar(MatchMove move)
        {
            MatchActionResult moveResult = RequireChannel().MoveCaterpillar(move);

            return moveResult;
        }

        private MatchActionResult AddFlowerLevel(MatchCell buildCell)
        {
            MatchActionResult buildResult = RequireChannel().AddFlowerLevel(buildCell);

            return buildResult;
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
