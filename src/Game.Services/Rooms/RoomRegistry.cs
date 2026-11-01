using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Game.Services
{
    public sealed class RoomRegistry
    {
        private readonly Lock _registryLock = new Lock();
        private readonly Dictionary<int, LiveRoom> _roomsById = new Dictionary<int, LiveRoom>();
        private readonly Dictionary<string, LiveRoom> _roomsByCode = new Dictionary<string, LiveRoom>(StringComparer.Ordinal);
        private readonly Dictionary<int, int> _roomIdsByOwnerId = new Dictionary<int, int>();
        private readonly HashSet<int> _reservedOwnerIds = new HashSet<int>();

        internal bool Reserve(int ownerId)
        {
            bool wasReserved = false;
            lock (_registryLock)
            {
                if (!_roomIdsByOwnerId.ContainsKey(ownerId))
                {
                    wasReserved = _reservedOwnerIds.Add(ownerId);
                }
            }

            return wasReserved;
        }

        internal void Release(int ownerId)
        {
            lock (_registryLock)
            {
                _reservedOwnerIds.Remove(ownerId);
            }
        }

        internal void AddRoom(LiveRoom room, int hostOwnerId)
        {
            lock (_registryLock)
            {
                _roomsById.Add(room.RoomId, room);
                _roomsByCode.Add(room.Code, room);
                _reservedOwnerIds.Remove(hostOwnerId);
                _roomIdsByOwnerId[hostOwnerId] = room.RoomId;
            }
        }

        internal void RecordMembership(int ownerId, LiveRoom room)
        {
            lock (_registryLock)
            {
                _reservedOwnerIds.Remove(ownerId);
                _roomIdsByOwnerId[ownerId] = room.RoomId;
            }
        }

        internal void ForgetMembership(int ownerId)
        {
            lock (_registryLock)
            {
                _roomIdsByOwnerId.Remove(ownerId);
            }
        }

        internal void RemoveRoom(LiveRoom room)
        {
            lock (_registryLock)
            {
                _roomsById.Remove(room.RoomId);
                _roomsByCode.Remove(room.Code);
            }
        }

        internal LiveRoom? FindById(int roomId)
        {
            LiveRoom? room;
            lock (_registryLock)
            {
                _roomsById.TryGetValue(roomId, out room);
            }

            return room;
        }

        internal LiveRoom? FindByCode(string roomCode)
        {
            LiveRoom? room;
            lock (_registryLock)
            {
                _roomsByCode.TryGetValue(roomCode, out room);
            }

            return room;
        }

        internal LiveRoom? FindRoomOf(int ownerId)
        {
            LiveRoom? room = null;
            lock (_registryLock)
            {
                if (_roomIdsByOwnerId.TryGetValue(ownerId, out int roomId))
                {
                    _roomsById.TryGetValue(roomId, out room);
                }
            }

            return room;
        }

        internal IReadOnlyList<LiveRoom> ListRooms()
        {
            List<LiveRoom> rooms;
            lock (_registryLock)
            {
                rooms = _roomsById.Values.OrderBy(room => room.RoomId).ToList();
            }

            return rooms;
        }
    }
}
