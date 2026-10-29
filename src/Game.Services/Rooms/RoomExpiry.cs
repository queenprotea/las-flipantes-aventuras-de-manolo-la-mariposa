using System.Collections.Generic;

namespace Game.Services
{
    internal sealed record RoomExpiry(IReadOnlyList<int> RemovedPlayerIds, bool HasHostChanged, bool IsRoomEmpty);
}
