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
        private readonly ChatService _chatService;
        private readonly HistoryService _historyService;
        private readonly MatchService _matchService;

        public GameSessionService(SessionService sessionService, RoomService roomService, FriendsService friendsService, ChatService chatService, HistoryService historyService, MatchService matchService)
        {
            ArgumentNullException.ThrowIfNull(sessionService);
            ArgumentNullException.ThrowIfNull(roomService);
            ArgumentNullException.ThrowIfNull(friendsService);
            ArgumentNullException.ThrowIfNull(chatService);
            ArgumentNullException.ThrowIfNull(historyService);
            ArgumentNullException.ThrowIfNull(matchService);

            _sessionService = sessionService;
            _roomService = roomService;
            _friendsService = friendsService;
            _chatService = chatService;
            _historyService = historyService;
            _matchService = matchService;
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

        public FriendsOverviewResponse GetFriendsOverview()
        {
            FriendsOverviewResponse overview = _friendsService.GetFriendsOverview();

            return overview;
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
            Task<StartMatchResult> startTask = _matchService.StartMatchAsync();

            return startTask;
        }

        /// <inheritdoc/>
        public PlacementResult PlaceCaterpillar(int row, int column)
        {
            PlacementResult placementResult = _matchService.PlaceCaterpillar(row, column);

            return placementResult;
        }

        /// <inheritdoc/>
        public PlacementResult PlaceButterfly(int row, int column)
        {
            PlacementResult placementResult = _matchService.PlaceButterfly(row, column);

            return placementResult;
        }

        /// <inheritdoc/>
        public EndMatchResult EndMatch()
        {
            EndMatchResult endResult = _matchService.EndMatch();

            return endResult;
        }
    }
}
