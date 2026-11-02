using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Lets players, with an account or as guests, create rooms, find them, enter and leave them, and lets hosts remove members.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IRoomNoticeReceiver))]
    public interface IRoomService
    {
        /// <summary>
        /// Creates a room with the caller as its host and only member.
        /// </summary>
        /// <param name="visibility">Whether the room appears in the public list or is entered only with its code.</param>
        /// <returns>The new room, or why it could not be created, with the end of the ban when the caller is banned.</returns>
        [OperationContract(Name = "CreateRoom", IsInitiating = false)]
        Task<CreateRoomResult> CreateRoomAsync(RoomVisibility visibility);

        /// <summary>
        /// Lists the open public rooms.
        /// </summary>
        /// <returns>The open public rooms.</returns>
        [OperationContract(Name = "GetRooms", IsInitiating = false)]
        RoomListResponse GetRooms();

        /// <summary>
        /// Looks for the open room with a code, public or private, without telling upper and lower case apart.
        /// </summary>
        /// <param name="roomCode">Code of the room, four characters long.</param>
        /// <returns>The room found, or an empty list when no open room has that code.</returns>
        [OperationContract(Name = "FindRoom", IsInitiating = false)]
        RoomListResponse FindRoom(string roomCode);

        /// <summary>
        /// Enters the caller into a room.
        /// </summary>
        /// <param name="roomId">Identifier of the room, as the room list gives it.</param>
        /// <returns>The room entered, or why the caller could not enter, with the end of the ban when the caller is banned.</returns>
        [OperationContract(Name = "JoinRoom", IsInitiating = false)]
        Task<JoinRoomResult> JoinRoomAsync(int roomId);

        /// <summary>
        /// Takes the caller out of their room; the host role passes on, and the room closes when nobody is left.
        /// </summary>
        /// <returns>Whether the caller left a room.</returns>
        [OperationContract(Name = "LeaveRoom", IsInitiating = false)]
        Task<LeaveRoomResult> LeaveRoomAsync();

        /// <summary>
        /// Reads the room the caller is in.
        /// </summary>
        /// <returns>The caller's room, or that the caller is in none.</returns>
        [OperationContract(Name = "GetCurrentRoom", IsInitiating = false)]
        RoomLookupResponse GetCurrentRoom();

        /// <summary>
        /// Removes another member from the room the caller hosts.
        /// </summary>
        /// <param name="targetMemberId">Member identifier of the player to remove.</param>
        /// <returns>Whether the member was removed, or why not.</returns>
        [OperationContract(Name = "KickMember", IsInitiating = false)]
        KickMemberResult KickMember(int targetMemberId);
    }
}
