using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Game.Domain.Tests
{
    public sealed class MatchTableTurnTests
    {
        private const int TurnRoundNumberTwo = 2;
        private const int TurnRoundNumberThree = 3;
        private const int OneActionPoint = 1;
        private const int TwoPlanterCells = 2;
        private const int FourNeighborCells = 4;
        private const int PointsLeftAfterPlacement = MatchTable.ActionPointsPerTurn - SampleRules.PlacementCost;
        private const int PointsLeftAfterMovement = MatchTable.ActionPointsPerTurn - SampleRules.MovementCost;
        private const int PointsLeftAfterClimb = MatchTable.ActionPointsPerTurn - SampleRules.MovementCost - SampleRules.ClimbCost;
        private const int PointsLeftAfterBuilding = MatchTable.ActionPointsPerTurn - SampleRules.BuildingCost;
        private const int CaterpillarsLeftAfterTheOpening = MatchTable.CaterpillarsPerSeat - 1;

        private static readonly BoardCell _northOfNorthWest = new BoardCell(0, 1);
        private static readonly BoardCell _southOfNorthWest = new BoardCell(2, 1);
        private static readonly BoardCell _eastOfNorthWest = new BoardCell(1, 2);
        private static readonly BoardCell _diagonalOfNorthWest = new BoardCell(2, 2);
        private static readonly BoardCell _farGroundCell = new BoardCell(4, 6);
        private static readonly BoardCell _tallFlowerCell = new BoardCell(1, 3);
        private static readonly BoardCell _southOfTallFlower = new BoardCell(2, 3);
        private static readonly BoardCell _eastOfFarGround = new BoardCell(4, 7);
        private static readonly BoardCell _westOfNorthEast = new BoardCell(1, 5);
        private static readonly BoardCell[] _fiveGroundCells = new BoardCell[] { new BoardCell(0, 0), new BoardCell(0, 2), new BoardCell(0, 4), new BoardCell(5, 5), new BoardCell(7, 7) };
        private static readonly Caterpillar _seatOneOnNorthWest = new Caterpillar(SampleRules.SeatNumberOne, SampleLayouts.NorthWestFlower);

        [Fact]
        public void Test_PlaceButterfly_ByLastPlacer_GivesFiveActionPointsToSeatOne()
        {
            MatchTable table = CreatePlayedOpening();

            Assert.Equal(MatchTable.ActionPointsPerTurn, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_PlaceButterfly_ByLastPlacer_GivesTheBuildsOfTheFirstTurn()
        {
            MatchTable table = CreatePlayedOpening();

            Assert.Equal(SampleRules.ThreeBuildings, table.BuildsLeft);
        }

        [Fact]
        public void Test_PlaceCaterpillar_OnFreeFlower_ReturnsApplied()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);

            Assert.Equal(TurnActionOutcome.Applied, outcome);
        }

        [Fact]
        public void Test_PlaceCaterpillar_OnFreeFlower_ChargesThePlacementCost()
        {
            MatchTable table = CreatePlayedOpening();

            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);

            Assert.Equal(PointsLeftAfterPlacement, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_PlaceCaterpillar_OnOccupiedFlower_ReturnsCellOccupied()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.NorthEastFlower);

            Assert.Equal(TurnActionOutcome.CellOccupied, outcome);
        }

        [Fact]
        public void Test_PlaceCaterpillar_OnGround_ReturnsNotAFlower()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.GroundCell);

            Assert.Equal(TurnActionOutcome.NotAFlower, outcome);
        }

        [Fact]
        public void Test_PlaceCaterpillar_OutOfTurn_ReturnsNotYourTurn()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.PlaceCaterpillar(SampleRules.SeatNumberTwo, SampleLayouts.CenterFlower);

            Assert.Equal(TurnActionOutcome.NotYourTurn, outcome);
        }

        [Fact]
        public void Test_PlaceCaterpillar_DuringCaterpillarPlacement_ReturnsWrongPhase()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, SampleRules.TwoSeatCount, SampleRules.TwoSeatRules, SampleRules.CreateChanceSource());

            TurnActionOutcome outcome = table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);

            Assert.Equal(TurnActionOutcome.WrongPhase, outcome);
        }

        [Fact]
        public void Test_PlaceCaterpillar_AfterSpendingFourPoints_ReturnsNotEnoughActionPoints()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);
            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.SouthWestFlower);

            TurnActionOutcome outcome = table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.SouthEastFlower);

            Assert.Equal(TurnActionOutcome.NotEnoughActionPoints, outcome);
        }

        [Fact]
        public void Test_PlaceCaterpillar_WithFiveCaterpillarsOnBoard_ReturnsNoCaterpillarsLeft()
        {
            List<Caterpillar> fiveCaterpillars = _fiveGroundCells.Select(CreateSeatOneCaterpillar).ToList();
            MatchTable table = CreateTurnTable(SampleLayouts.FiveStartingFlowers, fiveCaterpillars);

            TurnActionOutcome outcome = table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);

            Assert.Equal(TurnActionOutcome.NoCaterpillarsLeft, outcome);
        }

        [Fact]
        public void Test_CountCaterpillarsLeft_AfterTheOpening_ReturnsFour()
        {
            MatchTable table = CreatePlayedOpening();

            int caterpillarsLeft = table.CountCaterpillarsLeft(SampleRules.SeatNumberOne);

            Assert.Equal(CaterpillarsLeftAfterTheOpening, caterpillarsLeft);
        }

        [Fact]
        public void Test_MoveCaterpillar_FromFlowerToGround_ReturnsApplied()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.MoveCaterpillar(SampleRules.SeatNumberOne, new CaterpillarMove(SampleLayouts.NorthWestFlower, _southOfNorthWest));

            Assert.Equal(TurnActionOutcome.Applied, outcome);
        }

        [Fact]
        public void Test_MoveCaterpillar_FromFlowerToGround_ChargesOnlyTheMovement()
        {
            MatchTable table = CreatePlayedOpening();

            table.MoveCaterpillar(SampleRules.SeatNumberOne, new CaterpillarMove(SampleLayouts.NorthWestFlower, _southOfNorthWest));

            Assert.Equal(PointsLeftAfterMovement, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_MoveCaterpillar_FromFlowerToGround_LeavesTheCaterpillarOnTheDestination()
        {
            MatchTable table = CreatePlayedOpening();

            table.MoveCaterpillar(SampleRules.SeatNumberOne, new CaterpillarMove(SampleLayouts.NorthWestFlower, _southOfNorthWest));

            Assert.Contains(new Caterpillar(SampleRules.SeatNumberOne, _southOfNorthWest), table.Caterpillars);
        }

        [Fact]
        public void Test_MoveCaterpillar_FromGroundToLevelOneFlower_ChargesMovementAndOneClimb()
        {
            MatchTable table = CreateTurnTable(SampleLayouts.FiveStartingFlowers, new Caterpillar[] { CreateSeatOneCaterpillar(_southOfNorthWest) });

            table.MoveCaterpillar(SampleRules.SeatNumberOne, new CaterpillarMove(_southOfNorthWest, SampleLayouts.NorthWestFlower));

            Assert.Equal(PointsLeftAfterClimb, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_MoveCaterpillar_FromGroundToLevelTwoFlower_ReturnsClimbTooHigh()
        {
            var flowers = new Flower[] { new Flower(SampleLayouts.NorthWestFlower, SampleLayouts.LevelTwo) };
            MatchTable table = CreateTurnTable(flowers, new Caterpillar[] { CreateSeatOneCaterpillar(_southOfNorthWest) });

            TurnActionOutcome outcome = table.MoveCaterpillar(SampleRules.SeatNumberOne, new CaterpillarMove(_southOfNorthWest, SampleLayouts.NorthWestFlower));

            Assert.Equal(TurnActionOutcome.ClimbTooHigh, outcome);
        }

        [Fact]
        public void Test_MoveCaterpillar_FromLevelThreeFlowerToGround_ReturnsApplied()
        {
            var flowers = new Flower[] { new Flower(_tallFlowerCell, SampleLayouts.LevelThree) };
            MatchTable table = CreateTurnTable(flowers, new Caterpillar[] { CreateSeatOneCaterpillar(_tallFlowerCell) });

            TurnActionOutcome outcome = table.MoveCaterpillar(SampleRules.SeatNumberOne, new CaterpillarMove(_tallFlowerCell, _eastOfNorthWest));

            Assert.Equal(TurnActionOutcome.Applied, outcome);
        }

        [Fact]
        public void Test_MoveCaterpillar_FromLevelThreeFlowerToGround_ChargesOnlyTheMovement()
        {
            var flowers = new Flower[] { new Flower(_tallFlowerCell, SampleLayouts.LevelThree) };
            MatchTable table = CreateTurnTable(flowers, new Caterpillar[] { CreateSeatOneCaterpillar(_tallFlowerCell) });

            table.MoveCaterpillar(SampleRules.SeatNumberOne, new CaterpillarMove(_tallFlowerCell, _eastOfNorthWest));

            Assert.Equal(PointsLeftAfterMovement, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_MoveCaterpillar_ToDiagonalCell_ReturnsNotNeighborCell()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.MoveCaterpillar(SampleRules.SeatNumberOne, new CaterpillarMove(SampleLayouts.NorthWestFlower, _diagonalOfNorthWest));

            Assert.Equal(TurnActionOutcome.NotNeighborCell, outcome);
        }

        [Fact]
        public void Test_MoveCaterpillar_OfAnotherSeat_ReturnsNotYourCaterpillar()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.MoveCaterpillar(SampleRules.SeatNumberOne, new CaterpillarMove(SampleLayouts.NorthEastFlower, _westOfNorthEast));

            Assert.Equal(TurnActionOutcome.NotYourCaterpillar, outcome);
        }

        [Fact]
        public void Test_MoveCaterpillar_OntoAnotherCaterpillar_ReturnsCellOccupied()
        {
            MatchTable table = CreateTurnTable(SampleLayouts.FiveStartingFlowers, new Caterpillar[] { _seatOneOnNorthWest, new Caterpillar(SampleRules.SeatNumberTwo, _southOfNorthWest) });

            TurnActionOutcome outcome = table.MoveCaterpillar(SampleRules.SeatNumberOne, new CaterpillarMove(SampleLayouts.NorthWestFlower, _southOfNorthWest));

            Assert.Equal(TurnActionOutcome.CellOccupied, outcome);
        }

        [Fact]
        public void Test_MoveCaterpillar_WithOneActionPointUphill_KeepsTheActionPoint()
        {
            MatchTable table = CreateTurnTable(SampleLayouts.FiveStartingFlowers, new Caterpillar[] { CreateSeatOneCaterpillar(_southOfNorthWest), CreateSeatOneCaterpillar(_farGroundCell) });
            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);
            table.MoveCaterpillar(SampleRules.SeatNumberOne, new CaterpillarMove(_farGroundCell, _eastOfFarGround));
            table.MoveCaterpillar(SampleRules.SeatNumberOne, new CaterpillarMove(_eastOfFarGround, _farGroundCell));

            table.MoveCaterpillar(SampleRules.SeatNumberOne, new CaterpillarMove(_southOfNorthWest, SampleLayouts.NorthWestFlower));

            Assert.Equal(OneActionPoint, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_MoveCaterpillar_WithOneActionPointUphill_ReturnsNotEnoughActionPoints()
        {
            MatchTable table = CreateTurnTable(SampleLayouts.FiveStartingFlowers, new Caterpillar[] { CreateSeatOneCaterpillar(_southOfNorthWest), CreateSeatOneCaterpillar(_farGroundCell) });
            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);
            table.MoveCaterpillar(SampleRules.SeatNumberOne, new CaterpillarMove(_farGroundCell, _eastOfFarGround));
            table.MoveCaterpillar(SampleRules.SeatNumberOne, new CaterpillarMove(_eastOfFarGround, _farGroundCell));

            TurnActionOutcome outcome = table.MoveCaterpillar(SampleRules.SeatNumberOne, new CaterpillarMove(_southOfNorthWest, SampleLayouts.NorthWestFlower));

            Assert.Equal(TurnActionOutcome.NotEnoughActionPoints, outcome);
        }

        [Fact]
        public void Test_FindMoveTargets_FromStartingFlower_OffersTheFourGroundNeighbors()
        {
            MatchTable table = CreatePlayedOpening();

            IReadOnlyList<MoveTarget> moveTargets = table.FindMoveTargets(SampleLayouts.NorthWestFlower);

            Assert.Equal(FourNeighborCells, moveTargets.Count);
        }

        [Fact]
        public void Test_FindMoveTargets_FromStartingFlower_ChargesTheMovementCostForEach()
        {
            MatchTable table = CreatePlayedOpening();

            IReadOnlyList<MoveTarget> moveTargets = table.FindMoveTargets(SampleLayouts.NorthWestFlower);

            Assert.Equal(new int[] { SampleRules.MovementCost }, moveTargets.Select(moveTarget => moveTarget.ActionPointCost).Distinct());
        }

        [Fact]
        public void Test_FindMoveTargets_FromAnotherSeatCaterpillar_ReturnsNoTargets()
        {
            MatchTable table = CreatePlayedOpening();

            IReadOnlyList<MoveTarget> moveTargets = table.FindMoveTargets(SampleLayouts.NorthEastFlower);

            Assert.Empty(moveTargets);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnFreeFlower_ReturnsApplied()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.AddFlowerLevel(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);

            Assert.Equal(TurnActionOutcome.Applied, outcome);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnFreeFlower_RaisesItOneLevel()
        {
            MatchTable table = CreatePlayedOpening();

            table.AddFlowerLevel(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);

            IReadOnlyList<Flower> flowers = table.FindFlowers();
            Assert.Contains(new Flower(SampleLayouts.CenterFlower, SampleLayouts.LevelTwo), flowers);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnFreeFlower_SpendsOneBuild()
        {
            MatchTable table = CreatePlayedOpening();

            table.AddFlowerLevel(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);

            Assert.Equal(SampleRules.TwoBuildings, table.BuildsLeft);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnFreeFlower_ChargesTheBuildingCost()
        {
            MatchTable table = CreatePlayedOpening();

            table.AddFlowerLevel(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);

            Assert.Equal(PointsLeftAfterBuilding, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_AddFlowerLevel_UnderCaterpillar_ReturnsCellOccupied()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.AddFlowerLevel(SampleRules.SeatNumberOne, SampleLayouts.NorthWestFlower);

            Assert.Equal(TurnActionOutcome.CellOccupied, outcome);
        }

        [Fact]
        public void Test_AddFlowerLevel_UnderButterfly_ReturnsApplied()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.AddFlowerLevel(SampleRules.SeatNumberOne, SampleLayouts.SouthEastFlower);

            Assert.Equal(TurnActionOutcome.Applied, outcome);
        }

        [Fact]
        public void Test_AddFlowerLevel_UnderButterfly_KeepsTheButterflyOnTheFlower()
        {
            MatchTable table = CreatePlayedOpening();

            table.AddFlowerLevel(SampleRules.SeatNumberOne, SampleLayouts.SouthEastFlower);

            Assert.Equal(SampleLayouts.SouthEastFlower, table.ButterflyCell);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnLevelThreeFlower_ReturnsFlowerAtMaximumLevel()
        {
            var flowers = new Flower[] { new Flower(_tallFlowerCell, SampleLayouts.LevelThree) };
            MatchTable table = CreateTurnTable(flowers, Array.Empty<Caterpillar>());

            TurnActionOutcome outcome = table.AddFlowerLevel(SampleRules.SeatNumberOne, _tallFlowerCell);

            Assert.Equal(TurnActionOutcome.FlowerAtMaximumLevel, outcome);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnGroundNextToPlanter_ReturnsApplied()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.AddFlowerLevel(SampleRules.SeatNumberOne, _northOfNorthWest);

            Assert.Equal(TurnActionOutcome.Applied, outcome);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnGroundNextToPlanter_GrowsThatPlanter()
        {
            MatchTable table = CreatePlayedOpening();

            table.AddFlowerLevel(SampleRules.SeatNumberOne, _northOfNorthWest);

            IReadOnlyList<Planter> planters = table.FindPlanters();
            Assert.Contains(TwoPlanterCells, planters.Select(planter => planter.CellCount));
        }

        [Fact]
        public void Test_AddFlowerLevel_OnGroundAwayFromPlanters_ReturnsNotNextToPlanter()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.AddFlowerLevel(SampleRules.SeatNumberOne, _farGroundCell);

            Assert.Equal(TurnActionOutcome.NotNextToPlanter, outcome);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnGroundBetweenTwoPlanters_ReturnsJoinsPlanters()
        {
            var flowers = new Flower[] { new Flower(_southOfNorthWest, SampleLayouts.LevelOne), new Flower(_southOfTallFlower, SampleLayouts.LevelOne) };
            MatchTable table = CreateTurnTable(flowers, Array.Empty<Caterpillar>());

            TurnActionOutcome outcome = table.AddFlowerLevel(SampleRules.SeatNumberOne, _diagonalOfNorthWest);

            Assert.Equal(TurnActionOutcome.JoinsPlanters, outcome);
        }

        [Fact]
        public void Test_AddFlowerLevel_AfterThreeBuilds_ReturnsNoBuildsLeft()
        {
            MatchTable table = CreatePlayedOpening();
            table.AddFlowerLevel(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);
            table.AddFlowerLevel(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);
            table.AddFlowerLevel(SampleRules.SeatNumberOne, SampleLayouts.SouthWestFlower);

            TurnActionOutcome outcome = table.AddFlowerLevel(SampleRules.SeatNumberOne, SampleLayouts.SouthWestFlower);

            Assert.Equal(TurnActionOutcome.NoBuildsLeft, outcome);
        }

        [Fact]
        public void Test_FindBuildTargets_AfterTheOpening_LeavesOutTheCaterpillarFlowers()
        {
            MatchTable table = CreatePlayedOpening();

            IReadOnlyList<BoardCell> buildTargets = table.FindBuildTargets();

            Assert.DoesNotContain(SampleLayouts.NorthWestFlower, buildTargets);
        }

        [Fact]
        public void Test_FindBuildTargets_AfterTheOpening_OffersTheFreeFlowers()
        {
            MatchTable table = CreatePlayedOpening();

            IReadOnlyList<BoardCell> buildTargets = table.FindBuildTargets();

            Assert.Contains(SampleLayouts.CenterFlower, buildTargets);
        }

        [Fact]
        public void Test_FindBuildTargets_AfterTheOpening_OffersTheGroundNextToTheFlowers()
        {
            MatchTable table = CreatePlayedOpening();

            IReadOnlyList<BoardCell> buildTargets = table.FindBuildTargets();

            Assert.Contains(_northOfNorthWest, buildTargets);
        }

        [Fact]
        public void Test_EndTurn_BySeatOne_ReturnsApplied()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);

            TurnActionOutcome outcome = table.EndTurn(SampleRules.SeatNumberOne);

            Assert.Equal(TurnActionOutcome.Applied, outcome);
        }

        [Fact]
        public void Test_EndTurn_BySeatOne_GivesTheTurnToSeatTwo()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);

            table.EndTurn(SampleRules.SeatNumberOne);

            Assert.Equal(SampleRules.SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_EndTurn_BySeatOne_GivesTheWholeActionPoints()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);

            table.EndTurn(SampleRules.SeatNumberOne);

            Assert.Equal(MatchTable.ActionPointsPerTurn, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_EndTurn_ByLastSeat_StartsTheNextTurnRound()
        {
            MatchTable table = CreatePlayedOpening();
            table.EndTurn(SampleRules.SeatNumberOne);

            table.EndTurn(SampleRules.SeatNumberTwo);

            Assert.Equal(TurnRoundNumberTwo, table.TurnRoundNumber);
        }

        [Fact]
        public void Test_EndTurn_ByLastSeat_StartsTheNextTurnWithSeatOne()
        {
            MatchTable table = CreatePlayedOpening();
            table.EndTurn(SampleRules.SeatNumberOne);

            table.EndTurn(SampleRules.SeatNumberTwo);

            Assert.Equal(SampleRules.SeatNumberOne, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_EndTurn_OutOfTurn_ReturnsNotYourTurn()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.EndTurn(SampleRules.SeatNumberTwo);

            Assert.Equal(TurnActionOutcome.NotYourTurn, outcome);
        }

        [Fact]
        public void Test_ExpireTurn_OfSeatOne_GivesTheTurnToSeatTwo()
        {
            MatchTable table = CreatePlayedOpening();

            table.ExpireTurn();

            Assert.Equal(SampleRules.SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_ExpireTurn_DuringCaterpillarPlacement_ThrowsInvalidOperationException()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, SampleRules.TwoSeatCount, SampleRules.TwoSeatRules, SampleRules.CreateChanceSource());

            Assert.Throws<InvalidOperationException>(table.ExpireTurn);
        }

        [Fact]
        public void Test_ExpireTurn_AfterEveryTurnOfRoundOne_MovesToRoundTwo()
        {
            MatchTable table = CreatePlayedOpening();

            ExpireTurns(table, SampleRules.FourTurns * SampleRules.TwoSeatCount);

            Assert.Equal(SampleRules.RoundNumberTwo, table.RoundNumber);
        }

        [Fact]
        public void Test_ExpireTurn_AfterEveryTurnOfRoundOne_OpensTheButterflyPlacement()
        {
            MatchTable table = CreatePlayedOpening();

            ExpireTurns(table, SampleRules.FourTurns * SampleRules.TwoSeatCount);

            Assert.Equal(TablePhase.ButterflyPlacement, table.Phase);
        }

        [Fact]
        public void Test_ExpireTurn_InTurnThreeWithThreeSeats_StartsTheThirdTurnRound()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, SampleRules.ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());
            table.PlaceInitialCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.NorthWestFlower);
            table.PlaceInitialCaterpillar(SampleRules.SeatNumberTwo, SampleLayouts.NorthEastFlower);
            table.PlaceInitialCaterpillar(SampleRules.SeatNumberThree, SampleLayouts.SouthWestFlower);
            table.PlaceButterfly(SampleRules.SeatNumberThree, SampleLayouts.CenterFlower);

            ExpireTurns(table, TurnRoundNumberTwo * SampleRules.ThreeSeatCount);

            Assert.Equal(TurnRoundNumberThree, table.TurnRoundNumber);
        }

        [Fact]
        public void Test_ExpireTurn_InTurnThreeWithThreeSeats_HandsOutTwoBuilds()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, SampleRules.ThreeSeatCount, SampleRules.ThreeSeatRules, SampleRules.CreateChanceSource());
            table.PlaceInitialCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.NorthWestFlower);
            table.PlaceInitialCaterpillar(SampleRules.SeatNumberTwo, SampleLayouts.NorthEastFlower);
            table.PlaceInitialCaterpillar(SampleRules.SeatNumberThree, SampleLayouts.SouthWestFlower);
            table.PlaceButterfly(SampleRules.SeatNumberThree, SampleLayouts.CenterFlower);

            ExpireTurns(table, TurnRoundNumberTwo * SampleRules.ThreeSeatCount);

            Assert.Equal(SampleRules.TwoBuildings, table.BuildsLeft);
        }

        [Fact]
        public void Test_TakeSnapshot_AfterSpendingPoints_KeepsTheActionPointsLeft()
        {
            MatchTable table = CreatePlayedOpening();
            table.AddFlowerLevel(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);

            MatchTable restoredTable = MatchTable.RestoreFromSnapshot(table.TakeSnapshot(), SampleRules.TwoSeatRules, SampleRules.CreateChanceSource());

            Assert.Equal(table.ActionPointsLeft, restoredTable.ActionPointsLeft);
        }

        [Fact]
        public void Test_TakeSnapshot_AfterSpendingPoints_KeepsTheBuildsLeft()
        {
            MatchTable table = CreatePlayedOpening();
            table.AddFlowerLevel(SampleRules.SeatNumberOne, SampleLayouts.CenterFlower);

            MatchTable restoredTable = MatchTable.RestoreFromSnapshot(table.TakeSnapshot(), SampleRules.TwoSeatRules, SampleRules.CreateChanceSource());

            Assert.Equal(table.BuildsLeft, restoredTable.BuildsLeft);
        }

        private static MatchTable CreatePlayedOpening()
        {
            MatchTable table = CreatePlayedOpening(SampleRules.TwoSeatRules);

            return table;
        }

        private static MatchTable CreatePlayedOpening(MatchRules rules)
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, SampleRules.TwoSeatCount, rules, SampleRules.CreateChanceSource());
            table.PlaceInitialCaterpillar(SampleRules.SeatNumberOne, SampleLayouts.NorthWestFlower);
            table.PlaceInitialCaterpillar(SampleRules.SeatNumberTwo, SampleLayouts.NorthEastFlower);
            table.PlaceButterfly(SampleRules.SeatNumberTwo, SampleLayouts.SouthEastFlower);

            return table;
        }

        private static MatchTable CreateTurnTable(IReadOnlyList<Flower> flowers, IReadOnlyList<Caterpillar> caterpillars)
        {
            var snapshot = new MatchSnapshot(TablePhase.PlayerTurn, MatchTable.FirstRoundNumber, MatchTable.FirstTurnRoundNumber, SampleRules.TwoSeatCount, SampleRules.SeatNumberOne, MatchTable.ActionPointsPerTurn, SampleRules.ThreeBuildings, flowers, caterpillars, null, Array.Empty<SeatStanding>(), Array.Empty<SeatCards>(), Array.Empty<int>(), null);
            MatchTable table = MatchTable.RestoreFromSnapshot(snapshot, SampleRules.TwoSeatRules, SampleRules.CreateChanceSource());

            return table;
        }

        private static Caterpillar CreateSeatOneCaterpillar(BoardCell caterpillarCell)
        {
            var caterpillar = new Caterpillar(SampleRules.SeatNumberOne, caterpillarCell);

            return caterpillar;
        }

        private static void ExpireTurns(MatchTable table, int turnCount)
        {
            for (var i = 0; i < turnCount; i++)
            {
                table.ExpireTurn();
            }
        }
    }
}
