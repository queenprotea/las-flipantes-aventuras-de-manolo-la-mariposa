namespace Game.Services
{
    internal readonly record struct RoomDeparture(bool WasMember, bool HasHostChanged, bool IsRoomEmpty);
}
