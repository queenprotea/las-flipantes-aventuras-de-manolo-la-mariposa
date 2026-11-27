using System;
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
        private const int NoCardPoints = 0;
        private const int SecondSeatIndex = 1;
        private const int SecondRoundIndex = 1;

        [Fact]
        public void Test_ExpireTurn_ClosingRoundOne_AddsEachSeatPlanterPoints()
        {
            MatchTable table = CreatePlayedOpening();

            ExpireTurns(table, TwoSeatCount);

            Assert.Equal(new int[] { OnePoint, OnePoint }, table.Standings.Select(standing => standing.RoundScores.Single().TotalPoints));
        }

        [Fact]
        public void Test_ExpireTurn_ClosingRoundOneWithATie_OpensTheButterflyPlacement()
        {
            MatchTable table = CreatePlayedOpening();

            ExpireTurns(table, TwoSeatCount);

            Assert.Equal(TablePhase.ButterflyPlacement, table.Phase);
        }

        [Fact]
        public void Test_ExpireTurn_ClosingRoundOneWithATie_GivesTheButterflyToOneOfTheTiedSeats()
        {
            MatchTable table = CreatePlayedOpening();

            ExpireTurns(table, TwoSeatCount);

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
        public void Test_ExpirePlacement_OfRoundTwoButterfly_KeepsTheButterflyPlacementOpen()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, TwoSeatCount);

            table.ExpirePlacement();

            Assert.Equal(TablePhase.ButterflyPlacement, table.Phase);
        }

        [Fact]
        public void Test_ExpirePlacement_OfRoundTwoButterfly_PassesItToTheNextLowestScore()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, TwoSeatCount);

            table.ExpirePlacement();

            Assert.Equal(SeatNumberOne, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_ExpirePlacement_OfEveryRoundTwoPlacer_StartsThePlayerTurn()
        {
            MatchTable table = CreatePlayedOpening();
            ExpireTurns(table, TwoSeatCount);
            table.ExpirePlacement();

            table.ExpirePlacement();

            Assert.Equal(TablePhase.PlayerTurn, table.Phase);
        }

        [Fact]
        public void Test_ExpirePlacement_OfEveryRoundTwoPlacer_LeavesTheButterflyWhereItWas()
        {
            MatchTable table = CreatePlayedOpening();
            ExpireTurns(table, TwoSeatCount);
            table.ExpirePlacement();

            table.ExpirePlacement();

            Assert.Equal(SampleLayouts.SouthEastFlower, table.ButterflyCell);
        }

        [Fact]
        public void Test_PlaceButterfly_InRoundTwo_ReturnsPlaced()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, TwoSeatCount);

            PlacementOutcome outcome = table.PlaceButterfly(SeatNumberTwo, SampleLayouts.NorthEastFlower);

            Assert.Equal(PlacementOutcome.Placed, outcome);
        }

        [Fact]
        public void Test_PlaceButterfly_InRoundTwo_MovesToRoundTwo()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, TwoSeatCount);

            table.PlaceButterfly(SeatNumberTwo, SampleLayouts.NorthEastFlower);

            Assert.Equal(RoundNumberTwo, table.RoundNumber);
        }

        [Fact]
        public void Test_PlaceButterfly_InRoundTwo_StartsItsFirstTurnRound()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, TwoSeatCount);

            table.PlaceButterfly(SeatNumberTwo, SampleLayouts.NorthEastFlower);

            Assert.Equal(MatchTable.FirstTurnRoundNumber, table.TurnRoundNumber);
        }

        [Fact]
        public void Test_PlaceButterfly_InRoundTwo_StartsItsFirstTurnWithSeatOne()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, TwoSeatCount);

            table.PlaceButterfly(SeatNumberTwo, SampleLayouts.NorthEastFlower);

            Assert.Equal(SeatNumberOne, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_PlaceButterfly_InRoundTwo_GivesTheWholeActionPoints()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);
            ExpireTurns(table, TwoSeatCount);

            table.PlaceButterfly(SeatNumberTwo, SampleLayouts.NorthEastFlower);

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

            SeatRoundScore secondRoundScore = table.Standings[SecondSeatIndex].RoundScores[SecondRoundIndex];
            Assert.Equal(new SeatRoundScore(OnePoint, TenPoints, OneScoredPlanter), secondRoundScore);
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

            IReadOnlyList<BoardCell> buildTargets = table.FindBuildTargets();
            Assert.Empty(buildTargets);
        }

        [Fact]
        public void Test_FindRanking_WithHigherTotal_PutsItFirst()
        {
            MatchTable table = CreateFinishedTable(new SeatStanding(SeatNumberOne, new SeatRoundScore[] { new SeatRoundScore(OnePoint, 0, OneScoredPlanter) }, false, NoCardPoints), new SeatStanding(SeatNumberTwo, new SeatRoundScore[] { new SeatRoundScore(TwoPoints, 0, OneScoredPlanter) }, false, NoCardPoints));

            int winnerSeatNumber = table.FindRanking()[0].SeatNumber;

            Assert.Equal(SeatNumberTwo, winnerSeatNumber);
        }

        [Fact]
        public void Test_FindRanking_WithTiedTotals_PutsMoreScoredPlantersFirst()
        {
            MatchTable table = CreateFinishedTable(new SeatStanding(SeatNumberOne, new SeatRoundScore[] { new SeatRoundScore(TwoPoints, 0, OneScoredPlanter) }, false, NoCardPoints), new SeatStanding(SeatNumberTwo, new SeatRoundScore[] { new SeatRoundScore(TwoPoints, 0, TwoScoredPlanters) }, false, NoCardPoints));

            int winnerSeatNumber = table.FindRanking()[0].SeatNumber;

            Assert.Equal(SeatNumberTwo, winnerSeatNumber);
        }

        [Fact]
        public void Test_FindRanking_WithFullTie_PutsTheLowerSeatFirst()
        {
            MatchTable table = CreateFinishedTable(new SeatStanding(SeatNumberOne, new SeatRoundScore[] { new SeatRoundScore(TwoPoints, 0, OneScoredPlanter) }, false, NoCardPoints), new SeatStanding(SeatNumberTwo, new SeatRoundScore[] { new SeatRoundScore(TwoPoints, 0, OneScoredPlanter) }, false, NoCardPoints));

            int winnerSeatNumber = table.FindRanking()[0].SeatNumber;

            Assert.Equal(SeatNumberOne, winnerSeatNumber);
        }

        [Fact]
        public void Test_FindFinishingOrder_WithRetiredSeatOfHigherTotal_PutsItAfterTheSeatStillPlaying()
        {
            MatchTable table = CreateFinishedTable(new SeatStanding(SeatNumberOne, new SeatRoundScore[] { new SeatRoundScore(TenPoints, 0, OneScoredPlanter) }, true, NoCardPoints), new SeatStanding(SeatNumberTwo, new SeatRoundScore[] { new SeatRoundScore(OnePoint, 0, OneScoredPlanter) }, false, NoCardPoints));

            List<int> finishingSeatNumbers = table.FindFinishingOrder().Select(standing => standing.SeatNumber).ToList();

            Assert.Equal(new int[] { SeatNumberTwo, SeatNumberOne }, finishingSeatNumbers);
        }

        [Fact]
        public void Test_FindFinishingOrder_WithTwoRetiredSeats_OrdersThemByFinalScore()
        {
            MatchTable table = CreateFinishedTable(new SeatStanding(SeatNumberOne, new SeatRoundScore[] { new SeatRoundScore(OnePoint, 0, OneScoredPlanter) }, true, NoCardPoints), new SeatStanding(SeatNumberTwo, new SeatRoundScore[] { new SeatRoundScore(TenPoints, 0, OneScoredPlanter) }, true, NoCardPoints));

            List<int> finishingSeatNumbers = table.FindFinishingOrder().Select(standing => standing.SeatNumber).ToList();

            Assert.Equal(new int[] { SeatNumberTwo, SeatNumberOne }, finishingSeatNumbers);
        }

        [Fact]
        public void Test_RetireSeat_OfActiveSeatInATurn_GivesTheTurnToTheNextSeat()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();

            table.RetireSeat(SeatNumberOne);

            Assert.Equal(SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_RetireSeat_OfActiveSeatInATurn_TakesItsCaterpillarsOffTheBoard()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();

            table.RetireSeat(SeatNumberOne);

            Assert.DoesNotContain(SeatNumberOne, table.Caterpillars.Select(caterpillar => caterpillar.SeatNumber));
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
        public void Test_RetireSeat_LeavingOneSeat_FinishesTheMatch()
        {
            MatchTable table = CreatePlayedOpening();

            table.RetireSeat(SeatNumberOne);

            Assert.Equal(TablePhase.Finished, table.Phase);
        }

        [Fact]
        public void Test_RetireSeat_LeavingOneSeat_FinishesTheMatchWithTooFewSeats()
        {
            MatchTable table = CreatePlayedOpening();

            table.RetireSeat(SeatNumberOne);

            Assert.Equal(FinishCause.TooFewSeats, table.FinishCause);
        }

        [Fact]
        public void Test_RetireSeat_LeavingOneSeat_RanksOnlyTheRemainingSeat()
        {
            MatchTable table = CreatePlayedOpening();

            table.RetireSeat(SeatNumberOne);

            IReadOnlyList<SeatStanding> ranking = table.FindRanking();
            Assert.Equal(new int[] { SeatNumberTwo }, ranking.Select(standing => standing.SeatNumber));
        }

        [Fact]
        public void Test_RetireSeat_DuringItsCaterpillarPlacement_KeepsTheCaterpillarPlacementOpen()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            table.RetireSeat(SeatNumberOne);

            Assert.Equal(TablePhase.CaterpillarPlacement, table.Phase);
        }

        [Fact]
        public void Test_RetireSeat_DuringItsCaterpillarPlacement_PassesThePlacement()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            table.RetireSeat(SeatNumberOne);

            Assert.Equal(SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_RetireSeat_BeforeRoundOneCloses_OpensTheButterflyPlacement()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();
            table.RetireSeat(SeatNumberThree);

            ExpireTurns(table, TwoSeatCount * SampleRules.ThreeSeatRules.GetTurnCount(MatchTable.FirstRoundNumber));

            Assert.Equal(TablePhase.ButterflyPlacement, table.Phase);
        }

        [Fact]
        public void Test_RetireSeat_BeforeRoundOneCloses_LeavesTheSeatOutOfTheButterflyPlacers()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();
            table.RetireSeat(SeatNumberThree);

            ExpireTurns(table, TwoSeatCount * SampleRules.ThreeSeatRules.GetTurnCount(MatchTable.FirstRoundNumber));

            Assert.NotEqual(SeatNumberThree, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_RetireSeat_Twice_KeepsTheSeatRetired()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();
            table.RetireSeat(SeatNumberTwo);

            table.RetireSeat(SeatNumberTwo);

            bool isSeatRetired = table.IsSeatRetired(SeatNumberTwo);
            Assert.True(isSeatRetired);
        }

        [Fact]
        public void Test_RetireSeat_Twice_KeepsTheTurnGoing()
        {
            MatchTable table = CreatePlayedThreeSeatOpening();
            table.RetireSeat(SeatNumberTwo);

            table.RetireSeat(SeatNumberTwo);

            Assert.Equal(TablePhase.PlayerTurn, table.Phase);
        }

        [Fact]
        public void Test_TakeSnapshot_AfterRoundOne_KeepsTheStandings()
        {
            MatchTable table = CreatePlayedOpening();
            ExpireTurns(table, TwoSeatCount);

            MatchTable restoredTable = MatchTable.RestoreFromSnapshot(table.TakeSnapshot(), SampleRules.OneTurnPerRoundRules, SampleRules.CreateChanceSource());

            Assert.Equal(table.Standings, restoredTable.Standings, new SeatStandingComparer());
        }

        [Fact]
        public void Test_ExpirePlacement_OnTableRestoredAfterRoundOne_PassesItToTheSamePlacer()
        {
            MatchTable table = CreatePlayedOpening();
            ExpireTurns(table, TwoSeatCount);
            MatchTable restoredTable = MatchTable.RestoreFromSnapshot(table.TakeSnapshot(), SampleRules.OneTurnPerRoundRules, SampleRules.CreateChanceSource());
            table.ExpirePlacement();

            restoredTable.ExpirePlacement();

            Assert.Equal(table.ActiveSeatNumber, restoredTable.ActiveSeatNumber);
        }

        private static MatchTable CreatePlayedOpening()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, TwoSeatCount, SampleRules.OneTurnPerRoundRules, SampleRules.CreateChanceSource());
            table.PlaceInitialCaterpillar(SeatNumberOne, SampleLayouts.NorthWestFlower);
            table.PlaceInitialCaterpillar(SeatNumberTwo, SampleLayouts.NorthEastFlower);
            table.PlaceButterfly(SeatNumberTwo, SampleLayouts.SouthEastFlower);

            return table;
        }

        private static MatchTable CreatePlayedThreeSeatOpening()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());
            table.PlaceInitialCaterpillar(SeatNumberOne, SampleLayouts.NorthWestFlower);
            table.PlaceInitialCaterpillar(SeatNumberTwo, SampleLayouts.NorthEastFlower);
            table.PlaceInitialCaterpillar(SeatNumberThree, SampleLayouts.SouthWestFlower);
            table.PlaceButterfly(SeatNumberThree, SampleLayouts.CenterFlower);

            return table;
        }

        private static MatchTable CreateFinishedTable(SeatStanding seatOneStanding, SeatStanding seatTwoStanding)
        {
            var snapshot = new MatchSnapshot(TablePhase.Finished, MatchRules.RoundCount, MatchTable.FirstTurnRoundNumber, TwoSeatCount, SeatNumberOne, 0, 0, SampleLayouts.FiveStartingFlowers, new List<Caterpillar>(), null, new SeatStanding[] { seatOneStanding, seatTwoStanding }, Array.Empty<SeatCards>(), Array.Empty<int>(), FinishCause.Completed);
            MatchTable table = MatchTable.RestoreFromSnapshot(snapshot, SampleRules.OneTurnPerRoundRules, SampleRules.CreateChanceSource());

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
