using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Services
{
    public sealed class RoomRegistry
    {
        private readonly object _registryLock = new object();
        private readonly Dictionary<int, LiveRoom> _roomsById = new Dictionary<int, LiveRoom>();
        private readonly Dictionary<string, LiveRoom> _roomsByCode = new Dictionary<string, LiveRoom>(StringComparer.Ordinal);
        private readonly Dictionary<int, int> _roomIdsByPlayerId = new Dictionary<int, int>();
        private readonly HashSet<int> _reservedPlayerIds = new HashSet<int>();

        internal bool Reserve(int playerId)
        {
            bool wasReserved = false;
            lock (_registryLock)
            {
                if (!_roomIdsByPlayerId.ContainsKey(playerId))
                {
                    wasReserved = _reservedPlayerIds.Add(playerId);
                }
            }

            return wasReserved;
        }

        internal void Release(int playerId)
        {
            lock (_registryLock)
            {
                _reservedPlayerIds.Remove(playerId);
            }
        }

        internal void AddRoom(LiveRoom room, int hostPlayerId)
        {
            lock (_registryLock)
            {
                _roomsById.Add(room.RoomId, room);
                _roomsByCode.Add(room.Code, room);
                _reservedPlayerIds.Remove(hostPlayerId);
                _roomIdsByPlayerId[hostPlayerId] = room.RoomId;
            }
        }

        internal void RecordMembership(int playerId, LiveRoom room)
        {
            lock (_registryLock)
            {
                _reservedPlayerIds.Remove(playerId);
                _roomIdsByPlayerId[playerId] = room.RoomId;
            }
        }

        internal void ForgetMembership(int playerId)
        {
            lock (_registryLock)
            {
                _roomIdsByPlayerId.Remove(playerId);
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

        internal LiveRoom? FindRoomOf(int playerId)
        {
            LiveRoom? room = null;
            lock (_registryLock)
            {
                if (_roomIdsByPlayerId.TryGetValue(playerId, out int roomId))
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
