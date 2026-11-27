using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Avatars;
using Torres.Client.Connection;

namespace Torres.Client.Tests
{
    internal sealed class RecordingGameSessionConnection : IGameSessionConnection
    {
        private static readonly TimeSpan _oneSecond = TimeSpan.FromSeconds(1);

        public TimeSpan ReconnectionRetryInterval { get; set; } = _oneSecond;

        public bool IsSessionOpen { get; set; } = true;

        internal Task<OpenSessionResult> OpenReply { get; set; } = Task.FromResult(new OpenSessionResult());

        internal Task<OpenSessionResult> ReopenReply { get; set; } = Task.FromResult(new OpenSessionResult());

        internal Task<CreateRoomResult> CreateRoomReply { get; set; } = Task.FromResult(new CreateRoomResult());

        internal Task<RoomListResponse> GetRoomsReply { get; set; } = Task.FromResult(new RoomListResponse { Rooms = new List<RoomSummary>() });

        internal Task<RoomListResponse> FindRoomReply { get; set; } = Task.FromResult(new RoomListResponse { Rooms = new List<RoomSummary>() });

        internal Task<JoinRoomResult> JoinRoomReply { get; set; } = Task.FromResult(new JoinRoomResult());

        internal Task<LeaveRoomResult> LeaveRoomReply { get; set; } = Task.FromResult(default(LeaveRoomResult));

        internal Task<KickMemberResult> KickMemberReply { get; set; } = Task.FromResult(default(KickMemberResult));

        internal Task<SendChatMessageResult> SendChatMessageReply { get; set; } = Task.FromResult(default(SendChatMessageResult));

        internal Task<ChatHistoryResponse> GetChatHistoryReply { get; set; } = Task.FromResult(new ChatHistoryResponse());

        internal Task<ReportChatMessageResult> ReportChatMessageReply { get; set; } = Task.FromResult(default(ReportChatMessageResult));

        internal Task<MatchHistoryResponse> GetMatchHistoryReply { get; set; } = Task.FromResult(new MatchHistoryResponse());

        internal Task<MatchDetailResponse> GetMatchDetailReply { get; set; } = Task.FromResult(new MatchDetailResponse());

        internal Task<FriendshipListResponse> GetFriendshipListReply { get; set; } = Task.FromResult(new FriendshipListResponse());

        internal Task<PlayerSearchResponse> FindPlayerByUsernameReply { get; set; } = Task.FromResult(new PlayerSearchResponse());

        internal Task<FriendRequestResult> SendFriendRequestReply { get; set; } = Task.FromResult(default(FriendRequestResult));

        internal Task<FriendRequestResponseResult> AcceptFriendRequestReply { get; set; } = Task.FromResult(default(FriendRequestResponseResult));

        internal Task<FriendRequestResponseResult> RejectFriendRequestReply { get; set; } = Task.FromResult(default(FriendRequestResponseResult));

        internal Task<FriendRemovalResult> RemoveFriendReply { get; set; } = Task.FromResult(default(FriendRemovalResult));

        internal Task<PendingInvitationsResponse> GetPendingInvitationsReply { get; set; } = Task.FromResult(new PendingInvitationsResponse());

        internal Task<RoomInviteesResponse> GetRoomInviteesReply { get; set; } = Task.FromResult(new RoomInviteesResponse());

        internal Task<SendInvitationResult> SendInvitationReply { get; set; } = Task.FromResult(default(SendInvitationResult));

        internal Task<SendInvitationResult> SendInvitationByEmailReply { get; set; } = Task.FromResult(default(SendInvitationResult));

        internal Task<RespondInvitationResult> AcceptInvitationReply { get; set; } = Task.FromResult(new RespondInvitationResult());

        internal Task<RespondInvitationResult> RejectInvitationReply { get; set; } = Task.FromResult(new RespondInvitationResult());

        internal Task<StartMatchResult> StartMatchReply { get; set; } = Task.FromResult(new StartMatchResult());

        internal Task<ForfeitResult> ForfeitReply { get; set; } = Task.FromResult(default(ForfeitResult));

        internal Task<ReturnToMatchResult> ReturnToMatchReply { get; set; } = Task.FromResult(default(ReturnToMatchResult));

        internal Task<MatchActionResult> PlacePieceReply { get; set; } = Task.FromResult(default(MatchActionResult));

        internal Task<MatchActionResult> MoveCaterpillarReply { get; set; } = Task.FromResult(default(MatchActionResult));

        internal Task<MatchActionResult> AddFlowerLevelReply { get; set; } = Task.FromResult(default(MatchActionResult));

