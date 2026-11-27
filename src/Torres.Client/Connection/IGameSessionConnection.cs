using System;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Avatars;

namespace Torres.Client.Connection
{
    internal interface IGameSessionConnection
    {
        TimeSpan ReconnectionRetryInterval { get; }

        bool IsSessionOpen { get; }

        Task<OpenSessionResult> OpenAsync(string sessionToken);

        Task<OpenSessionResult> ReopenAsync();

        void MarkSessionOpen();

        Task<CreateRoomResult> CreateRoomAsync(RoomVisibility roomVisibility);

        Task<RoomListResponse> GetRoomsAsync();

        Task<RoomListResponse> FindRoomAsync(string roomCode);

        Task<JoinRoomResult> JoinRoomAsync(int roomId);

        Task<LeaveRoomResult> LeaveRoomAsync();

        Task<KickMemberResult> KickMemberAsync(int targetMemberId);

        Task<SendChatMessageResult> SendChatMessageAsync(string messageText);

        Task<ChatHistoryResponse> GetChatHistoryAsync();

        Task<ReportChatMessageResult> ReportChatMessageAsync(ChatMessage reportedMessage);

        Task<MatchHistoryResponse> GetMatchHistoryAsync(int pageNumber);

        Task<MatchDetailResponse> GetMatchDetailAsync(int matchId);

        Task<FriendshipListResponse> GetFriendshipListAsync();

        Task<PlayerSearchResponse> FindPlayerByUsernameAsync(string username);

        Task<FriendRequestResult> SendFriendRequestAsync(int targetPlayerId);

        Task<FriendRequestResponseResult> AcceptFriendRequestAsync(int requesterPlayerId);

        Task<FriendRequestResponseResult> RejectFriendRequestAsync(int requesterPlayerId);

        Task<FriendRemovalResult> RemoveFriendAsync(int friendPlayerId);

        Task<PendingInvitationsResponse> GetPendingInvitationsAsync();

        Task<RoomInviteesResponse> GetRoomInviteesAsync();

        Task<SendInvitationResult> SendInvitationAsync(int targetPlayerId);

        Task<SendInvitationResult> SendInvitationByEmailAsync(int targetPlayerId);

        Task<RespondInvitationResult> AcceptInvitationAsync(int roomId);

        Task<RespondInvitationResult> RejectInvitationAsync(int roomId);

        Task<StartMatchResult> StartMatchAsync();

        Task<ForfeitResult> ForfeitAsync();

        Task<ReturnToMatchResult> ReturnToMatchAsync();

        Task<MatchActionResult> PlacePieceAsync(PiecePlacement piecePlacement);

        Task<MatchActionResult> MoveCaterpillarAsync(MatchMove move);

        Task<MatchActionResult> AddFlowerLevelAsync(MatchCell buildCell);

        Task<MatchActionResult> DrawCardAsync();

        Task<MatchActionResult> PlayCardAsync(MatchCardPlay cardPlay);

        Task<MatchActionResult> EndTurnAsync();

        Task<ProfileResponse> GetProfileAsync();

        Task<BeginAvatarUploadResult> BeginAvatarUploadAsync(PreparedAvatar preparedAvatar);

        Task<SendAvatarPartResult> SendAvatarPartAsync(AvatarPart avatarPart);

        void RequestAvatarUploadCancellation();

        Task<UpdateProfileResult> UpdateProfileAsync(string username, bool includesNewAvatar);

        void CloseSession();

        void AbandonSession();

        void AbortSessionChannel();
    }
}
