using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Torres.Client.Avatars;
using Torres.Client.Threading;

namespace Torres.Client.Connection
{
    internal sealed class GameSessionConnection
    {
        private const string ClosedConnectionMessage = "The game session is not open.";
        private const string NoSessionTokenMessage = "There is no session to reopen.";
        private const int NoFailedCall = -1;

        private readonly GameSessionNoticeQueue _noticeQueue = new GameSessionNoticeQueue();
        private readonly DuplexChannelFactory<IGameSessionService> _channelFactory;
        private readonly TimeSpan _sessionCloseTimeout;
        private readonly TimeSpan _stepSilenceMargin;

        private IGameSessionService? _sessionChannel;
        private IGameSessionService? _closingChannel;
        private Task<CloseSessionResult>? _pendingSessionClose;
        private TimeSpan _sinceSessionCloseStarted;
        private string? _sessionToken;
        private string? _matchReconnectionToken;
        private Task? _pendingUploadCancellation;
        private Task<RoomLookupResponse>? _pendingSilenceCheck;
        private MatchChange? _lastMatchChange;
        private bool _hasCheckedStepSilence;
        private int _channelGeneration;
        private int _failedCallGeneration = NoFailedCall;

        internal GameSessionConnection(ClientSettings clientSettings)
        {
            _channelFactory = new DuplexChannelFactory<IGameSessionService>(
                new InstanceContext(_noticeQueue),
                CreateSessionBinding(clientSettings),
                clientSettings.CreateEndpointAddress(clientSettings.ServicePaths.GameSession));
            _sessionCloseTimeout = clientSettings.SessionCloseTimeout;
            _stepSilenceMargin = clientSettings.StepSilenceMargin;
            ReconnectionRetryInterval = clientSettings.ReconnectionRetryInterval;
        }

        internal bool IsSessionOpen { get; private set; }

        internal bool WasConnectionLost { get; private set; }

        internal TimeSpan ReconnectionRetryInterval { get; }

        internal Task<OpenSessionResult> OpenAsync(string sessionToken)
        {
            _sessionToken = sessionToken;
            _matchReconnectionToken = null;
            Task<OpenSessionResult> sessionOpening = OpenWithAsync(sessionToken);

            return sessionOpening;
        }

        internal Task<OpenSessionResult> ReopenAsync()
        {
            string reopeningToken = _matchReconnectionToken ?? _sessionToken ?? throw new InvalidOperationException(NoSessionTokenMessage);
            Task<OpenSessionResult> sessionOpening = OpenWithAsync(reopeningToken);

            return sessionOpening;
        }

        internal void MarkSessionOpen()
        {
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

            FollowUploadCancellation();
            FollowStepSilence(_sessionChannel);
            bool hasCallFailed = Volatile.Read(ref _failedCallGeneration) == _channelGeneration;
            bool isChannelDown = _sessionChannel is ICommunicationObject { State: CommunicationState.Faulted or CommunicationState.Closed };
            if (hasCallFailed || isChannelDown)
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
            if (matchChange is not null)
            {
                FollowMatchChange(matchChange);
            }

            return matchChange;
        }

        internal RoomInvitation? TakeRoomInvitation()
        {
            RoomInvitation? roomInvitation = _noticeQueue.TakeRoomInvitation();

            return roomInvitation;
        }

        internal Task<CreateRoomResult> CreateRoomAsync(RoomVisibility roomVisibility)
        {
            Task<CreateRoomResult> roomCreation = ObserveAsync(RequireChannel().CreateRoomAsync(roomVisibility));

            return roomCreation;
        }

        internal Task<RoomListResponse> GetRoomsAsync()
        {
            Task<RoomListResponse> roomListing = ObserveAsync(Task.Run(RequireChannel().GetRooms));

            return roomListing;
        }

        internal Task<RoomListResponse> FindRoomAsync(string roomCode)
        {
            Task<RoomListResponse> roomSearch = ObserveAsync(new BackgroundCall<string, RoomListResponse>(RequireChannel().FindRoom, roomCode).StartAsync());

            return roomSearch;
        }

        internal Task<JoinRoomResult> JoinRoomAsync(int roomId)
        {
            Task<JoinRoomResult> roomJoining = ObserveAsync(RequireChannel().JoinRoomAsync(roomId));

            return roomJoining;
        }

