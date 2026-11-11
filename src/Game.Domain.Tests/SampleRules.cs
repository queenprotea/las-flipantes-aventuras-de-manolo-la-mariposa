namespace Game.Domain.Tests
{
    internal static class SampleRules
    {
        internal const int PlacementCost = 2;
        internal const int MovementCost = 1;
        internal const int ClimbCost = 1;
        internal const int BuildingCost = 1;
        internal const int ThreeBuildings = 3;
        internal const int TwoBuildings = 2;

        internal static ActionCosts Costs { get; } = new ActionCosts(PlacementCost, MovementCost, ClimbCost, BuildingCost);

        internal static RoundLayout FourTurnRound { get; } = new RoundLayout(new int[] { ThreeBuildings, ThreeBuildings, ThreeBuildings, ThreeBuildings });

        internal static RoundLayout OneTurnRound { get; } = new RoundLayout(new int[] { ThreeBuildings });

        internal static MatchRules TwoSeatRules { get; } = new MatchRules(new RoundLayout[] { FourTurnRound, FourTurnRound, FourTurnRound }, Costs);

        internal static MatchRules ThreeSeatRules { get; } = new MatchRules(new RoundLayout[] { new RoundLayout(new int[] { ThreeBuildings, ThreeBuildings, TwoBuildings, TwoBuildings }), new RoundLayout(new int[] { ThreeBuildings, ThreeBuildings, TwoBuildings }), new RoundLayout(new int[] { ThreeBuildings, ThreeBuildings, TwoBuildings }) }, Costs);

        internal static MatchRules OneTurnPerRoundRules { get; } = new MatchRules(new RoundLayout[] { OneTurnRound, OneTurnRound, OneTurnRound }, Costs);
    }
}
