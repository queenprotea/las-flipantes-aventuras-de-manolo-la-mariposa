using System;
using System.Collections.Generic;

using Xunit;

namespace Game.Domain.Tests
{
    public sealed class MatchTableTests
    {
        private const int SingleSeatCount = 1;
        private const int FiveSeatCount = 5;
        private const int SeatNumberFour = 4;
        private const int NoBuilds = 0;

        private static readonly Caterpillar _seatOneCaterpillar = new Caterpillar(SampleRules.SeatNumberOne, SampleLayouts.NorthWestFlower);
        private static readonly Caterpillar _seatTwoCaterpillar = new Caterpillar(SampleRules.SeatNumberTwo, SampleLayouts.NorthEastFlower);
        private static readonly Caterpillar _seatThreeCaterpillar = new Caterpillar(SampleRules.SeatNumberThree, SampleLayouts.CenterFlower);

        [Fact]
        public void Test_Constructor_WithThreeSeats_StartsInCaterpillarPlacement()
        {
            StartingLayout layout = SampleLayouts.FiveSingleCellPlanters;

            var table = new MatchTable(layout, SampleRules.ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            Assert.Equal(TablePhase.CaterpillarPlacement, table.Phase);
        }

        [Fact]
        public void Test_Constructor_WithThreeSeats_GivesFirstPlacementToSeatOne()
        {
            StartingLayout layout = SampleLayouts.FiveSingleCellPlanters;

            var table = new MatchTable(layout, SampleRules.ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            Assert.Equal(SampleRules.SeatNumberOne, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_PlaceInitialCaterpillar_OnFreeFlowerBySeatOne_ReturnsPlaced()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, SampleRules.ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            PlacementOutcome outcome = table.PlaceInitialCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.NorthWestFlower);

            Assert.Equal(PlacementOutcome.Placed, outcome);
        }

        [Fact]
        public void Test_PlaceInitialCaterpillar_BySeatOne_PassesPlacementToSeatTwo()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, SampleRules.ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            table.PlaceInitialCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.NorthWestFlower);

            Assert.Equal(SampleRules.SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_PlaceInitialCaterpillar_BySeatTwoDuringSeatOnePlacement_ReturnsNotYourPlacement()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, SampleRules.ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            PlacementOutcome outcome = table.PlaceInitialCaterpillar(SampleRules.SeatNumberTwo, SampleLayouts.NorthWestFlower);

            Assert.Equal(PlacementOutcome.NotYourPlacement, outcome);
        }

        [Fact]
        public void Test_PlaceInitialCaterpillar_OnGround_ReturnsNotAFlower()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, SampleRules.ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            PlacementOutcome outcome = table.PlaceInitialCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.GroundCell);

            Assert.Equal(PlacementOutcome.NotAFlower, outcome);
        }

