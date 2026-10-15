namespace Game.Persistence
{
    public sealed class BuildAllowance
    {
        public short PlayerCount { get; set; }

        public short RoundNumber { get; set; }

        public short TurnNumber { get; set; }

        public short BuildingCount { get; set; }
    }
}
