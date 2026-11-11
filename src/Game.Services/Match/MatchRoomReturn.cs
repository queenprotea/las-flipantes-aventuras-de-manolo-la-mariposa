using Game.Contracts;

namespace Game.Services
{
    internal sealed class MatchRoomReturn
    {
        private readonly RoomRegistry _roomRegistry;
        private readonly RoomService _roomService;

        internal MatchRoomReturn(RoomRegistry roomRegistry, RoomService roomService)
        {
            _roomRegistry = roomRegistry;
            _roomService = roomService;
        }

        internal LiveRoom? ReturnRoomOf(int ownerId, RoomChangeReason returnReason)
        {
            LiveRoom? returnedRoom = _roomRegistry.FindRoomOf(ownerId);
            if ((returnedRoom is not null) && returnedRoom.EndMatch())
            {
                int? noExcludedOwnerId = null;
                _roomService.NotifyMembers(returnedRoom, returnReason, noExcludedOwnerId);
            }
            else
            {
                returnedRoom = null;
            }

            return returnedRoom;
        }
    }
}
