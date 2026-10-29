using Game.Contracts;

namespace Torres.Client.Session
{
    internal sealed record RoomChange(RoomChangeReason Reason, Room Room);
}
