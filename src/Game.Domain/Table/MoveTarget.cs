namespace Game.Domain
{
    public readonly record struct MoveTarget(BoardCell Destination, int ActionPointCost);
}
