namespace Game.Services
{
    internal readonly record struct RoomActivityChange(bool HasChanged, bool HasHostChanged);
}
