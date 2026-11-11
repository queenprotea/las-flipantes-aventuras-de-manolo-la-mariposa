using System;
using System.Threading.Tasks;

using CoreWCF;

using Game.Contracts;

namespace Game.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public sealed class GameSessionService : IGameSessionService
    {
        private readonly SessionService _sessionService;
        private readonly RoomService _roomService;
        private readonly FriendsService _friendsService;
        private readonly InvitationService _invitationService;
        private readonly ChatService _chatService;
        private readonly HistoryService _historyService;
        private readonly MatchService _matchService;
        private readonly ProfileService _profileService;

        public GameSessionService(SessionService sessionService, RoomService roomService, FriendsService friendsService, InvitationService invitationService, ChatService chatService, HistoryService historyService, MatchService matchService, ProfileService profileService)
        {
            ArgumentNullException.ThrowIfNull(sessionService);
            ArgumentNullException.ThrowIfNull(roomService);
            ArgumentNullException.ThrowIfNull(friendsService);
            ArgumentNullException.ThrowIfNull(invitationService);
            ArgumentNullException.ThrowIfNull(chatService);
            ArgumentNullException.ThrowIfNull(historyService);
            ArgumentNullException.ThrowIfNull(matchService);
            ArgumentNullException.ThrowIfNull(profileService);

            _sessionService = sessionService;
            _roomService = roomService;
            _friendsService = friendsService;
            _invitationService = invitationService;
            _chatService = chatService;
            _historyService = historyService;
            _matchService = matchService;
            _profileService = profileService;
        }

        public OpenSessionResult OpenSession(string sessionToken)
        {
            OpenSessionResult opening = _sessionService.OpenSession(sessionToken);

            return opening;
        }

        public ConnectionCheckResponse IsAlive()
        {
            ConnectionCheckResponse connectionCheck = _sessionService.IsAlive();

            return connectionCheck;
        }

        public Task<CloseSessionResult> CloseSessionAsync()
        {
            Task<CloseSessionResult> closing = _sessionService.CloseSessionAsync();

            return closing;
        }

        public Task<CreateRoomResult> CreateRoomAsync(RoomVisibility visibility)
        {
            Task<CreateRoomResult> creation = _roomService.CreateRoomAsync(visibility);

            return creation;
        }

        public RoomListResponse GetRooms()
        {
            RoomListResponse roomList = _roomService.GetRooms();

            return roomList;
        }

        public RoomListResponse FindRoom(string roomCode)
        {
            RoomListResponse roomList = _roomService.FindRoom(roomCode);

            return roomList;
        }

        public Task<JoinRoomResult> JoinRoomAsync(int roomId)
        {
            Task<JoinRoomResult> joining = _roomService.JoinRoomAsync(roomId);

            return joining;
        }

        public Task<LeaveRoomResult> LeaveRoomAsync()
        {
            Task<LeaveRoomResult> leaving = _roomService.LeaveRoomAsync();

            return leaving;
        }

        public RoomLookupResponse GetCurrentRoom()
        {
            RoomLookupResponse lookup = _roomService.GetCurrentRoom();

            return lookup;
        }

        public KickMemberResult KickMember(int targetMemberId)
        {
            KickMemberResult kickResult = _roomService.KickMember(targetMemberId);

            return kickResult;
        }

        public FriendshipListResponse GetFriendshipList()
        {
            FriendshipListResponse friendshipList = _friendsService.GetFriendshipList();

            return friendshipList;
        }

        public PlayerSearchResponse FindPlayerByUsername(string username)
        {
            PlayerSearchResponse searchResult = _friendsService.FindPlayerByUsername(username);

            return searchResult;
        }

        public FriendRequestResult SendFriendRequest(int targetPlayerId)
        {
            FriendRequestResult requestResult = _friendsService.SendFriendRequest(targetPlayerId);

            return requestResult;
        }

        public FriendRequestResponseResult RespondFriendRequest(int requesterPlayerId, bool accept)
        {
            FriendRequestResponseResult responseResult = _friendsService.RespondFriendRequest(requesterPlayerId, accept);

            return responseResult;
        }

        public FriendRemovalResult RemoveFriend(int friendPlayerId)
        {
            FriendRemovalResult removalResult = _friendsService.RemoveFriend(friendPlayerId);

            return removalResult;
        }

        /// <inheritdoc/>
        public Task<PendingInvitationsResponse> GetPendingInvitationsAsync()
        {
            Task<PendingInvitationsResponse> invitationsReading = _invitationService.GetPendingInvitationsAsync();

            return invitationsReading;
        }

        /// <inheritdoc/>
        public Task<RoomInviteesResponse> GetRoomInviteesAsync()
        {
            Task<RoomInviteesResponse> inviteesReading = _invitationService.GetRoomInviteesAsync();

            return inviteesReading;
        }

        /// <inheritdoc/>
        public Task<SendInvitationResult> SendInvitationAsync(int targetPlayerId)
        {
            Task<SendInvitationResult> invitationSending = _invitationService.SendInvitationAsync(targetPlayerId);

            return invitationSending;
        }

        /// <inheritdoc/>
        public Task<RespondInvitationResult> RespondInvitationAsync(int roomId, bool accept)
        {
            Task<RespondInvitationResult> invitationAnswer = _invitationService.RespondInvitationAsync(roomId, accept);

            return invitationAnswer;
        }

        public SendChatMessageResult SendChatMessage(string text)
        {
            SendChatMessageResult sendResult = _chatService.SendChatMessage(text);

            return sendResult;
        }

        public ChatHistoryResponse GetChatHistory()
        {
            ChatHistoryResponse history = _chatService.GetChatHistory();

            return history;
        }

        public Task<MatchHistoryResponse> GetMatchHistoryAsync(int pageNumber)
        {
            Task<MatchHistoryResponse> historyReading = _historyService.GetMatchHistoryAsync(pageNumber);

            return historyReading;
        }

        public Task<MatchDetailResponse> GetMatchDetailAsync(int matchId)
        {
            Task<MatchDetailResponse> detailReading = _historyService.GetMatchDetailAsync(matchId);

            return detailReading;
        }

        /// <inheritdoc/>
        public Task<StartMatchResult> StartMatchAsync()
        {
            Task<StartMatchResult> matchStart = _matchService.StartMatchAsync();

            return matchStart;
        }

        /// <inheritdoc/>
        public MatchActionResult PlacePiece(MatchPiece piece, MatchCell destinationCell)
        {
            MatchActionResult actionResult = _matchService.PlacePiece(piece, destinationCell);

            return actionResult;
        }

        /// <inheritdoc/>
        public MatchActionResult MoveCaterpillar(MatchMove move)
        {
            MatchActionResult actionResult = _matchService.MoveCaterpillar(move);

            return actionResult;
        }

        /// <inheritdoc/>
        public MatchActionResult AddFlowerLevel(MatchCell buildCell)
        {
            MatchActionResult actionResult = _matchService.AddFlowerLevel(buildCell);

            return actionResult;
        }

        /// <inheritdoc/>
        public MatchActionResult EndTurn()
        {
            MatchActionResult actionResult = _matchService.EndTurn();

            return actionResult;
        }

        /// <inheritdoc/>
        public EndMatchResult EndMatch()
        {
            EndMatchResult endResult = _matchService.EndMatch();

            return endResult;
        }

        /// <inheritdoc/>
        public Task<ProfileResponse> GetProfileAsync()
        {
            Task<ProfileResponse> profileReading = _profileService.GetProfileAsync();

            return profileReading;
        }

        /// <inheritdoc/>
        public BeginAvatarUploadResult BeginAvatarUpload(int originalBytes, int thumbnailBytes)
        {
            BeginAvatarUploadResult beginResult = _profileService.BeginAvatarUpload(originalBytes, thumbnailBytes);

            return beginResult;
        }

        /// <inheritdoc/>
        public SendAvatarPartResult SendAvatarPart(AvatarFileKind fileKind, int offset, byte[] part)
        {
            SendAvatarPartResult partResult = _profileService.SendAvatarPart(fileKind, offset, part);

            return partResult;
        }

        /// <inheritdoc/>
        public void CancelAvatarUpload()
        {
            _profileService.CancelAvatarUpload();
        }

        /// <inheritdoc/>
        public Task<UpdateProfileResult> UpdateProfileAsync(string username, bool includesNewAvatar)
        {
            Task<UpdateProfileResult> profileUpdate = _profileService.UpdateProfileAsync(username, includesNewAvatar);

            return profileUpdate;
        }

        /// <inheritdoc/>
        public Task<AvatarResponse> GetAvatarThumbnailAsync(string avatarReference)
        {
            Task<AvatarResponse> thumbnailReading = _profileService.GetAvatarThumbnailAsync(avatarReference);

            return thumbnailReading;
        }
    }
}
