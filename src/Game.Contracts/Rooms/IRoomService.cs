using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    [ServiceContract(CallbackContract = typeof(IRoomCallback))]
    public interface IRoomService
    {
        [OperationContract(Name = "CreateRoom")]
        Task<CreateRoomResponseContract> CreateRoomAsync();

        [OperationContract(Name = "GetRooms")]
        Task<RoomListContract> GetRoomsAsync();

        [OperationContract(Name = "JoinRoom")]
        Task<JoinRoomResponseContract> JoinRoomAsync(int roomId);

        [OperationContract(Name = "LeaveRoom")]
        Task<LeaveRoomResult> LeaveRoomAsync();

        [OperationContract(Name = "GetCurrentRoom")]
        Task<RoomLookupContract> GetCurrentRoomAsync();

        [OperationContract(Name = "KickPlayer")]
        Task<KickPlayerResult> KickPlayerAsync(int targetPlayerId);
    }
}
