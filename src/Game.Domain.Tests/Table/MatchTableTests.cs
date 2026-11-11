using System;
using System.Collections.Generic;

using Xunit;

namespace Game.Domain.Tests
{
    public sealed class MatchTableTests
    {
        private const int SingleSeatCount = 1;
        private const int ThreeSeatCount = 3;
        private const int FiveSeatCount = 5;
        private const int SeatNumberOne = 1;
        private const int SeatNumberTwo = 2;
        private const int SeatNumberThree = 3;
        private const int SeatNumberFour = 4;
        private const int FiveFlowers = 5;
        private const int NoActionPoints = 0;
        private const int NoBuilds = 0;

        private static readonly Caterpillar _seatOneCaterpillar = new Caterpillar(SeatNumberOne, SampleLayouts.NorthWestFlower);
        private static readonly Caterpillar _seatTwoCaterpillar = new Caterpillar(SeatNumberTwo, SampleLayouts.NorthEastFlower);
        private static readonly Caterpillar _seatThreeCaterpillar = new Caterpillar(SeatNumberThree, SampleLayouts.CenterFlower);

        [Fact]
        public void Test_Constructor_WithThreeSeats_StartsInCaterpillarPlacement()
        {
            StartingLayout layout = SampleLayouts.FiveSingleCellPlanters;

            var table = new MatchTable(layout, ThreeSeatCount, SampleRules.ThreeSeatRules);

            Assert.Equal(TablePhase.CaterpillarPlacement, table.Phase);
        }

        [Fact]
        public void Test_Constructor_WithThreeSeats_GivesFirstPlacementToSeatOne()
        {
            StartingLayout layout = SampleLayouts.FiveSingleCellPlanters;

            var table = new MatchTable(layout, ThreeSeatCount, SampleRules.ThreeSeatRules);

            Assert.Equal(SeatNumberOne, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_Constructor_WithOneSeat_ThrowsArgumentOutOfRangeException()
        {
            StartingLayout layout = SampleLayouts.FiveSingleCellPlanters;

            Assert.Throws<ArgumentOutOfRangeException>(() => new MatchTable(layout, SingleSeatCount, SampleRules.ThreeSeatRules));
        }

        [Fact]
        public void Test_Constructor_WithFiveSeats_ThrowsArgumentOutOfRangeException()
        {
            StartingLayout layout = SampleLayouts.FiveSingleCellPlanters;

            Assert.Throws<ArgumentOutOfRangeException>(() => new MatchTable(layout, FiveSeatCount, SampleRules.ThreeSeatRules));
        }

        [Fact]
        public void Test_PlaceInitialCaterpillar_OnFreeFlowerBySeatOne_ReturnsPlaced()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, ThreeSeatCount, SampleRules.ThreeSeatRules);

            PlacementOutcome outcome = table.PlaceInitialCaterpillar(SeatNumberOne, SampleLayouts.NorthWestFlower);

            Assert.Equal(PlacementOutcome.Placed, outcome);
        }

        [Fact]
        public void Test_PlaceInitialCaterpillar_BySeatOne_PassesPlacementToSeatTwo()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, ThreeSeatCount, SampleRules.ThreeSeatRules);

            table.PlaceInitialCaterpillar(SeatNumberOne, SampleLayouts.NorthWestFlower);

            Assert.Equal(SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_PlaceInitialCaterpillar_BySeatTwoDuringSeatOnePlacement_ReturnsNotYourPlacement()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, ThreeSeatCount, SampleRules.ThreeSeatRules);

            PlacementOutcome outcome = table.PlaceInitialCaterpillar(SeatNumberTwo, SampleLayouts.NorthWestFlower);

