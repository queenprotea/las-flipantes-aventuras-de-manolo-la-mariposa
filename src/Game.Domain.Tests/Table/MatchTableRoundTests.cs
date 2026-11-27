using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Game.Domain.Tests
{
    public sealed class MatchTableRoundTests
    {
        private const int OnePoint = 1;
        private const int TwoPoints = 2;
        private const int TenPoints = 10;
        private const int TwoScoredPlanters = 2;
        private const int NoCardPoints = 0;
        private const int SecondSeatIndex = 1;
        private const int SecondRoundIndex = 1;

        [Fact]
        public void Test_ExpireTurn_ClosingRoundOne_AddsEachSeatPlanterPoints()
        {
            MatchTable table = CreatePlayedOpening();

            ExpireTurns(table, SampleRules.TwoSeatCount);

            Assert.Equal(new int[] { OnePoint, OnePoint }, table.Standings.Select(standing => standing.RoundScores.Single().TotalPoints));
        }

        [Fact]
        public void Test_ExpireTurn_ClosingRoundOneWithATie_OpensTheButterflyPlacement()
        {
            MatchTable table = CreatePlayedOpening();

            ExpireTurns(table, SampleRules.TwoSeatCount);

            Assert.Equal(TablePhase.ButterflyPlacement, table.Phase);
        }

        [Fact]
        public void Test_ExpireTurn_ClosingRoundOneWithATie_GivesTheButterflyToOneOfTheTiedSeats()
        {
            MatchTable table = CreatePlayedOpening();

            ExpireTurns(table, SampleRules.TwoSeatCount);

            Assert.Contains(table.ActiveSeatNumber, new int[] { SampleRules.SeatNumberOne, SampleRules.SeatNumberTwo });
        }

        [Fact]
        public void Test_ExpireTurn_ClosingRoundOne_GivesTheButterflyToTheLowestScore()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);

            ExpireTurns(table, SampleRules.TwoSeatCount);

            Assert.Equal(SampleRules.SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_ExpirePlacement_OfRoundTwoButterfly_KeepsTheButterflyPlacementOpen()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, SampleRules.TwoSeatCount);

            table.ExpirePlacement();

            Assert.Equal(TablePhase.ButterflyPlacement, table.Phase);
        }

        [Fact]
        public void Test_ExpirePlacement_OfRoundTwoButterfly_PassesItToTheNextLowestScore()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, SampleRules.TwoSeatCount);

            table.ExpirePlacement();

            Assert.Equal(SampleRules.SeatNumberOne, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_ExpirePlacement_OfEveryRoundTwoPlacer_StartsThePlayerTurn()
        {
            MatchTable table = CreatePlayedOpening();
            ExpireTurns(table, SampleRules.TwoSeatCount);
            table.ExpirePlacement();

            table.ExpirePlacement();

            Assert.Equal(TablePhase.PlayerTurn, table.Phase);
        }

        [Fact]
        public void Test_ExpirePlacement_OfEveryRoundTwoPlacer_LeavesTheButterflyWhereItWas()
        {
            MatchTable table = CreatePlayedOpening();
            ExpireTurns(table, SampleRules.TwoSeatCount);
            table.ExpirePlacement();

            table.ExpirePlacement();

            Assert.Equal(SampleLayouts.SouthEastFlower, table.ButterflyCell);
        }

        [Fact]
        public void Test_PlaceButterfly_InRoundTwo_ReturnsPlaced()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, SampleRules.TwoSeatCount);

            PlacementOutcome outcome = table.PlaceButterfly(SampleRules.SeatNumberTwo, SampleLayouts.NorthEastFlower);

            Assert.Equal(PlacementOutcome.Placed, outcome);
        }

        [Fact]
        public void Test_PlaceButterfly_InRoundTwo_MovesToRoundTwo()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, SampleRules.TwoSeatCount);

            table.PlaceButterfly(SampleRules.SeatNumberTwo, SampleLayouts.NorthEastFlower);

            Assert.Equal(SampleRules.RoundNumberTwo, table.RoundNumber);
        }

        [Fact]
        public void Test_PlaceButterfly_InRoundTwo_StartsItsFirstTurnRound()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, SampleRules.TwoSeatCount);

            table.PlaceButterfly(SampleRules.SeatNumberTwo, SampleLayouts.NorthEastFlower);

            Assert.Equal(MatchTable.FirstTurnRoundNumber, table.TurnRoundNumber);
        }

        [Fact]
        public void Test_PlaceButterfly_InRoundTwo_StartsItsFirstTurnWithSeatOne()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, SampleRules.TwoSeatCount);

            table.PlaceButterfly(SampleRules.SeatNumberTwo, SampleLayouts.NorthEastFlower);

            Assert.Equal(SampleRules.SeatNumberOne, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_PlaceButterfly_InRoundTwo_GivesTheWholeActionPoints()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, SampleRules.TwoSeatCount);

            table.PlaceButterfly(SampleRules.SeatNumberTwo, SampleLayouts.NorthEastFlower);

            Assert.Equal(MatchTable.ActionPointsPerTurn, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_PlaceButterfly_WithCaterpillarOnItsFlower_AddsTheRoundBonus()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, SampleRules.TwoSeatCount);
            table.PlaceButterfly(SampleRules.SeatNumberTwo, SampleLayouts.NorthEastFlower);

            ExpireTurns(table, SampleRules.TwoSeatCount);

            SeatRoundScore secondRoundScore = table.Standings[SecondSeatIndex].RoundScores[SecondRoundIndex];
            Assert.Equal(new SeatRoundScore(OnePoint, TenPoints, SampleLayouts.OneScoredPlanter), secondRoundScore);
        }

        [Fact]
        public void Test_ExpireTurn_ClosingRoundThree_FinishesTheMatch()
        {
            MatchTable table = CreatePlayedOpening();

            PlayRoundsWithoutButterfly(table, MatchRules.RoundCount);

            Assert.Equal(TablePhase.Finished, table.Phase);
        }

        [Fact]
        public void Test_ExpireTurn_ClosingRoundThree_FinishesTheMatchAsCompleted()
        {
            MatchTable table = CreatePlayedOpening();

            PlayRoundsWithoutButterfly(table, MatchRules.RoundCount);

            Assert.Equal(FinishCause.Completed, table.FinishCause);
        }

        [Fact]
        public void Test_ExpireTurn_ClosingRoundThree_KeepsTheScoreOfEveryRound()
        {
            MatchTable table = CreatePlayedOpening();

            PlayRoundsWithoutButterfly(table, MatchRules.RoundCount);

            Assert.Equal(new int[] { MatchRules.RoundCount, MatchRules.RoundCount }, table.Standings.Select(standing => standing.RoundScores.Count));
        }

        [Fact]
        public void Test_ExpireTurn_ClosingRoundThree_OffersNoBuildTarget()
        {
            MatchTable table = CreatePlayedOpening();

            PlayRoundsWithoutButterfly(table, MatchRules.RoundCount);

            Assert.Empty(table.FindBuildTargets());
        }

        [Fact]
        public void Test_FindRanking_WithHigherTotal_PutsItFirst()
        {
            MatchTable table = CreateFinishedTable(new SeatStanding(SampleRules.SeatNumberOne, new SeatRoundScore[] { new SeatRoundScore(OnePoint, 0, SampleLayouts.OneScoredPlanter) }, false, NoCardPoints), new SeatStanding(SampleRules.SeatNumberTwo, new SeatRoundScore[] { new SeatRoundScore(TwoPoints, 0, SampleLayouts.OneScoredPlanter) }, false, NoCardPoints));

            int winnerSeatNumber = table.FindRanking()[0].SeatNumber;

            Assert.Equal(SampleRules.SeatNumberTwo, winnerSeatNumber);
        }

        [Fact]
        public void Test_FindRanking_WithTiedTotals_PutsMoreScoredPlantersFirst()
        {
            MatchTable table = CreateFinishedTable(new SeatStanding(SampleRules.SeatNumberOne, new SeatRoundScore[] { new SeatRoundScore(TwoPoints, 0, SampleLayouts.OneScoredPlanter) }, false, NoCardPoints), new SeatStanding(SampleRules.SeatNumberTwo, new SeatRoundScore[] { new SeatRoundScore(TwoPoints, 0, TwoScoredPlanters) }, false, NoCardPoints));

            int winnerSeatNumber = table.FindRanking()[0].SeatNumber;

            Assert.Equal(SampleRules.SeatNumberTwo, winnerSeatNumber);
        }

        [Fact]
        public void Test_FindRanking_WithFullTie_PutsTheLowerSeatFirst()
        {
            MatchTable table = CreateFinishedTable(new SeatStanding(SampleRules.SeatNumberOne, new SeatRoundScore[] { new SeatRoundScore(TwoPoints, 0, SampleLayouts.OneScoredPlanter) }, false, NoCardPoints), new SeatStanding(SampleRules.SeatNumberTwo, new SeatRoundScore[] { new SeatRoundScore(TwoPoints, 0, SampleLayouts.OneScoredPlanter) }, false, NoCardPoints));

            int winnerSeatNumber = table.FindRanking()[0].SeatNumber;

            Assert.Equal(SampleRules.SeatNumberOne, winnerSeatNumber);
        }

        [Fact]
        public void Test_FindFinishingOrder_WithRetiredSeatOfHigherTotal_PutsItAfterTheSeatStillPlaying()
        {
            MatchTable table = CreateFinishedTable(new SeatStanding(SampleRules.SeatNumberOne, new SeatRoundScore[] { new SeatRoundScore(TenPoints, 0, SampleLayouts.OneScoredPlanter) }, true, NoCardPoints), new SeatStanding(SampleRules.SeatNumberTwo, new SeatRoundScore[] { new SeatRoundScore(OnePoint, 0, SampleLayouts.OneScoredPlanter) }, false, NoCardPoints));

            List<int> finishingSeatNumbers = table.FindFinishingOrder().Select(standing => standing.SeatNumber).ToList();

            Assert.Equal(new int[] { SampleRules.SeatNumberTwo, SampleRules.SeatNumberOne }, finishingSeatNumbers);
        }

        [Fact]
        public void Test_FindFinishingOrder_WithTwoRetiredSeats_OrdersThemByFinalScore()
        {
            MatchTable table = CreateFinishedTable(new SeatStanding(SampleRules.SeatNumberOne, new SeatRoundScore[] { new SeatRoundScore(OnePoint, 0, SampleLayouts.OneScoredPlanter) }, true, NoCardPoints), new SeatStanding(SampleRules.SeatNumberTwo, new SeatRoundScore[] { new SeatRoundScore(TenPoints, 0, SampleLayouts.OneScoredPlanter) }, true, NoCardPoints));

            List<int> finishingSeatNumbers = table.FindFinishingOrder().Select(standing => standing.SeatNumber).ToList();

            Assert.Equal(new int[] { SampleRules.SeatNumberTwo, SampleRules.SeatNumberOne }, finishingSeatNumbers);
        }

        [Fact]
        public void Test_RetireSeat_OfActiveSeatInATurn_GivesTheTurnToTheNextSeat()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();

            table.RetireSeat(SampleRules.SeatNumberOne);

            Assert.Equal(SampleRules.SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_RetireSeat_OfActiveSeatInATurn_TakesItsCaterpillarsOffTheBoard()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();

            table.RetireSeat(SampleRules.SeatNumberOne);

            Assert.DoesNotContain(SampleRules.SeatNumberOne, table.Caterpillars.Select(caterpillar => caterpillar.SeatNumber));
        }

        [Fact]
        public void Test_RetireSeat_OfWaitingSeat_IsSkippedWhenTheTurnPasses()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();
            table.RetireSeat(SampleRules.SeatNumberTwo);

            table.EndTurn(SampleRules.SeatNumberOne);

            Assert.Equal(SampleRules.SeatNumberThree, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_RetireSeat_LeavingOneSeat_FinishesTheMatch()
        {
            MatchTable table = CreatePlayedOpening();

            table.RetireSeat(SampleRules.SeatNumberOne);

            Assert.Equal(TablePhase.Finished, table.Phase);
        }

        [Fact]
        public void Test_RetireSeat_LeavingOneSeat_FinishesTheMatchWithTooFewSeats()
        {
            MatchTable table = CreatePlayedOpening();

            table.RetireSeat(SampleRules.SeatNumberOne);

            Assert.Equal(FinishCause.TooFewSeats, table.FinishCause);
        }

        [Fact]
        public void Test_RetireSeat_LeavingOneSeat_RanksOnlyTheRemainingSeat()
        {
            MatchTable table = CreatePlayedOpening();

            table.RetireSeat(SampleRules.SeatNumberOne);

            Assert.Equal(new int[] { SampleRules.SeatNumberTwo }, table.FindRanking().Select(standing => standing.SeatNumber));
        }

        [Fact]
        public void Test_RetireSeat_DuringItsCaterpillarPlacement_KeepsTheCaterpillarPlacementOpen()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, SampleRules.ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            table.RetireSeat(SampleRules.SeatNumberOne);

            Assert.Equal(TablePhase.CaterpillarPlacement, table.Phase);
        }

        [Fact]
        public void Test_RetireSeat_DuringItsCaterpillarPlacement_PassesThePlacement()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, SampleRules.ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            table.RetireSeat(SampleRules.SeatNumberOne);

            Assert.Equal(SampleRules.SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_RetireSeat_BeforeRoundOneCloses_OpensTheButterflyPlacement()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();
            table.RetireSeat(SampleRules.SeatNumberThree);

            ExpireTurns(table, SampleRules.TwoSeatCount * SampleRules.ThreeSeatRules.GetTurnCount(MatchTable.FirstRoundNumber));

            Assert.Equal(TablePhase.ButterflyPlacement, table.Phase);
        }

        [Fact]
        public void Test_RetireSeat_BeforeRoundOneCloses_LeavesTheSeatOutOfTheButterflyPlacers()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();
            table.RetireSeat(SampleRules.SeatNumberThree);

            ExpireTurns(table, SampleRules.TwoSeatCount * SampleRules.ThreeSeatRules.GetTurnCount(MatchTable.FirstRoundNumber));

            Assert.NotEqual(SampleRules.SeatNumberThree, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_RetireSeat_Twice_KeepsTheSeatRetired()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();
            table.RetireSeat(SampleRules.SeatNumberTwo);

            table.RetireSeat(SampleRules.SeatNumberTwo);

            Assert.True(table.IsSeatRetired(SampleRules.SeatNumberTwo));
        }

        [Fact]
        public void Test_RetireSeat_Twice_KeepsTheTurnGoing()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();
            table.RetireSeat(SampleRules.SeatNumberTwo);

            table.RetireSeat(SampleRules.SeatNumberTwo);

            Assert.Equal(TablePhase.PlayerTurn, table.Phase);
        }

        [Fact]
        public void Test_TakeSnapshot_AfterRoundOne_KeepsTheStandings()
        {
            MatchTable table = CreatePlayedOpening();
            ExpireTurns(table, SampleRules.TwoSeatCount);

            MatchTable restoredTable = MatchTable.RestoreFromSnapshot(table.TakeSnapshot(), SampleRules.OneTurnPerRoundRules, SampleRules.CreateChanceSource());

            Assert.Equal(table.Standings, restoredTable.Standings, new SeatStandingComparer());
        }

        [Fact]
        public void Test_ExpirePlacement_OnTableRestoredAfterRoundOne_PassesItToTheSamePlacer()
        {
            MatchTable table = CreatePlayedOpening();
            ExpireTurns(table, SampleRules.TwoSeatCount);
            MatchTable restoredTable = MatchTable.RestoreFromSnapshot(table.TakeSnapshot(), SampleRules.OneTurnPerRoundRules, SampleRules.CreateChanceSource());
            table.ExpirePlacement();

            restoredTable.ExpirePlacement();

            Assert.Equal(table.ActiveSeatNumber, restoredTable.ActiveSeatNumber);
        }

        private static MatchTable CreatePlayedOpening()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, SampleRules.TwoSeatCount, SampleRules.OneTurnPerRoundRules, SampleRules.CreateChanceSource());
            table.PlaceInitialCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.NorthWestFlower);
            table.PlaceInitialCaterpillar(SampleRules.SeatNumberTwo, SampleLayouts.NorthEastFlower);
            table.PlaceButterfly(SampleRules.SeatNumberTwo, SampleLayouts.SouthEastFlower);

            return table;
        }

        private static MatchTable CreatePlayedThreeSeatOpening()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, SampleRules.ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());
            table.PlaceInitialCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.NorthWestFlower);
            table.PlaceInitialCaterpillar(SampleRules.SeatNumberTwo, SampleLayouts.NorthEastFlower);
            table.PlaceInitialCaterpillar(SampleRules.SeatNumberThree, SampleLayouts.SouthWestFlower);
            table.PlaceButterfly(SampleRules.SeatNumberThree, SampleLayouts.CenterFlower);

            return table;
        }

        private static MatchTable CreateFinishedTable(SeatStanding seatOneStanding, SeatStanding seatTwoStanding)
        {
            var snapshot = new MatchSnapshot(TablePhase.Finished, MatchRules.RoundCount, MatchTable.FirstTurnRoundNumber, SampleRules.TwoSeatCount, SampleRules.SeatNumberOne, 0, 0, SampleLayouts.FiveStartingFlowers, new List<Caterpillar>(), null, new SeatStanding[] { seatOneStanding, seatTwoStanding }, Array.Empty<SeatCards>(), Array.Empty<int>(), FinishCause.Completed);
            MatchTable table = MatchTable.RestoreFromSnapshot(snapshot, SampleRules.OneTurnPerRoundRules, SampleRules.CreateChanceSource());

            return table;
        }

        private static void PlayRoundsWithoutButterfly(MatchTable table, int roundCount)
        {
            ExpireTurns(table, SampleRules.TwoSeatCount);
            for (var i = 1; i < roundCount; i++)
            {
                ExpirePlacements(table, SampleRules.TwoSeatCount);
                ExpireTurns(table, SampleRules.TwoSeatCount);
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
            public bool Equals(SeatStanding? firstStanding, SeatStanding? secondStanding)
            {
                bool areEqual = (firstStanding is SeatStanding comparedFirstStanding) && (secondStanding is SeatStanding comparedSecondStanding) && HaveSameScores(comparedFirstStanding, comparedSecondStanding);

                return areEqual;
            }

            public int GetHashCode(SeatStanding standing)
            {
                int hashCode = standing.SeatNumber;

                return hashCode;
            }

            private static bool HaveSameScores(SeatStanding firstStanding, SeatStanding secondStanding)
            {
                bool haveSameRounds = (firstStanding.SeatNumber == secondStanding.SeatNumber) && firstStanding.RoundScores.SequenceEqual(secondStanding.RoundScores);
                bool haveSameEnding = (firstStanding.ScoredPlanterCount == secondStanding.ScoredPlanterCount) && (firstStanding.IsRetired == secondStanding.IsRetired);
                bool haveSameScores = haveSameRounds && haveSameEnding;

                return haveSameScores;
            }
        }
    }
}
