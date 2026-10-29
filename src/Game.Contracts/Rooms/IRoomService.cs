using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    [ServiceContract(CallbackContract = typeof(IRoomNoticeReceiver))]
    public interface IRoomService
    {
        [OperationContract(Name = "CreateRoom", IsInitiating = false)]
        Task<CreateRoomResult> CreateRoomAsync(RoomVisibility visibility);

        [OperationContract(Name = "GetRooms", IsInitiating = false)]
        RoomListResponse GetRooms();

        [OperationContract(Name = "FindRoom", IsInitiating = false)]
        RoomListResponse FindRoom(string roomCode);

        [OperationContract(Name = "JoinRoom", IsInitiating = false)]
        Task<JoinRoomResult> JoinRoomAsync(int roomId);

        [OperationContract(Name = "LeaveRoom", IsInitiating = false)]
        Task<LeaveRoomResult> LeaveRoomAsync();

        [OperationContract(Name = "GetCurrentRoom", IsInitiating = false)]
        RoomLookupResponse GetCurrentRoom();

        [OperationContract(Name = "KickMember", IsInitiating = false)]
        KickMemberResult KickMember(int targetMemberId);
    }
}