        internal Task<MatchActionResult> DrawCardReply { get; set; } = Task.FromResult(default(MatchActionResult));

        internal Task<MatchActionResult> PlayCardReply { get; set; } = Task.FromResult(default(MatchActionResult));

        internal Task<MatchActionResult> EndTurnReply { get; set; } = Task.FromResult(default(MatchActionResult));

        internal Task<ProfileResponse> GetProfileReply { get; set; } = Task.FromResult(new ProfileResponse());

        internal Task<BeginAvatarUploadResult> BeginAvatarUploadReply { get; set; } = Task.FromResult(default(BeginAvatarUploadResult));

        internal Task<SendAvatarPartResult> SendAvatarPartReply { get; set; } = Task.FromResult(default(SendAvatarPartResult));

        internal Task<UpdateProfileResult> UpdateProfileReply { get; set; } = Task.FromResult(new UpdateProfileResult());

        internal List<string> CalledMethods { get; } = new List<string>();

        public Task<OpenSessionResult> OpenAsync(string sessionToken)
        {
            CalledMethods.Add(nameof(OpenAsync));

            return OpenReply;
        }

        public Task<OpenSessionResult> ReopenAsync()
        {
            CalledMethods.Add(nameof(ReopenAsync));

            return ReopenReply;
        }

        public void MarkSessionOpen()
        {
            CalledMethods.Add(nameof(MarkSessionOpen));
        }

        public Task<CreateRoomResult> CreateRoomAsync(RoomVisibility roomVisibility)
        {
            CalledMethods.Add(nameof(CreateRoomAsync));

            return CreateRoomReply;
        }

        public Task<RoomListResponse> GetRoomsAsync()
        {
            CalledMethods.Add(nameof(GetRoomsAsync));

            return GetRoomsReply;
        }

        public Task<RoomListResponse> FindRoomAsync(string roomCode)
        {
            CalledMethods.Add(nameof(FindRoomAsync));

            return FindRoomReply;
        }

        public Task<JoinRoomResult> JoinRoomAsync(int roomId)
        {
            CalledMethods.Add(nameof(JoinRoomAsync));

            return JoinRoomReply;
        }

        public Task<LeaveRoomResult> LeaveRoomAsync()
        {
            CalledMethods.Add(nameof(LeaveRoomAsync));

            return LeaveRoomReply;
        }

        public Task<KickMemberResult> KickMemberAsync(int targetMemberId)
        {
            CalledMethods.Add(nameof(KickMemberAsync));

            return KickMemberReply;
        }

        public Task<SendChatMessageResult> SendChatMessageAsync(string messageText)
        {
            CalledMethods.Add(nameof(SendChatMessageAsync));

            return SendChatMessageReply;
        }

        public Task<ChatHistoryResponse> GetChatHistoryAsync()
        {
            CalledMethods.Add(nameof(GetChatHistoryAsync));

            return GetChatHistoryReply;
        }

        public Task<ReportChatMessageResult> ReportChatMessageAsync(ChatMessage reportedMessage)
        {
            CalledMethods.Add(nameof(ReportChatMessageAsync));

            return ReportChatMessageReply;
        }

        public Task<MatchHistoryResponse> GetMatchHistoryAsync(int pageNumber)
        {
            CalledMethods.Add(nameof(GetMatchHistoryAsync));

            return GetMatchHistoryReply;
        }

        public Task<MatchDetailResponse> GetMatchDetailAsync(int matchId)
        {
            CalledMethods.Add(nameof(GetMatchDetailAsync));

            return GetMatchDetailReply;
        }

        public Task<FriendshipListResponse> GetFriendshipListAsync()
        {
            CalledMethods.Add(nameof(GetFriendshipListAsync));

            return GetFriendshipListReply;
        }

        public Task<PlayerSearchResponse> FindPlayerByUsernameAsync(string username)
        {
            CalledMethods.Add(nameof(FindPlayerByUsernameAsync));

            return FindPlayerByUsernameReply;
        }

        public Task<FriendRequestResult> SendFriendRequestAsync(int targetPlayerId)
        {
            CalledMethods.Add(nameof(SendFriendRequestAsync));

            return SendFriendRequestReply;
        }

        public Task<FriendRequestResponseResult> AcceptFriendRequestAsync(int requesterPlayerId)
        {
            CalledMethods.Add(nameof(AcceptFriendRequestAsync));

            return AcceptFriendRequestReply;
        }

        public Task<FriendRequestResponseResult> RejectFriendRequestAsync(int requesterPlayerId)
        {
            CalledMethods.Add(nameof(RejectFriendRequestAsync));

            return RejectFriendRequestReply;
        }