            Assert.Equal(PlacementOutcome.NotYourPlacement, outcome);
        }

        [Fact]
        public void Test_PlaceInitialCaterpillar_OnGround_ReturnsNotAFlower()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, ThreeSeatCount, SampleRules.ThreeSeatRules);

            PlacementOutcome outcome = table.PlaceInitialCaterpillar(SeatNumberOne, SampleLayouts.GroundCell);

            Assert.Equal(PlacementOutcome.NotAFlower, outcome);
        }

        [Fact]
        public void Test_PlaceInitialCaterpillar_OnOccupiedFlower_ReturnsFlowerOccupied()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.CaterpillarPlacement, SeatNumberTwo, new Caterpillar[] { _seatOneCaterpillar }), SampleRules.ThreeSeatRules);

            PlacementOutcome outcome = table.PlaceInitialCaterpillar(SeatNumberTwo, SampleLayouts.NorthWestFlower);

            Assert.Equal(PlacementOutcome.FlowerOccupied, outcome);
        }

        [Fact]
        public void Test_PlaceInitialCaterpillar_OutsideBoard_ThrowsArgumentOutOfRangeException()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, ThreeSeatCount, SampleRules.ThreeSeatRules);

            Assert.Throws<ArgumentOutOfRangeException>(() => table.PlaceInitialCaterpillar(SeatNumberOne, SampleLayouts.OutsideCell));
        }

        [Fact]
        public void Test_PlaceInitialCaterpillar_WithSeatFourInAThreeSeatMatch_ThrowsArgumentOutOfRangeException()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, ThreeSeatCount, SampleRules.ThreeSeatRules);

            Assert.Throws<ArgumentOutOfRangeException>(() => table.PlaceInitialCaterpillar(SeatNumberFour, SampleLayouts.NorthWestFlower));
        }

        [Fact]
        public void Test_PlaceInitialCaterpillar_ByLastSeat_GivesButterflyToLastSeat()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.CaterpillarPlacement, SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar }), SampleRules.ThreeSeatRules);

            table.PlaceInitialCaterpillar(SeatNumberThree, SampleLayouts.CenterFlower);

            Assert.Equal(TablePhase.ButterflyPlacement, table.Phase);
            Assert.Equal(SeatNumberThree, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_PlaceInitialCaterpillar_DuringButterflyPlacement_ReturnsWrongPhase()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.ButterflyPlacement, SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar, _seatThreeCaterpillar }), SampleRules.ThreeSeatRules);

            PlacementOutcome outcome = table.PlaceInitialCaterpillar(SeatNumberThree, SampleLayouts.SouthEastFlower);

            Assert.Equal(PlacementOutcome.WrongPhase, outcome);
        }

        [Fact]
        public void Test_ExpirePlacement_DuringCaterpillarPlacement_PassesToNextSeat()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, ThreeSeatCount, SampleRules.ThreeSeatRules);

            table.ExpirePlacement();

            Assert.Equal(SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_ExpirePlacement_OfLastSeatWithoutCaterpillar_GivesButterflyToLastPlacer()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.CaterpillarPlacement, SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar }), SampleRules.ThreeSeatRules);

            table.ExpirePlacement();

            Assert.Equal(TablePhase.ButterflyPlacement, table.Phase);
            Assert.Equal(SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_ExpirePlacement_OfButterflyPlacer_GivesButterflyToPreviousPlacer()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.ButterflyPlacement, SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar, _seatThreeCaterpillar }), SampleRules.ThreeSeatRules);

            table.ExpirePlacement();

            Assert.Equal(SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_ExpirePlacement_OfLastButterflyCandidate_StartsFirstTurnWithoutButterfly()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.ButterflyPlacement, SeatNumberOne, new Caterpillar[] { _seatOneCaterpillar }), SampleRules.ThreeSeatRules);

            table.ExpirePlacement();

            Assert.Equal(TablePhase.PlayerTurn, table.Phase);
            Assert.Null(table.ButterflyCell);
        }

        [Fact]
        public void Test_ExpirePlacement_WhenNobodyPlacedACaterpillar_StartsFirstTurn()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.CaterpillarPlacement, SeatNumberThree, Array.Empty<Caterpillar>()), SampleRules.ThreeSeatRules);

            table.ExpirePlacement();

            Assert.Equal(TablePhase.PlayerTurn, table.Phase);
        }

        [Fact]
        public void Test_ExpirePlacement_DuringPlayerTurn_ThrowsInvalidOperationException()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.PlayerTurn, SeatNumberOne, new Caterpillar[] { _seatOneCaterpillar }), SampleRules.ThreeSeatRules);

            Assert.Throws<InvalidOperationException>(() => table.ExpirePlacement());
        }

        [Fact]
        public void Test_PlaceButterfly_OnFlowerWithCaterpillar_ReturnsPlaced()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.ButterflyPlacement, SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar, _seatThreeCaterpillar }), SampleRules.ThreeSeatRules);

            PlacementOutcome outcome = table.PlaceButterfly(SeatNumberThree, SampleLayouts.NorthWestFlower);

            Assert.Equal(PlacementOutcome.Placed, outcome);
        }

        [Fact]
        public void Test_PlaceButterfly_OnGround_ReturnsNotAFlower()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.ButterflyPlacement, SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar, _seatThreeCaterpillar }), SampleRules.ThreeSeatRules);

            PlacementOutcome outcome = table.PlaceButterfly(SeatNumberThree, SampleLayouts.GroundCell);

            Assert.Equal(PlacementOutcome.NotAFlower, outcome);
        }

        [Fact]
        public void Test_PlaceButterfly_ByPlacer_StartsFirstTurnWithSeatOne()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.ButterflyPlacement, SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar, _seatThreeCaterpillar }), SampleRules.ThreeSeatRules);

            table.PlaceButterfly(SeatNumberThree, SampleLayouts.SouthEastFlower);

            Assert.Equal(TablePhase.PlayerTurn, table.Phase);
            Assert.Equal(SeatNumberOne, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_PlaceButterfly_ByAnotherSeat_ReturnsNotYourPlacement()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.ButterflyPlacement, SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar, _seatThreeCaterpillar }), SampleRules.ThreeSeatRules);

            PlacementOutcome outcome = table.PlaceButterfly(SeatNumberOne, SampleLayouts.SouthEastFlower);

            Assert.Equal(PlacementOutcome.NotYourPlacement, outcome);
        }

        [Fact]
        public void Test_PlaceButterfly_DuringCaterpillarPlacement_ReturnsWrongPhase()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, ThreeSeatCount, SampleRules.ThreeSeatRules);

            PlacementOutcome outcome = table.PlaceButterfly(SeatNumberOne, SampleLayouts.NorthWestFlower);

            Assert.Equal(PlacementOutcome.WrongPhase, outcome);
        }

        [Fact]
        public void Test_FindPlacementTargets_WithOneOccupiedFlower_LeavesOutTheOccupiedFlower()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.CaterpillarPlacement, SeatNumberTwo, new Caterpillar[] { _seatOneCaterpillar }), SampleRules.ThreeSeatRules);
            var expectedTargets = new BoardCell[] { SampleLayouts.NorthEastFlower, SampleLayouts.CenterFlower, SampleLayouts.SouthWestFlower, SampleLayouts.SouthEastFlower };

            IReadOnlyList<BoardCell> placementTargets = table.FindPlacementTargets();

            Assert.Equal(expectedTargets, placementTargets);
        }

        [Fact]
        public void Test_FindPlacementTargets_DuringButterflyPlacement_IncludesOccupiedFlowers()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.ButterflyPlacement, SeatNumberThree, new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar, _seatThreeCaterpillar }), SampleRules.ThreeSeatRules);

            IReadOnlyList<BoardCell> placementTargets = table.FindPlacementTargets();

            Assert.Equal(FiveFlowers, placementTargets.Count);
        }

        [Fact]
        public void Test_FindPlacementTargets_DuringPlayerTurnWithoutActionPoints_ReturnsNoTargets()
        {
            MatchTable table = MatchTable.RestoreFromSnapshot(CreateThreeSeatSnapshot(TablePhase.PlayerTurn, SeatNumberOne, new Caterpillar[] { _seatOneCaterpillar }), SampleRules.ThreeSeatRules);

            IReadOnlyList<BoardCell> placementTargets = table.FindPlacementTargets();

            Assert.Empty(placementTargets);
        }

        [Fact]
        public void Test_TakeSnapshot_OfNewTable_KeepsTheStartingFlowers()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, ThreeSeatCount, SampleRules.ThreeSeatRules);

            MatchSnapshot snapshot = table.TakeSnapshot();

            Assert.Equal(SampleLayouts.FiveStartingFlowers, snapshot.Flowers);
        }

        [Fact]
        public void Test_Restore_WithPlacedCaterpillars_RestoresThem()
        {
            var placedCaterpillars = new Caterpillar[] { _seatOneCaterpillar, _seatTwoCaterpillar };
            MatchSnapshot snapshot = CreateThreeSeatSnapshot(TablePhase.CaterpillarPlacement, SeatNumberThree, placedCaterpillars);

            MatchTable table = MatchTable.RestoreFromSnapshot(snapshot, SampleRules.ThreeSeatRules);

            Assert.Equal(placedCaterpillars, table.Caterpillars);
        }

        [Fact]
        public void Test_Restore_WithActiveSeatBeyondSeatCount_ThrowsArgumentOutOfRangeException()
        {
            MatchSnapshot snapshot = CreateThreeSeatSnapshot(TablePhase.CaterpillarPlacement, SeatNumberFour, Array.Empty<Caterpillar>());

            Assert.Throws<ArgumentOutOfRangeException>(() => MatchTable.RestoreFromSnapshot(snapshot, SampleRules.ThreeSeatRules));
        }

        private static MatchSnapshot CreateThreeSeatSnapshot(TablePhase phase, int activeSeatNumber, IReadOnlyList<Caterpillar> caterpillars)
        {
            var snapshot = new MatchSnapshot(phase, MatchTable.FirstRoundNumber, MatchTable.FirstTurnNumber, ThreeSeatCount, activeSeatNumber, NoActionPoints, NoBuilds, SampleLayouts.FiveStartingFlowers, caterpillars, null);

            return snapshot;
        }
    }
}
