using System;
using System.Collections.Generic;

using Game.Contracts;

namespace Game.Match.Tests
{
    internal static class SampleMatches
    {
        internal const int TieBreakSeed = 7;
        internal const int CaterpillarPlacementCost = 2;

        internal static TimeSpan PlacementTime { get; } = TimeSpan.FromSeconds(90);

        internal static RoundRule FourTurnRound { get; } = new RoundRule(new int[] { 3, 3, 3, 3 });

        internal static RoundRule OneTurnRound { get; } = new RoundRule(new int[] { 3 });

        internal static MatchRuleSet OneTurnPerRoundRules { get; } = new MatchRuleSet(new RoundRule[] { OneTurnRound, OneTurnRound, OneTurnRound }, new ActionPointCosts { CaterpillarPlacement = CaterpillarPlacementCost, Movement = 1, LevelClimb = 1, FlowerLevelAddition = 1 });

        internal static MatchRuleSet Rules { get; } = new MatchRuleSet(new RoundRule[] { FourTurnRound, FourTurnRound, FourTurnRound }, new ActionPointCosts { CaterpillarPlacement = CaterpillarPlacementCost, Movement = 1, LevelClimb = 1, FlowerLevelAddition = 1 });

        internal static TimeSpan FirstPlacementTaken { get; } = TimeSpan.FromSeconds(60);

        internal static TimeSpan RestOfFirstPlacement { get; } = PlacementTime - FirstPlacementTaken;

        internal static TimeSpan JustBeforeDeadline { get; } = PlacementTime - TimeSpan.FromTicks(1);

        internal static MatchCell NorthWestFlower { get; } = new MatchCell { Row = 1, Column = 1 };

        internal static MatchCell NorthEastFlower { get; } = new MatchCell { Row = 1, Column = 6 };

        internal static MatchCell CenterFlower { get; } = new MatchCell { Row = 3, Column = 3 };

        internal static MatchCell SouthWestFlower { get; } = new MatchCell { Row = 6, Column = 1 };

        internal static MatchCell SouthEastFlower { get; } = new MatchCell { Row = 6, Column = 6 };

        internal static MatchCell SouthOfNorthWest { get; } = new MatchCell { Row = 2, Column = 1 };

        internal static MatchCell SouthOfNorthEast { get; } = new MatchCell { Row = 2, Column = 6 };

        internal static MatchCell GroundCell { get; } = new MatchCell { Row = 4, Column = 4 };

        internal static MatchCell OutsideCell { get; } = new MatchCell { Row = 8, Column = 0 };

        internal static IReadOnlyList<MatchCell> FiveFlowerCells { get; } = new MatchCell[] { NorthWestFlower, NorthEastFlower, CenterFlower, SouthWestFlower, SouthEastFlower };

        internal static MatchEntrant Ana { get; } = new MatchEntrant(10, 1, "Ana", false, true, SeatColor.Green);

        internal static MatchEntrant Beto { get; } = new MatchEntrant(20, 2, "Beto", false, true, SeatColor.Blue);

        internal static MatchEntrant CaroGuest { get; } = new MatchEntrant(-1, 3, "Caro", true, true, SeatColor.Red);

        internal static MatchEntrant Dani { get; } = new MatchEntrant(40, 4, "Dani", false, true, SeatColor.Yellow);

        internal static MatchEntrant InactiveEva { get; } = new MatchEntrant(50, 5, "Eva", false, false, SeatColor.Red);

        internal static IReadOnlyList<MatchEntrant> AnaAndBeto { get; } = new MatchEntrant[] { Ana, Beto };

        internal static IReadOnlyList<MatchEntrant> AnaBetoAndDani { get; } = new MatchEntrant[] { Ana, Beto, Dani };

        internal static IReadOnlyList<MatchEntrant> CaroAndDani { get; } = new MatchEntrant[] { CaroGuest, Dani };
    }
}
