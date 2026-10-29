using Game.Contracts;

namespace Game.Services
{
    internal sealed record RoomKick(KickMemberResult Result, int? KickedPlayerId, Room? KickedView);
}
