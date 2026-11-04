using Game.Contracts;

namespace Torres.Client.Scene
{
    internal readonly record struct BoardCaterpillar(SeatColor Color, BoardCell Cell, float HeadingDegrees);
}
