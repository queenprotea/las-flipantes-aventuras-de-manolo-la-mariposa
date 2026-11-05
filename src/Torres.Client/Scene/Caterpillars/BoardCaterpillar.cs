using Game.Contracts;

namespace Torres.Client.Scene
{
    internal readonly record struct BoardCaterpillar(SeatColor SeatColor, BoardCell Cell, float HeadingDegrees);
}