        [Fact]
        public void Test_PlaceInitialCaterpillar_OnOccupiedFlower_ReturnsFlowerOccupied()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.CaterpillarPlacement, SampleRules.SeatNumberTwo, new Caterpillar[] { _seatOneCaterpillar }), SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            PlacementOutcome outcome = table.PlaceInitialCaterpillar(SampleRules.SeatNumberTwo, SampleLayouts.NorthWestFlower);

            Assert.Equal(PlacementOutcome.FlowerOccupied, outcome);
        }

        [Fact]
        public void Test_PlaceInitialCaterpillar_ByLastSeat_GivesButterflyToLastSeat()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.CaterpillarPlacement, SampleRules.SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar }), SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            table.PlaceInitialCaterpillar(SampleRules.SeatNumberThree, SampleLayouts.CenterFlower);

            Assert.Equal(TablePhase.ButterflyPlacement, table.Phase);
            Assert.Equal(SampleRules.SeatNumberThree, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_PlaceInitialCaterpillar_DuringButterflyPlacement_ReturnsWrongPhase()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.ButterflyPlacement, SampleRules.SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar, _seatThreeCaterpillar }), SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            PlacementOutcome outcome = table.PlaceInitialCaterpillar(SampleRules.SeatNumberThree, SampleLayouts.SouthEastFlower);

            Assert.Equal(PlacementOutcome.WrongPhase, outcome);
        }

        [Fact]
        public void Test_ExpirePlacement_DuringCaterpillarPlacement_PassesToNextSeat()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, SampleRules.ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            table.ExpirePlacement();

            Assert.Equal(SampleRules.SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_ExpirePlacement_OfLastSeatWithoutCaterpillar_GivesButterflyToLastPlacer()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.CaterpillarPlacement, SampleRules.SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar }), SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            table.ExpirePlacement();

            Assert.Equal(TablePhase.ButterflyPlacement, table.Phase);
            Assert.Equal(SampleRules.SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_ExpirePlacement_OfButterflyPlacer_GivesButterflyToPreviousPlacer()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.ButterflyPlacement, SampleRules.SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar, _seatThreeCaterpillar }), SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            table.ExpirePlacement();

            Assert.Equal(SampleRules.SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_ExpirePlacement_OfLastButterflyCandidate_StartsFirstTurnWithoutButterfly()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.ButterflyPlacement, SampleRules.SeatNumberOne, new Caterpillar[] { _seatOneCaterpillar }), SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            table.ExpirePlacement();

            Assert.Equal(TablePhase.PlayerTurn, table.Phase);
            Assert.Null(table.ButterflyCell);
        }

        [Fact]
        public void Test_ExpirePlacement_WhenNobodyPlacedACaterpillar_StartsFirstTurn()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.CaterpillarPlacement, SampleRules.SeatNumberThree, Array.Empty<Caterpillar>()), SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            table.ExpirePlacement();

            Assert.Equal(TablePhase.PlayerTurn, table.Phase);
        }

        [Fact]
        public void Test_ExpirePlacement_DuringPlayerTurn_ThrowsInvalidOperationException()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.PlayerTurn, SampleRules.SeatNumberOne, new Caterpillar[] { _seatOneCaterpillar }), SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            Assert.Throws<InvalidOperationException>(() => table.ExpirePlacement());
        }

        [Fact]
        public void Test_PlaceButterfly_OnFlowerWithCaterpillar_ReturnsPlaced()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.ButterflyPlacement, SampleRules.SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar, _seatThreeCaterpillar }), SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            PlacementOutcome outcome = table.PlaceButterfly(SampleRules.SeatNumberThree, SampleLayouts.NorthWestFlower);

            Assert.Equal(PlacementOutcome.Placed, outcome);
        }

        [Fact]
        public void Test_PlaceButterfly_OnGround_ReturnsNotAFlower()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.ButterflyPlacement, SampleRules.SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar, _seatThreeCaterpillar }), SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            PlacementOutcome outcome = table.PlaceButterfly(SampleRules.SeatNumberThree, SampleLayouts.GroundCell);

            Assert.Equal(PlacementOutcome.NotAFlower, outcome);
        }

        [Fact]
        public void Test_PlaceButterfly_ByPlacer_StartsFirstTurnWithSeatOne()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.ButterflyPlacement, SampleRules.SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar, _seatThreeCaterpillar }), SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            table.PlaceButterfly(SampleRules.SeatNumberThree, SampleLayouts.SouthEastFlower);

            Assert.Equal(TablePhase.PlayerTurn, table.Phase);
            Assert.Equal(SampleRules.SeatNumberOne, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_PlaceButterfly_ByAnotherSeat_ReturnsNotYourPlacement()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.ButterflyPlacement, SampleRules.SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar, _seatThreeCaterpillar }), SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            PlacementOutcome outcome = table.PlaceButterfly(SampleRules.SeatNumberOne, SampleLayouts.SouthEastFlower);

            Assert.Equal(PlacementOutcome.NotYourPlacement, outcome);
        }

        [Fact]
        public void Test_PlaceButterfly_DuringCaterpillarPlacement_ReturnsWrongPhase()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, SampleRules.ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            PlacementOutcome outcome = table.PlaceButterfly(SampleRules.SeatNumberOne, SampleLayouts.NorthWestFlower);

            Assert.Equal(PlacementOutcome.WrongPhase, outcome);
        }

        [Fact]
        public void Test_FindPlacementTargets_WithOneOccupiedFlower_LeavesOutTheOccupiedFlower()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.CaterpillarPlacement, SampleRules.SeatNumberTwo, new Caterpillar[] { _seatOneCaterpillar }), SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());
            var expectedTargets = new BoardCell[] { SampleLayouts.NorthEastFlower, SampleLayouts.CenterFlower, SampleLayouts.SouthWestFlower, SampleLayouts.SouthEastFlower };

            IReadOnlyList<BoardCell> placementTargets = table.FindPlacementTargets();

            Assert.Equal(expectedTargets, placementTargets);
        }

        [Fact]
        public void Test_FindPlacementTargets_DuringButterflyPlacement_IncludesOccupiedFlowers()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.ButterflyPlacement, SampleRules.SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar, _seatThreeCaterpillar }), SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            IReadOnlyList<BoardCell> placementTargets = table.FindPlacementTargets();

            Assert.Equal(SampleLayouts.FiveFlowers, placementTargets.Count);
        }

        [Fact]
        public void Test_FindPlacementTargets_DuringPlayerTurnWithoutActionPoints_ReturnsNoTargets()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.PlayerTurn, SampleRules.SeatNumberOne, new Caterpillar[] { _seatOneCaterpillar }), SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            IReadOnlyList<BoardCell> placementTargets = table.FindPlacementTargets();

            Assert.Empty(placementTargets);
        }

        [Fact]
        public void Test_TakeSnapshot_OfNewTable_KeepsTheStartingFlowers()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, SampleRules.ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            MatchSnapshot snapshot = table.TakeSnapshot();

            Assert.Equal(SampleLayouts.FiveStartingFlowers, snapshot.Flowers);
        }

        [Fact]
        public void Test_Restore_WithPlacedCaterpillars_RestoresThem()
        {
            var placedCaterpillars = new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar };
            MatchSnapshot snapshot = CreateThreeSeatSnapshot(TablePhase.CaterpillarPlacement, SampleRules.SeatNumberThree, placedCaterpillars);

            MatchTable table = MatchTable.RestoreFromSnapshot(snapshot, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());

            Assert.Equal(placedCaterpillars, table.Caterpillars);
        }

        [Fact]
        public void Test_Restore_WithActiveSeatBeyondSeatCount_ThrowsArgumentOutOfRangeException()
        {
            MatchSnapshot snapshot = CreateThreeSeatSnapshot(TablePhase.CaterpillarPlacement, SeatNumberFour, Array.Empty<Caterpillar>());

            Assert.Throws<ArgumentOutOfRangeException>(() => MatchTable.RestoreFromSnapshot(snapshot, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource()));
        }

        [Fact]
        public void Test_Restore_WithCaterpillarOutsideBoard_ThrowsArgumentOutOfRangeException()
        {
            var outsideCaterpillars = new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, SampleLayouts.OutsideCell) };
            MatchSnapshot snapshot = CreateThreeSeatSnapshot(TablePhase.CaterpillarPlacement, SampleRules.SeatNumberOne, outsideCaterpillars);

            Assert.Throws<ArgumentOutOfRangeException>(() => MatchTable.RestoreFromSnapshot(snapshot, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource()));
        }

        [Fact]
        public void Test_Restore_WithTurnBeyondTheRound_ThrowsArgumentOutOfRangeException()
        {
            int turnBeyondTheRound = SampleRules.ThreeSeatRules.GetTurnCount(MatchTable.FirstRoundNumber) + 1;
            var snapshot = new MatchSnapshot(TablePhase.PlayerTurn, MatchTable.FirstRoundNumber, turnBeyondTheRound, SampleRules.ThreeSeatCount, SampleRules.SeatNumberOne, SampleRules.NoActionPoints, NoBuilds, SampleLayouts.FiveStartingFlowers, Array.Empty<Caterpillar>(), null, Array.Empty<SeatStanding>(), Array.Empty<SeatCards>(), Array.Empty<int>(), null);

            Assert.Throws<ArgumentOutOfRangeException>(() => MatchTable.RestoreFromSnapshot(snapshot, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource()));
        }

        private static MatchSnapshot CreateThreeSeatSnapshot(TablePhase phase, int activeSeatNumber, IReadOnlyList<Caterpillar> caterpillars)
        {
            var snapshot = new MatchSnapshot(phase, MatchTable.FirstRoundNumber, MatchTable.FirstTurnRoundNumber, SampleRules.ThreeSeatCount, activeSeatNumber, SampleRules.NoActionPoints, NoBuilds, SampleLayouts.FiveStartingFlowers, caterpillars, null, Array.Empty<SeatStanding>(), Array.Empty<SeatCards>(), Array.Empty<int>(), null);

            return snapshot;
        }
    }
}