        internal Task<LeaveRoomResult> LeaveRoomAsync()
        {
            Task<LeaveRoomResult> roomLeaving = ObserveAsync(RequireChannel().LeaveRoomAsync());

            return roomLeaving;
        }

        internal Task<KickMemberResult> KickMemberAsync(int targetMemberId)
        {
            Task<KickMemberResult> memberKick = ObserveAsync(new BackgroundCall<int, KickMemberResult>(RequireChannel().KickMember, targetMemberId).StartAsync());

            return memberKick;
        }

        internal Task<SendChatMessageResult> SendChatMessageAsync(string messageText)
        {
            Task<SendChatMessageResult> messageSending = ObserveAsync(new BackgroundCall<string, SendChatMessageResult>(RequireChannel().SendChatMessage, messageText).StartAsync());

            return messageSending;
        }

        internal Task<ChatHistoryResponse> GetChatHistoryAsync()
        {
            Task<ChatHistoryResponse> chatHistoryReading = ObserveAsync(Task.Run(RequireChannel().GetChatHistory));

            return chatHistoryReading;
        }

        internal Task<ReportChatMessageResult> ReportChatMessageAsync(ChatMessage reportedMessage)
        {
            Task<ReportChatMessageResult> messageReporting = ObserveAsync(RequireChannel().ReportChatMessageAsync(reportedMessage.RoomId, reportedMessage.MessageId));

            return messageReporting;
        }

        internal Task<MatchHistoryResponse> GetMatchHistoryAsync(int pageNumber)
        {
            Task<MatchHistoryResponse> matchHistoryReading = ObserveAsync(RequireChannel().GetMatchHistoryAsync(pageNumber));

            return matchHistoryReading;
        }

        internal Task<MatchDetailResponse> GetMatchDetailAsync(int matchId)
        {
            Task<MatchDetailResponse> matchDetailReading = ObserveAsync(RequireChannel().GetMatchDetailAsync(matchId));

            return matchDetailReading;
        }

        internal Task<FriendshipListResponse> GetFriendshipListAsync()
        {
            Task<FriendshipListResponse> friendshipListReading = ObserveAsync(Task.Run(RequireChannel().GetFriendshipList));

            return friendshipListReading;
        }

        internal Task<PlayerSearchResponse> FindPlayerByUsernameAsync(string username)
        {
            Task<PlayerSearchResponse> playerSearch = ObserveAsync(new BackgroundCall<string, PlayerSearchResponse>(RequireChannel().FindPlayerByUsername, username).StartAsync());

            return playerSearch;
        }

        internal Task<FriendRequestResult> SendFriendRequestAsync(int targetPlayerId)
        {
            Task<FriendRequestResult> friendRequestSending = ObserveAsync(new BackgroundCall<int, FriendRequestResult>(RequireChannel().SendFriendRequest, targetPlayerId).StartAsync());

            return friendRequestSending;
        }

        internal Task<FriendRequestResponseResult> AcceptFriendRequestAsync(int requesterPlayerId)
        {
            Task<FriendRequestResponseResult> friendRequestAcceptance = ObserveAsync(new BackgroundCall<int, FriendRequestResponseResult>(AcceptFriendRequest, requesterPlayerId).StartAsync());

            return friendRequestAcceptance;
        }

        internal Task<FriendRequestResponseResult> RejectFriendRequestAsync(int requesterPlayerId)
        {
            Task<FriendRequestResponseResult> friendRequestRejection = ObserveAsync(new BackgroundCall<int, FriendRequestResponseResult>(RejectFriendRequest, requesterPlayerId).StartAsync());

            return friendRequestRejection;
        }

        internal Task<FriendRemovalResult> RemoveFriendAsync(int friendPlayerId)
        {
            Task<FriendRemovalResult> friendRemoval = ObserveAsync(new BackgroundCall<int, FriendRemovalResult>(RequireChannel().RemoveFriend, friendPlayerId).StartAsync());

            return friendRemoval;
        }

        internal Task<PendingInvitationsResponse> GetPendingInvitationsAsync()
        {
            Task<PendingInvitationsResponse> invitationsReading = ObserveAsync(RequireChannel().GetPendingInvitationsAsync());

            return invitationsReading;
        }

        internal Task<RoomInviteesResponse> GetRoomInviteesAsync()
        {
            Task<RoomInviteesResponse> inviteesReading = ObserveAsync(RequireChannel().GetRoomInviteesAsync());

            return inviteesReading;
        }