        public Task<FriendRemovalResult> RemoveFriendAsync(int friendPlayerId)
        {
            CalledMethods.Add(nameof(RemoveFriendAsync));

            return RemoveFriendReply;
        }

        public Task<PendingInvitationsResponse> GetPendingInvitationsAsync()
        {
            CalledMethods.Add(nameof(GetPendingInvitationsAsync));

            return GetPendingInvitationsReply;
        }

        public Task<RoomInviteesResponse> GetRoomInviteesAsync()
        {
            CalledMethods.Add(nameof(GetRoomInviteesAsync));

            return GetRoomInviteesReply;
        }

        public Task<SendInvitationResult> SendInvitationAsync(int targetPlayerId)
        {
            CalledMethods.Add(nameof(SendInvitationAsync));

            return SendInvitationReply;
        }

        public Task<SendInvitationResult> SendInvitationByEmailAsync(int targetPlayerId)
        {
            CalledMethods.Add(nameof(SendInvitationByEmailAsync));

            return SendInvitationByEmailReply;
        }

        public Task<RespondInvitationResult> AcceptInvitationAsync(int roomId)
        {
            CalledMethods.Add(nameof(AcceptInvitationAsync));

            return AcceptInvitationReply;
        }

        public Task<RespondInvitationResult> RejectInvitationAsync(int roomId)
        {
            CalledMethods.Add(nameof(RejectInvitationAsync));

            return RejectInvitationReply;
        }

        public Task<StartMatchResult> StartMatchAsync()
        {
            CalledMethods.Add(nameof(StartMatchAsync));

            return StartMatchReply;
        }

        public Task<ForfeitResult> ForfeitAsync()
        {
            CalledMethods.Add(nameof(ForfeitAsync));

            return ForfeitReply;
        }

        public Task<ReturnToMatchResult> ReturnToMatchAsync()
        {
            CalledMethods.Add(nameof(ReturnToMatchAsync));

            return ReturnToMatchReply;
        }

        public Task<MatchActionResult> PlacePieceAsync(PiecePlacement piecePlacement)
        {
            CalledMethods.Add(nameof(PlacePieceAsync));

            return PlacePieceReply;
        }

        public Task<MatchActionResult> MoveCaterpillarAsync(MatchMove move)
        {
            CalledMethods.Add(nameof(MoveCaterpillarAsync));

            return MoveCaterpillarReply;
        }

        public Task<MatchActionResult> AddFlowerLevelAsync(MatchCell buildCell)
        {
            CalledMethods.Add(nameof(AddFlowerLevelAsync));

            return AddFlowerLevelReply;
        }

        public Task<MatchActionResult> DrawCardAsync()
        {
            CalledMethods.Add(nameof(DrawCardAsync));

            return DrawCardReply;
        }

        public Task<MatchActionResult> PlayCardAsync(MatchCardPlay cardPlay)
        {
            CalledMethods.Add(nameof(PlayCardAsync));

            return PlayCardReply;
        }

        public Task<MatchActionResult> EndTurnAsync()
        {
            CalledMethods.Add(nameof(EndTurnAsync));

            return EndTurnReply;
        }

        public Task<ProfileResponse> GetProfileAsync()
        {
            CalledMethods.Add(nameof(GetProfileAsync));

            return GetProfileReply;
        }

        public Task<BeginAvatarUploadResult> BeginAvatarUploadAsync(PreparedAvatar preparedAvatar)
        {
            CalledMethods.Add(nameof(BeginAvatarUploadAsync));

            return BeginAvatarUploadReply;
        }

        public Task<SendAvatarPartResult> SendAvatarPartAsync(AvatarPart avatarPart)
        {
            CalledMethods.Add(nameof(SendAvatarPartAsync));

            return SendAvatarPartReply;
        }

        public void RequestAvatarUploadCancellation()
        {
            CalledMethods.Add(nameof(RequestAvatarUploadCancellation));
        }

        public Task<UpdateProfileResult> UpdateProfileAsync(string username, bool includesNewAvatar)
        {
            CalledMethods.Add(nameof(UpdateProfileAsync));

            return UpdateProfileReply;
        }

        public void CloseSession()
        {
            CalledMethods.Add(nameof(CloseSession));
        }

        public void AbandonSession()
        {
            CalledMethods.Add(nameof(AbandonSession));
        }

        public void AbortSessionChannel()
        {
            CalledMethods.Add(nameof(AbortSessionChannel));
        }
    }
}
