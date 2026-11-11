using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Game.Domain.Tests
{
    public sealed class MatchTableRoundTests
    {
        private const int TwoSeatCount = 2;
        private const int ThreeSeatCount = 3;
        private const int SeatNumberOne = 1;
        private const int SeatNumberTwo = 2;
        private const int SeatNumberThree = 3;
        private const int RoundNumberTwo = 2;
        private const int OnePoint = 1;
        private const int TwoPoints = 2;
        private const int TenPoints = 10;
        private const int OneScoredPlanter = 1;
        private const int TwoScoredPlanters = 2;

        [Fact]
        public void Test_ExpireTurn_ClosingRoundOne_AddsEachSeatPlanterPoints()
        {
            MatchTable table = CreatePlayedOpening();

            ExpireTurns(table, TwoSeatCount);

            Assert.All(table.Standings, standing => Assert.Equal(OnePoint, Assert.Single(standing.RoundScores).TotalPoints));
        }

        [Fact]
        public void Test_ExpireTurn_ClosingRoundOneWithATie_GivesTheButterflyToOneOfTheTiedSeats()
        {
            MatchTable table = CreatePlayedOpening();

            ExpireTurns(table, TwoSeatCount);

            Assert.Equal(TablePhase.ButterflyPlacement, table.Phase);
            Assert.Contains(table.ActiveSeatNumber, new int[] { SeatNumberOne, SeatNumberTwo });
        }

        [Fact]
        public void Test_ExpireTurn_ClosingRoundOne_GivesTheButterflyToTheLowestScore()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);

            ExpireTurns(table, TwoSeatCount);

            Assert.Equal(SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_ExpirePlacement_OfRoundTwoButterfly_PassesItToTheNextLowestScore()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, TwoSeatCount);

            table.ExpirePlacement();

            Assert.Equal(TablePhase.ButterflyPlacement, table.Phase);
            Assert.Equal(SeatNumberOne, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_ExpirePlacement_OfEveryRoundTwoPlacer_LeavesTheButterflyWhereItWas()
        {
            MatchTable table = CreatePlayedOpening();
            ExpireTurns(table, TwoSeatCount);

            table.ExpirePlacement();
            table.ExpirePlacement();

            Assert.Equal(TablePhase.PlayerTurn, table.Phase);
            Assert.Equal(SampleLayouts.SouthEastFlower, table.ButterflyCell);
        }

        [Fact]
        public void Test_PlaceButterfly_InRoundTwo_StartsItsFirstTurnWithSeatOne()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, TwoSeatCount);

            PlacementOutcome outcome = table.PlaceButterfly(SeatNumberTwo, SampleLayouts.NorthEastFlower);

            Assert.Equal(PlacementOutcome.Placed, outcome);
            Assert.Equal(RoundNumberTwo, table.RoundNumber);
            Assert.Equal(MatchTable.FirstTurnNumber, table.TurnNumber);
            Assert.Equal(SeatNumberOne, table.ActiveSeatNumber);
            Assert.Equal(MatchTable.ActionPointsPerTurn, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_PlaceButterfly_WithCaterpillarOnItsFlower_AddsTheRoundBonus()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, TwoSeatCount);
            table.PlaceButterfly(SeatNumberTwo, SampleLayouts.NorthEastFlower);

            ExpireTurns(table, TwoSeatCount);

            Assert.Equal(new SeatRoundScore(OnePoint, TenPoints, OneScoredPlanter), table.Standings[SeatNumberTwo - MatchTable.FirstSeatNumber].RoundScores[RoundNumberTwo - MatchTable.FirstRoundNumber]);
        }

        [Fact]
        public void Test_ExpireTurn_ClosingRoundThree_FinishesTheMatchAsCompleted()
        {
            MatchTable table = CreatePlayedOpening();

            PlayRoundsWithoutButterfly(table, MatchRules.RoundCount);

            Assert.Equal(TablePhase.Finished, table.Phase);
            Assert.Equal(FinishCause.Completed, table.FinishCause);
            Assert.All(table.Standings, standing => Assert.Equal(MatchRules.RoundCount, standing.RoundScores.Count));
            Assert.Empty(table.FindBuildTargets());
        }

        [Fact]
        public void Test_FindRanking_WithHigherTotal_PutsItFirst()
        {
            MatchTable table = CreateFinishedTable(new SeatStanding(SeatNumberOne, new SeatRoundScore[] { new SeatRoundScore(OnePoint, 0, OneScoredPlanter) }, false), new SeatStanding(SeatNumberTwo, new SeatRoundScore[] { new SeatRoundScore(TwoPoints, 0, OneScoredPlanter) }, false));

            int winnerSeatNumber = table.FindRanking()[0].SeatNumber;

            Assert.Equal(SeatNumberTwo, winnerSeatNumber);
        }

        [Fact]
        public void Test_FindRanking_WithTiedTotals_PutsMoreScoredPlantersFirst()
        {
            MatchTable table = CreateFinishedTable(new SeatStanding(SeatNumberOne, new SeatRoundScore[] { new SeatRoundScore(TwoPoints, 0, OneScoredPlanter) }, false), new SeatStanding(SeatNumberTwo, new SeatRoundScore[] { new SeatRoundScore(TwoPoints, 0, TwoScoredPlanters) }, false));

            int winnerSeatNumber = table.FindRanking()[0].SeatNumber;

            Assert.Equal(SeatNumberTwo, winnerSeatNumber);
        }

        [Fact]
        public void Test_FindRanking_WithFullTie_PutsTheLowerSeatFirst()
        {
            MatchTable table = CreateFinishedTable(new SeatStanding(SeatNumberOne, new SeatRoundScore[] { new SeatRoundScore(TwoPoints, 0, OneScoredPlanter) }, false), new SeatStanding(SeatNumberTwo, new SeatRoundScore[] { new SeatRoundScore(TwoPoints, 0, OneScoredPlanter) }, false));

            int winnerSeatNumber = table.FindRanking()[0].SeatNumber;

            Assert.Equal(SeatNumberOne, winnerSeatNumber);
        }

        [Fact]
        public void Test_RetireSeat_OfActiveSeatInATurn_GivesTheTurnToTheNextSeat()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();

            table.RetireSeat(SeatNumberOne);

            Assert.Equal(SeatNumberTwo, table.ActiveSeatNumber);
            Assert.DoesNotContain(table.Caterpillars, caterpillar => caterpillar.SeatNumber == SeatNumberOne);
        }

        [Fact]
        public void Test_RetireSeat_OfWaitingSeat_IsSkippedWhenTheTurnPasses()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();
            table.RetireSeat(SeatNumberTwo);

            table.EndTurn(SeatNumberOne);

            Assert.Equal(SeatNumberThree, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_RetireSeat_LeavingOneSeat_FinishesTheMatchWithTooFewSeats()
        {
            MatchTable table = CreatePlayedOpening();

            table.RetireSeat(SeatNumberOne);

            Assert.Equal(TablePhase.Finished, table.Phase);
            Assert.Equal(FinishCause.TooFewSeats, table.FinishCause);
            Assert.Equal(SeatNumberTwo, Assert.Single(table.FindRanking()).SeatNumber);
        }

        [Fact]
        public void Test_RetireSeat_DuringItsCaterpillarPlacement_PassesThePlacement()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateTieBreakRandom());

            table.RetireSeat(SeatNumberOne);

            Assert.Equal(TablePhase.CaterpillarPlacement, table.Phase);
            Assert.Equal(SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_RetireSeat_BeforeRoundOneCloses_LeavesTheSeatOutOfTheButterflyPlacers()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();
            table.RetireSeat(SeatNumberThree);

            ExpireTurns(table, TwoSeatCount * SampleRules.ThreeSeatRules.GetTurnCount(MatchTable.FirstRoundNumber));

            Assert.Equal(TablePhase.ButterflyPlacement, table.Phase);
            Assert.NotEqual(SeatNumberThree, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_RetireSeat_Twice_KeepsTheSeatRetired()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();
            table.RetireSeat(SeatNumberTwo);

            table.RetireSeat(SeatNumberTwo);

            Assert.True(table.IsSeatRetired(SeatNumberTwo));
            Assert.Equal(TablePhase.PlayerTurn, table.Phase);
        }

        [Fact]
        public void Test_TakeSnapshot_AfterRoundOne_KeepsTheStandingsAndPendingPlacers()
        {
            MatchTable table = CreatePlayedOpening();
            ExpireTurns(table, TwoSeatCount);

            MatchTable restoredTable = MatchTable.RestoreFromSnapshot(table.TakeSnapshot(), SampleRules.OneTurnPerRoundRules, SampleRules.CreateTieBreakRandom());
            restoredTable.ExpirePlacement();
            table.ExpirePlacement();

            Assert.Equal(table.Standings, restoredTable.Standings, new SeatStandingComparer());
            Assert.Equal(table.ActiveSeatNumber, restoredTable.ActiveSeatNumber);
        }

        private static MatchTable CreatePlayedOpening()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, TwoSeatCount, SampleRules.OneTurnPerRoundRules, SampleRules.CreateTieBreakRandom());
            table.PlaceInitialCaterpillar(SeatNumberOne, SampleLayouts.NorthWestFlower);
            table.PlaceInitialCaterpillar(SeatNumberTwo, SampleLayouts.NorthEastFlower);
            table.PlaceButterfly(SeatNumberTwo, SampleLayouts.SouthEastFlower);

            return table;
        }

        private static MatchTable CreatePlayedThreeSeatOpening()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateTieBreakRandom());
            table.PlaceInitialCaterpillar(SeatNumberOne, SampleLayouts.NorthWestFlower);
            table.PlaceInitialCaterpillar(SeatNumberTwo, SampleLayouts.NorthEastFlower);
            table.PlaceInitialCaterpillar(SeatNumberThree, SampleLayouts.SouthWestFlower);
            table.PlaceButterfly(SeatNumberThree, SampleLayouts.CenterFlower);

            return table;
        }

        private static MatchTable CreateFinishedTable(SeatStanding seatOneStanding, SeatStanding seatTwoStanding)
        {
            var snapshot = new MatchSnapshot(TablePhase.Finished, MatchRules.RoundCount, MatchTable.FirstTurnNumber, TwoSeatCount, SeatNumberOne, 0, 0, SampleLayouts.FiveStartingFlowers, new List<Caterpillar>(), null)
            {
                Standings = new SeatStanding[] { seatOneStanding, seatTwoStanding },
                FinishCause = FinishCause.Completed,
            };
            MatchTable table = MatchTable.RestoreFromSnapshot(snapshot, SampleRules.OneTurnPerRoundRules, SampleRules.CreateTieBreakRandom());

            return table;
        }

        private static void PlayRoundsWithoutButterfly(MatchTable table, int roundCount)
        {
            ExpireTurns(table, TwoSeatCount);
            for (var i = 1; i < roundCount; i++)
            {
                ExpirePlacements(table, TwoSeatCount);
                ExpireTurns(table, TwoSeatCount);
            }
        }

        private static void ExpireTurns(MatchTable table, int turnCount)
        {
            for (var i = 0; i < turnCount; i++)
            {
                table.ExpireTurn();
            }
        }

        private static void ExpirePlacements(MatchTable table, int placementCount)
        {
            for (var i = 0; i < placementCount; i++)
            {
                table.ExpirePlacement();
            }
        }

        private sealed class SeatStandingComparer : IEqualityComparer<SeatStanding>
        {
            public bool Equals(SeatStanding? x, SeatStanding? y)
            {
                bool areEqual = (x is not null) && (y is not null) && (x.SeatNumber == y.SeatNumber) && x.RoundScores.SequenceEqual(y.RoundScores) && (x.ScoredPlanterCount == y.ScoredPlanterCount) && (x.IsRetired == y.IsRetired);

                return areEqual;
            }

            public int GetHashCode(SeatStanding obj)
            {
                int hashCode = obj.SeatNumber;

                return hashCode;
            }
        }
    }
}
