namespace Game.Domain
{
    public enum TurnActionOutcome
    {
        Applied,
        WrongPhase,
        NotYourTurn,
        NotEnoughActionPoints,
        NoCaterpillarsLeft,
        NotAFlower,
        CellOccupied,
        NotYourCaterpillar,
        NotNeighborCell,
        ClimbTooHigh,
        FlowerAtMaximumLevel,
        NoBuildsLeft,
        NotNextToPlanter,
        JoinsPlanters,
    }
}
