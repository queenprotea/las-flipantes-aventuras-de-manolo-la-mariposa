using System.Collections.Generic;

namespace Game.Services
{
    internal sealed record RoomExpiry(IReadOnlyList<int> RemovedOwnerIds, bool HasHostChanged, bool IsRoomEmpty);
}
