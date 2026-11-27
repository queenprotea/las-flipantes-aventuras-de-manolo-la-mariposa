using System;
using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

namespace Game.Match.Tests
{
    internal static class SampleMatches
    {
        internal const int ChanceSeed = 7;
        internal const int CaterpillarPlacementCost = 2;
        internal const int RoomId = 30;
        internal const string RoomCode = "K7Q2PX";

        internal static TimeSpan StepTime { get; } = TimeSpan.FromSeconds(90);

        internal static TimeSpan ReconnectionTime { get; } = TimeSpan.FromSeconds(40);

        internal static MatchTimeLimits TimeLimits { get; } = new MatchTimeLimits(StepTime, ReconnectionTime);

        internal static RoundRule FourTurnRound { get; } = new RoundRule(new int[] { 3, 3, 3, 3 });

        internal static RoundRule OneTurnRound { get; } = new RoundRule(new int[] { 3 });

        internal static MatchRuleSet OneTurnPerRoundRules { get; } = new MatchRuleSet(new RoundRule[] { OneTurnRound, OneTurnRound, OneTurnRound }, new ActionPointCosts(CaterpillarPlacementCost, 1, 1, 1, 1, 0));

        internal static MatchRuleSet Rules { get; } = new MatchRuleSet(new RoundRule[] { FourTurnRound, FourTurnRound, FourTurnRound }, new ActionPointCosts(CaterpillarPlacementCost, 1, 1, 1, 1, 0));

        internal static TimeSpan FirstPlacementTaken { get; } = TimeSpan.FromSeconds(60);

        internal static TimeSpan RestOfFirstPlacement { get; } = StepTime - FirstPlacementTaken;

        internal static TimeSpan JustBeforeDeadline { get; } = StepTime - TimeSpan.FromTicks(1);

        internal static MatchCell NorthWestFlower { get; } = new MatchCell { Row = 1, Column = 1 };

        internal static MatchCell NorthEastFlower { get; } = new MatchCell { Row = 1, Column = 6 };

        internal static MatchCell CenterFlower { get; } = new MatchCell { Row = 3, Column = 3 };

        internal static MatchCell SouthWestFlower { get; } = new MatchCell { Row = 6, Column = 1 };

        internal static MatchCell SouthEastFlower { get; } = new MatchCell { Row = 6, Column = 6 };

        internal static MatchCell SouthOfNorthWest { get; } = new MatchCell { Row = 2, Column = 1 };

        internal static MatchCell SouthOfNorthEast { get; } = new MatchCell { Row = 2, Column = 6 };

        internal static MatchCell NorthOfNorthEast { get; } = new MatchCell { Row = 0, Column = 6 };

        internal static MatchCell EastOfNorthWest { get; } = new MatchCell { Row = 1, Column = 2 };

        internal static MatchCell GroundCell { get; } = new MatchCell { Row = 4, Column = 4 };

        internal static MatchCell OutsideCell { get; } = new MatchCell { Row = 8, Column = 0 };

        internal static IReadOnlyList<MatchCell> FiveFlowerCells { get; } = new MatchCell[] { NorthWestFlower, NorthEastFlower, CenterFlower, SouthWestFlower, SouthEastFlower };

        internal static MatchEntrant Ana { get; } = new MatchEntrant(10, 1, "Ana", false, true, SeatColor.Green);

        internal static MatchEntrant Beto { get; } = new MatchEntrant(20, 2, "Beto", false, true, SeatColor.Blue);

        internal static MatchEntrant CaroGuest { get; } = new MatchEntrant(-1, 3, "Caro", true, true, SeatColor.Red);

        internal static MatchEntrant Dani { get; } = new MatchEntrant(40, 4, "Dani", false, true, SeatColor.Yellow);

        internal static MatchEntrant InactiveEva { get; } = new MatchEntrant(50, 5, "Eva", false, false, SeatColor.Red);

        internal static MatchEntrant EliGuest { get; } = new MatchEntrant(-2, 6, "Eli", true, true, SeatColor.Yellow);

        internal static IReadOnlyList<MatchEntrant> AnaAndBeto { get; } = new MatchEntrant[] { Ana, Beto };

        internal static IReadOnlyList<MatchEntrant> AnaBetoAndDani { get; } = new MatchEntrant[] { Ana, Beto, Dani };

        internal static IReadOnlyList<MatchEntrant> CaroAndDani { get; } = new MatchEntrant[] { CaroGuest, Dani };

        internal static IReadOnlyList<MatchEntrant> AnaAndCaroGuest { get; } = new MatchEntrant[] { Ana, CaroGuest };

        internal static IReadOnlyList<MatchEntrant> CaroAndEliGuests { get; } = new MatchEntrant[] { CaroGuest, EliGuest };

        internal static int FindPlanterNumber(MatchView view, MatchCell flowerCell)
        {
            int planterNumber = view.Flowers.First(flower => (flower.Cell.Row == flowerCell.Row) && (flower.Cell.Column == flowerCell.Column)).PlanterNumber;

            return planterNumber;
        }

        internal static bool HasCaterpillarOn(MatchView view, MatchCell cell)
        {
            bool hasCaterpillar = view.Caterpillars.Any(caterpillar => (caterpillar.Cell.Row == cell.Row) && (caterpillar.Cell.Column == cell.Column));

            return hasCaterpillar;
        }

        internal static int FindFlowerLevel(MatchView view, MatchCell flowerCell)
        {
            int flowerLevel = view.Flowers.First(flower => (flower.Cell.Row == flowerCell.Row) && (flower.Cell.Column == flowerCell.Column)).Level;

            return flowerLevel;
        }
    }
}
