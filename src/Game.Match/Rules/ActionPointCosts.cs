namespace Game.Match
{
    public sealed record ActionPointCosts
    {
        public int CaterpillarPlacement { get; init; }

        public int Movement { get; init; }

        public int LevelClimb { get; init; }

        public int FlowerLevelAddition { get; init; }
    }
}
