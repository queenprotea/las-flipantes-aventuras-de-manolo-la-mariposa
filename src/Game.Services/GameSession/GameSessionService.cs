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

        public GameSessionService(SessionService sessionService, RoomService roomService, FriendsService friendsService)
        {
            ArgumentNullException.ThrowIfNull(sessionService);
            ArgumentNullException.ThrowIfNull(roomService);
            ArgumentNullException.ThrowIfNull(friendsService);

            _sessionService = sessionService;
            _roomService = roomService;
            _friendsService = friendsService;
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
    }
}
