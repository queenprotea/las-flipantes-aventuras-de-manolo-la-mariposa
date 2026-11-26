using Game.Contracts;

namespace Torres.Client.Connection
{
    internal sealed record RoomChange(RoomChangeReason Reason, Room Room);
}
