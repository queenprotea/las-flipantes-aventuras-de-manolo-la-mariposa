using System;

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

        private const int TieBreakSeed = 7;

        internal static ActionCosts Costs { get; } = new ActionCosts(PlacementCost, MovementCost, ClimbCost, BuildingCost);

        internal static RoundTurnPlan FourTurnRound { get; } = new RoundTurnPlan(new int[] { ThreeBuildings, ThreeBuildings, ThreeBuildings, ThreeBuildings });

        internal static RoundTurnPlan OneTurnRound { get; } = new RoundTurnPlan(new int[] { ThreeBuildings });

        internal static MatchRules TwoSeatRules { get; } = new MatchRules(new RoundTurnPlan[] { FourTurnRound, FourTurnRound, FourTurnRound }, Costs);

        internal static MatchRules ThreeSeatRules { get; } = new MatchRules(new RoundTurnPlan[] { new RoundTurnPlan(new int[] { ThreeBuildings, ThreeBuildings, TwoBuildings, TwoBuildings }), new RoundTurnPlan(new int[] { ThreeBuildings, ThreeBuildings, TwoBuildings }), new RoundTurnPlan(new int[] { ThreeBuildings, ThreeBuildings, TwoBuildings }) }, Costs);

        internal static MatchRules OneTurnPerRoundRules { get; } = new MatchRules(new RoundTurnPlan[] { OneTurnRound, OneTurnRound, OneTurnRound }, Costs);

        internal static Random CreateTieBreakRandom()
        {
            var tieBreakRandom = new Random(TieBreakSeed);

            return tieBreakRandom;
        }
    }
}