        internal Task<SendInvitationResult> SendInvitationAsync(int targetPlayerId)
        {
            Task<SendInvitationResult> invitationSending = ObserveAsync(RequireChannel().SendInvitationAsync(targetPlayerId));

            return invitationSending;
        }

        internal Task<SendInvitationResult> SendInvitationByEmailAsync(int targetPlayerId)
        {
            Task<SendInvitationResult> invitationSending = ObserveAsync(RequireChannel().SendInvitationByEmailAsync(targetPlayerId));

            return invitationSending;
        }

        internal Task<RespondInvitationResult> AcceptInvitationAsync(int roomId)
        {
            Task<RespondInvitationResult> invitationAcceptance = ObserveAsync(RequireChannel().RespondInvitationAsync(roomId, true));

            return invitationAcceptance;
        }

        internal Task<RespondInvitationResult> RejectInvitationAsync(int roomId)
        {
            Task<RespondInvitationResult> invitationRejection = ObserveAsync(RequireChannel().RespondInvitationAsync(roomId, false));

            return invitationRejection;
        }

        internal Task<StartMatchResult> StartMatchAsync()
        {
            Task<StartMatchResult> matchStart = ObserveAsync(RequireChannel().StartMatchAsync());

            return matchStart;
        }

        internal Task<ForfeitResult> ForfeitAsync()
        {
            Task<ForfeitResult> forfeitRequest = ObserveAsync(Task.Run(RequireChannel().Forfeit));

            return forfeitRequest;
        }

        internal Task<ReturnToMatchResult> ReturnToMatchAsync()
        {
            Task<ReturnToMatchResult> returnRequest = ObserveAsync(Task.Run(RequireChannel().ReturnToMatch));

            return returnRequest;
        }

        internal Task<MatchActionResult> PlacePieceAsync(PiecePlacement piecePlacement)
        {
            Task<MatchActionResult> pieceRequest = ObserveAsync(new BackgroundCall<PiecePlacement, MatchActionResult>(PlacePiece, piecePlacement).StartAsync());

            return pieceRequest;
        }

        internal Task<MatchActionResult> MoveCaterpillarAsync(MatchMove move)
        {
            Task<MatchActionResult> moveRequest = ObserveAsync(new BackgroundCall<MatchMove, MatchActionResult>(MoveCaterpillar, move).StartAsync());

            return moveRequest;
        }

        internal Task<MatchActionResult> AddFlowerLevelAsync(MatchCell buildCell)
        {
            Task<MatchActionResult> buildRequest = ObserveAsync(new BackgroundCall<MatchCell, MatchActionResult>(AddFlowerLevel, buildCell).StartAsync());

            return buildRequest;
        }

        internal Task<MatchActionResult> DrawCardAsync()
        {
            Task<MatchActionResult> drawRequest = ObserveAsync(Task.Run(RequireChannel().DrawCard));

            return drawRequest;
        }

        internal Task<MatchActionResult> PlayCardAsync(MatchCardPlay cardPlay)
        {
            Task<MatchActionResult> playRequest = ObserveAsync(new BackgroundCall<MatchCardPlay, MatchActionResult>(PlayCard, cardPlay).StartAsync());

            return playRequest;
        }

        internal Task<MatchActionResult> EndTurnAsync()
        {
            Task<MatchActionResult> endTurnRequest = ObserveAsync(Task.Run(RequireChannel().EndTurn));

            return endTurnRequest;
        }

        internal Task<ProfileResponse> GetProfileAsync()
        {
            Task<ProfileResponse> profileReading = ObserveAsync(RequireChannel().GetProfileAsync());

            return profileReading;
        }

        internal Task<BeginAvatarUploadResult> BeginAvatarUploadAsync(PreparedAvatar preparedAvatar)
        {
            Task<BeginAvatarUploadResult> uploadBeginning = ObserveAsync(new BackgroundCall<PreparedAvatar, BeginAvatarUploadResult>(BeginAvatarUpload, preparedAvatar).StartAsync());

            return uploadBeginning;
        }

        internal Task<SendAvatarPartResult> SendAvatarPartAsync(AvatarPart avatarPart)
        {
            Task<SendAvatarPartResult> partSending = ObserveAsync(new BackgroundCall<AvatarPart, SendAvatarPartResult>(SendAvatarPart, avatarPart).StartAsync());

            return partSending;
        }

        internal void RequestAvatarUploadCancellation()
        {
            _pendingUploadCancellation = Task.Run(CancelAvatarUpload);
        }

        internal Task<UpdateProfileResult> UpdateProfileAsync(string username, bool includesNewAvatar)
        {
            Task<UpdateProfileResult> profileUpdate = ObserveAsync(RequireChannel().UpdateProfileAsync(username, includesNewAvatar));

            return profileUpdate;
        }

        internal Task<AvatarResponse> GetAvatarThumbnailAsync(string avatarReference)
        {
            Task<AvatarResponse> thumbnailReading = ObserveAsync(RequireChannel().GetAvatarThumbnailAsync(avatarReference));

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
            _matchReconnectionToken = null;
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
            _channelGeneration++;
            _pendingUploadCancellation = null;
            _pendingSilenceCheck = null;
            _lastMatchChange = null;
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

        private Task<OpenSessionResult> OpenWithAsync(string openingToken)
        {
            AbortSessionChannel();
            IGameSessionService openingChannel = _channelFactory.CreateChannel();
            _sessionChannel = openingChannel;
            Task<OpenSessionResult> sessionOpening = new BackgroundCall<string, OpenSessionResult>(openingChannel.OpenSession, openingToken).StartAsync();

            return sessionOpening;
        }

        private void FollowMatchChange(MatchChange matchChange)
        {
            _lastMatchChange = matchChange;
            _hasCheckedStepSilence = false;
            if (matchChange.MatchView.Phase == MatchPhase.Finished)
            {
                _matchReconnectionToken = null;
            }
            else if (!string.IsNullOrEmpty(matchChange.MatchView.ReconnectionToken))
            {
                _matchReconnectionToken = matchChange.MatchView.ReconnectionToken;
            }
        }

        private void FollowStepSilence(IGameSessionService sessionChannel)
        {
            if ((_pendingSilenceCheck is not null) && _pendingSilenceCheck.IsCompleted)
            {
                _pendingSilenceCheck = null;
            }

            if (IsStepSilent())
            {
                _hasCheckedStepSilence = true;
                _pendingSilenceCheck = ObserveAsync(Task.Run(sessionChannel.GetCurrentRoom));
            }
        }

        private bool IsStepSilent()
        {
            MatchChange? lastMatchChange = _lastMatchChange;
            bool isStepSilent = false;
            if ((_pendingSilenceCheck is null) && !_hasCheckedStepSilence && (lastMatchChange is not null))
            {
                isStepSilent = IsStepRunning(lastMatchChange.MatchView) && (DateTime.UtcNow >= FindSilenceLimit(lastMatchChange));
            }

            return isStepSilent;
        }

        private DateTime FindSilenceLimit(MatchChange lastMatchChange)
        {
            DateTime silenceLimitUtc = lastMatchChange.ReceivedAtUtc + lastMatchChange.MatchView.StepTimeLeft.GetValueOrDefault() + _stepSilenceMargin;

            return silenceLimitUtc;
        }

        private static bool IsStepRunning(MatchView matchView)
        {
            bool isStepRunning = (matchView.Phase is not (MatchPhase.Finished or MatchPhase.AwaitingPlayers)) && matchView.StepTimeLeft.HasValue;

            return isStepRunning;
        }

        private Task<TReply> ObserveAsync<TReply>(Task<TReply> serverCall)
        {
            _ = serverCall.ContinueWith(NoteFailedCall, _channelGeneration, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            return serverCall;
        }

        private void NoteFailedCall(Task failedCall, object? callGeneration)
        {
            Exception? failure = failedCall.Exception?.GetBaseException();
            bool isConnectionFailure = (failure is CommunicationException and not FaultException) || (failure is TimeoutException);
            if (isConnectionFailure && (callGeneration is int failedGeneration))
            {
                Volatile.Write(ref _failedCallGeneration, failedGeneration);
            }
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

        private static NetTcpBinding CreateSessionBinding(ClientSettings clientSettings)
        {
            var sessionBinding = new NetTcpBinding(SecurityMode.None);
            sessionBinding.ReaderQuotas.MaxArrayLength = ProfileLimits.LargestArrayBytes;
            sessionBinding.ReceiveTimeout = clientSettings.ReceiveTimeout;
            sessionBinding.SendTimeout = clientSettings.SendTimeout;

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
