using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Game.Domain.Tests
{
    public sealed class MatchTableTurnTests
    {
        private const int TwoSeatCount = 2;
        private const int ThreeSeatCount = 3;
        private const int SeatNumberOne = 1;
        private const int SeatNumberTwo = 2;
        private const int SeatNumberThree = 3;
        private const int TurnNumberTwo = 2;
        private const int TurnNumberThree = 3;
        private const int RoundNumberTwo = 2;
        private const int FourTurns = 4;
        private const int LevelOne = 1;
        private const int LevelTwo = 2;
        private const int LevelThree = 3;
        private const int OneActionPoint = 1;
        private const int TwoPlanterCells = 2;
        private const int FourNeighborCells = 4;

        private static readonly BoardCell _northOfNorthWest = new BoardCell(0, 1);
        private static readonly BoardCell _southOfNorthWest = new BoardCell(2, 1);
        private static readonly BoardCell _eastOfNorthWest = new BoardCell(1, 2);
        private static readonly BoardCell _diagonalOfNorthWest = new BoardCell(2, 2);
        private static readonly BoardCell _farGroundCell = new BoardCell(4, 6);
        private static readonly BoardCell _tallFlowerCell = new BoardCell(1, 3);
        private static readonly Caterpillar _seatOneOnNorthWest = new Caterpillar(SeatNumberOne, SampleLayouts.NorthWestFlower);
        private static readonly Caterpillar _seatTwoOnNorthEast = new Caterpillar(SeatNumberTwo, SampleLayouts.NorthEastFlower);

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
        public void Test_PlaceCaterpillar_OnFreeFlower_ChargesThePlacementCost()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);

            Assert.Equal(TurnActionOutcome.Applied, outcome);
            Assert.Equal(MatchTable.ActionPointsPerTurn - SampleRules.PlacementCost, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_PlaceCaterpillar_OnOccupiedFlower_ReturnsCellOccupied()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.NorthEastFlower);

            Assert.Equal(TurnActionOutcome.CellOccupied, outcome);
        }

        [Fact]
        public void Test_PlaceCaterpillar_OnGround_ReturnsNotAFlower()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.GroundCell);

            Assert.Equal(TurnActionOutcome.NotAFlower, outcome);
        }

        [Fact]
        public void Test_PlaceCaterpillar_OutOfTurn_ReturnsNotYourTurn()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.PlaceCaterpillar(SeatNumberTwo, SampleLayouts.CenterFlower);

            Assert.Equal(TurnActionOutcome.NotYourTurn, outcome);
        }

        [Fact]
        public void Test_PlaceCaterpillar_DuringCaterpillarPlacement_ReturnsWrongPhase()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, TwoSeatCount, SampleRules.TwoSeatRules);

            TurnActionOutcome outcome = table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);

            Assert.Equal(TurnActionOutcome.WrongPhase, outcome);
        }

        [Fact]
        public void Test_PlaceCaterpillar_AfterSpendingFourPoints_ReturnsNotEnoughActionPoints()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);
            table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.SouthWestFlower);

            TurnActionOutcome outcome = table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.SouthEastFlower);

            Assert.Equal(TurnActionOutcome.NotEnoughActionPoints, outcome);
        }

        [Fact]
        public void Test_PlaceCaterpillar_WithFiveCaterpillarsOnBoard_ReturnsNoCaterpillarsLeft()
        {
            List<Caterpillar> fiveCaterpillars = new BoardCell[] { new BoardCell(0, 0), new BoardCell(0, 2), new BoardCell(0, 4), new BoardCell(5, 5), new BoardCell(7, 7) }.Select(CreateSeatOneCaterpillar).ToList();
            MatchTable table = CreateTurnTable(SampleLayouts.FiveStartingFlowers, fiveCaterpillars);

            TurnActionOutcome outcome = table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);

            Assert.Equal(TurnActionOutcome.NoCaterpillarsLeft, outcome);
        }

        [Fact]
        public void Test_CountCaterpillarsLeft_AfterTheOpening_ReturnsFour()
        {
            MatchTable table = CreatePlayedOpening();

            int caterpillarsLeft = table.CountCaterpillarsLeft(SeatNumberOne);

            Assert.Equal(MatchTable.CaterpillarsPerSeat - 1, caterpillarsLeft);
        }

        [Fact]
        public void Test_MoveCaterpillar_FromFlowerToGround_ChargesOnlyTheMovement()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.MoveCaterpillar(SeatNumberOne, new CaterpillarMove(SampleLayouts.NorthWestFlower, _southOfNorthWest));

            Assert.Equal(TurnActionOutcome.Applied, outcome);
            Assert.Equal(MatchTable.ActionPointsPerTurn - SampleRules.MovementCost, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_MoveCaterpillar_FromFlowerToGround_LeavesTheCaterpillarOnTheDestination()
        {
            MatchTable table = CreatePlayedOpening();

            table.MoveCaterpillar(SeatNumberOne, new CaterpillarMove(SampleLayouts.NorthWestFlower, _southOfNorthWest));

            Assert.Contains(new Caterpillar(SeatNumberOne, _southOfNorthWest), table.Caterpillars);
        }

        [Fact]
        public void Test_MoveCaterpillar_FromGroundToLevelOneFlower_ChargesMovementAndOneClimb()
        {
            MatchTable table = CreateTurnTable(SampleLayouts.FiveStartingFlowers, new Caterpillar[] { CreateSeatOneCaterpillar(_southOfNorthWest) });

            table.MoveCaterpillar(SeatNumberOne, new CaterpillarMove(_southOfNorthWest, SampleLayouts.NorthWestFlower));

            Assert.Equal(MatchTable.ActionPointsPerTurn - SampleRules.MovementCost - SampleRules.ClimbCost, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_MoveCaterpillar_FromGroundToLevelTwoFlower_ReturnsClimbTooHigh()
        {
            var flowers = new Flower[] { new Flower(SampleLayouts.NorthWestFlower, LevelTwo) };
            MatchTable table = CreateTurnTable(flowers, new Caterpillar[] { CreateSeatOneCaterpillar(_southOfNorthWest) });

            TurnActionOutcome outcome = table.MoveCaterpillar(SeatNumberOne, new CaterpillarMove(_southOfNorthWest, SampleLayouts.NorthWestFlower));

            Assert.Equal(TurnActionOutcome.ClimbTooHigh, outcome);
        }

        [Fact]
        public void Test_MoveCaterpillar_FromLevelThreeFlowerToGround_ChargesOnlyTheMovement()
        {
            var flowers = new Flower[] { new Flower(_tallFlowerCell, LevelThree) };
            MatchTable table = CreateTurnTable(flowers, new Caterpillar[] { CreateSeatOneCaterpillar(_tallFlowerCell) });

            TurnActionOutcome outcome = table.MoveCaterpillar(SeatNumberOne, new CaterpillarMove(_tallFlowerCell, _eastOfNorthWest));

            Assert.Equal(TurnActionOutcome.Applied, outcome);
            Assert.Equal(MatchTable.ActionPointsPerTurn - SampleRules.MovementCost, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_MoveCaterpillar_ToDiagonalCell_ReturnsNotNeighborCell()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.MoveCaterpillar(SeatNumberOne, new CaterpillarMove(SampleLayouts.NorthWestFlower, _diagonalOfNorthWest));

            Assert.Equal(TurnActionOutcome.NotNeighborCell, outcome);
        }

        [Fact]
        public void Test_MoveCaterpillar_OfAnotherSeat_ReturnsNotYourCaterpillar()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.MoveCaterpillar(SeatNumberOne, new CaterpillarMove(SampleLayouts.NorthEastFlower, new BoardCell(1, 5)));

            Assert.Equal(TurnActionOutcome.NotYourCaterpillar, outcome);
        }

        [Fact]
        public void Test_MoveCaterpillar_OntoAnotherCaterpillar_ReturnsCellOccupied()
        {
            MatchTable table = CreateTurnTable(SampleLayouts.FiveStartingFlowers, new Caterpillar[] { _seatOneOnNorthWest, new Caterpillar(SeatNumberTwo, _southOfNorthWest) });

            TurnActionOutcome outcome = table.MoveCaterpillar(SeatNumberOne, new CaterpillarMove(SampleLayouts.NorthWestFlower, _southOfNorthWest));

            Assert.Equal(TurnActionOutcome.CellOccupied, outcome);
        }

        [Fact]
        public void Test_MoveCaterpillar_WithOneActionPointUphill_ReturnsNotEnoughActionPoints()
        {
            MatchTable table = CreateTurnTable(SampleLayouts.FiveStartingFlowers, new Caterpillar[] { CreateSeatOneCaterpillar(_southOfNorthWest), CreateSeatOneCaterpillar(_farGroundCell) });
            table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);
            table.MoveCaterpillar(SeatNumberOne, new CaterpillarMove(_farGroundCell, new BoardCell(4, 7)));
            table.MoveCaterpillar(SeatNumberOne, new CaterpillarMove(new BoardCell(4, 7), _farGroundCell));

            TurnActionOutcome outcome = table.MoveCaterpillar(SeatNumberOne, new CaterpillarMove(_southOfNorthWest, SampleLayouts.NorthWestFlower));

            Assert.Equal(OneActionPoint, table.ActionPointsLeft);
            Assert.Equal(TurnActionOutcome.NotEnoughActionPoints, outcome);
        }

        [Fact]
        public void Test_FindMoveTargets_FromStartingFlower_OffersTheFourGroundNeighbors()
        {
            MatchTable table = CreatePlayedOpening();

            IReadOnlyList<MoveTarget> moveTargets = table.FindMoveTargets(SampleLayouts.NorthWestFlower);

            Assert.Equal(FourNeighborCells, moveTargets.Count);
            Assert.True(moveTargets.All(moveTarget => moveTarget.ActionPointCost == SampleRules.MovementCost));
        }

        [Fact]
        public void Test_FindMoveTargets_FromAnotherSeatCaterpillar_ReturnsNoTargets()
        {
            MatchTable table = CreatePlayedOpening();

            IReadOnlyList<MoveTarget> moveTargets = table.FindMoveTargets(SampleLayouts.NorthEastFlower);

            Assert.Empty(moveTargets);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnFreeFlower_RaisesItOneLevel()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.AddFlowerLevel(SeatNumberOne, SampleLayouts.CenterFlower);

            Assert.Equal(TurnActionOutcome.Applied, outcome);
            Assert.Contains(new Flower(SampleLayouts.CenterFlower, LevelTwo), table.FindFlowers());
        }

        [Fact]
        public void Test_AddFlowerLevel_OnFreeFlower_SpendsOneBuildAndItsCost()
        {
            MatchTable table = CreatePlayedOpening();

            table.AddFlowerLevel(SeatNumberOne, SampleLayouts.CenterFlower);

            Assert.Equal(SampleRules.ThreeBuildings - 1, table.BuildsLeft);
            Assert.Equal(MatchTable.ActionPointsPerTurn - SampleRules.BuildingCost, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_AddFlowerLevel_UnderCaterpillar_ReturnsCellOccupied()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.AddFlowerLevel(SeatNumberOne, SampleLayouts.NorthWestFlower);

            Assert.Equal(TurnActionOutcome.CellOccupied, outcome);
        }

        [Fact]
        public void Test_AddFlowerLevel_UnderButterfly_RaisesTheFlower()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.AddFlowerLevel(SeatNumberOne, SampleLayouts.SouthEastFlower);

            Assert.Equal(TurnActionOutcome.Applied, outcome);
            Assert.Equal(SampleLayouts.SouthEastFlower, table.ButterflyCell);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnLevelThreeFlower_ReturnsFlowerAtMaximumLevel()
        {
            var flowers = new Flower[] { new Flower(_tallFlowerCell, LevelThree) };
            MatchTable table = CreateTurnTable(flowers, Array.Empty<Caterpillar>());

            TurnActionOutcome outcome = table.AddFlowerLevel(SeatNumberOne, _tallFlowerCell);

            Assert.Equal(TurnActionOutcome.FlowerAtMaximumLevel, outcome);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnGroundNextToPlanter_GrowsThatPlanter()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.AddFlowerLevel(SeatNumberOne, _northOfNorthWest);

            Assert.Equal(TurnActionOutcome.Applied, outcome);
            Assert.Contains(table.FindPlanters(), planter => planter.CellCount == TwoPlanterCells);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnGroundAwayFromPlanters_ReturnsNotNextToPlanter()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.AddFlowerLevel(SeatNumberOne, _farGroundCell);

            Assert.Equal(TurnActionOutcome.NotNextToPlanter, outcome);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnGroundBetweenTwoPlanters_ReturnsJoinsPlanters()
        {
            var flowers = new Flower[] { new Flower(_southOfNorthWest, LevelOne), new Flower(new BoardCell(2, 3), LevelOne) };
            MatchTable table = CreateTurnTable(flowers, Array.Empty<Caterpillar>());

            TurnActionOutcome outcome = table.AddFlowerLevel(SeatNumberOne, _diagonalOfNorthWest);

            Assert.Equal(TurnActionOutcome.JoinsPlanters, outcome);
        }

        [Fact]
        public void Test_AddFlowerLevel_AfterThreeBuilds_ReturnsNoBuildsLeft()
        {
            MatchTable table = CreatePlayedOpening();
            table.AddFlowerLevel(SeatNumberOne, SampleLayouts.CenterFlower);
            table.AddFlowerLevel(SeatNumberOne, SampleLayouts.CenterFlower);
            table.AddFlowerLevel(SeatNumberOne, SampleLayouts.SouthWestFlower);

            TurnActionOutcome outcome = table.AddFlowerLevel(SeatNumberOne, SampleLayouts.SouthWestFlower);

            Assert.Equal(TurnActionOutcome.NoBuildsLeft, outcome);
        }

        [Fact]
        public void Test_FindBuildTargets_AfterTheOpening_LeavesOutTheCaterpillarFlowers()
        {
            MatchTable table = CreatePlayedOpening();

            IReadOnlyList<BoardCell> buildTargets = table.FindBuildTargets();

            Assert.DoesNotContain(SampleLayouts.NorthWestFlower, buildTargets);
            Assert.Contains(SampleLayouts.CenterFlower, buildTargets);
            Assert.Contains(_northOfNorthWest, buildTargets);
        }

        [Fact]
        public void Test_EndTurn_BySeatOne_GivesTheTurnToSeatTwo()
        {
            MatchTable table = CreatePlayedOpening();
            table.PlaceCaterpillar(SeatNumberOne, SampleLayouts.CenterFlower);

            TurnActionOutcome outcome = table.EndTurn(SeatNumberOne);

            Assert.Equal(TurnActionOutcome.Applied, outcome);
            Assert.Equal(SeatNumberTwo, table.ActiveSeatNumber);
            Assert.Equal(MatchTable.ActionPointsPerTurn, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_EndTurn_ByLastSeat_StartsTheNextTurnWithSeatOne()
        {
            MatchTable table = CreatePlayedOpening();
            table.EndTurn(SeatNumberOne);

            table.EndTurn(SeatNumberTwo);

            Assert.Equal(TurnNumberTwo, table.TurnNumber);
            Assert.Equal(SeatNumberOne, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_EndTurn_OutOfTurn_ReturnsNotYourTurn()
        {
            MatchTable table = CreatePlayedOpening();

            TurnActionOutcome outcome = table.EndTurn(SeatNumberTwo);

            Assert.Equal(TurnActionOutcome.NotYourTurn, outcome);
        }

        [Fact]
        public void Test_ExpireTurn_OfSeatOne_GivesTheTurnToSeatTwo()
        {
            MatchTable table = CreatePlayedOpening();

            table.ExpireTurn();

            Assert.Equal(SeatNumberTwo, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_ExpireTurn_DuringCaterpillarPlacement_ThrowsInvalidOperationException()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, TwoSeatCount, SampleRules.TwoSeatRules);

            Assert.Throws<InvalidOperationException>(table.ExpireTurn);
        }

        [Fact]
        public void Test_ExpireTurn_AfterEveryTurnOfRoundOne_StartsRoundTwo()
        {
            MatchTable table = CreatePlayedOpening();

            ExpireTurns(table, FourTurns * TwoSeatCount);

            Assert.Equal(RoundNumberTwo, table.RoundNumber);
            Assert.Equal(MatchTable.FirstTurnNumber, table.TurnNumber);
            Assert.Equal(SeatNumberOne, table.ActiveSeatNumber);
        }

        [Fact]
        public void Test_ExpireTurn_AfterEveryTurnOfRoundThree_FinishesTheMatch()
        {
            MatchTable table = CreatePlayedOpening(SampleRules.OneTurnPerRoundRules);

            ExpireTurns(table, MatchRules.RoundCount * TwoSeatCount);

            Assert.Equal(TablePhase.Finished, table.Phase);
            Assert.Empty(table.FindBuildTargets());
        }

        [Fact]
        public void Test_ExpireTurn_InTurnThreeWithThreeSeats_HandsOutTwoBuilds()
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, ThreeSeatCount, SampleRules.ThreeSeatRules);
            table.PlaceInitialCaterpillar(SeatNumberOne, SampleLayouts.NorthWestFlower);
            table.PlaceInitialCaterpillar(SeatNumberTwo, SampleLayouts.NorthEastFlower);
            table.PlaceInitialCaterpillar(SeatNumberThree, SampleLayouts.SouthWestFlower);
            table.PlaceButterfly(SeatNumberThree, SampleLayouts.CenterFlower);

            ExpireTurns(table, TurnNumberTwo * ThreeSeatCount);

            Assert.Equal(TurnNumberThree, table.TurnNumber);
            Assert.Equal(SampleRules.TwoBuildings, table.BuildsLeft);
        }

        [Fact]
        public void Test_TakeSnapshot_AfterSpendingPoints_KeepsThePointsAndBuildsLeft()
        {
            MatchTable table = CreatePlayedOpening();
            table.AddFlowerLevel(SeatNumberOne, SampleLayouts.CenterFlower);

            MatchTable restoredTable = MatchTable.RestoreFromSnapshot(table.TakeSnapshot(), SampleRules.TwoSeatRules);

            Assert.Equal(table.ActionPointsLeft, restoredTable.ActionPointsLeft);
            Assert.Equal(table.BuildsLeft, restoredTable.BuildsLeft);
        }

        private static MatchTable CreatePlayedOpening()
        {
            MatchTable table = CreatePlayedOpening(SampleRules.TwoSeatRules);

            return table;
        }

        private static MatchTable CreatePlayedOpening(MatchRules rules)
        {
            var table = new MatchTable(SampleLayouts.FiveSingleCellPlanters, TwoSeatCount, rules);
            table.PlaceInitialCaterpillar(SeatNumberOne, SampleLayouts.NorthWestFlower);
            table.PlaceInitialCaterpillar(SeatNumberTwo, SampleLayouts.NorthEastFlower);
            table.PlaceButterfly(SeatNumberTwo, SampleLayouts.SouthEastFlower);

            return table;
        }

        private static MatchTable CreateTurnTable(IReadOnlyList<Flower> flowers, IReadOnlyList<Caterpillar> caterpillars)
        {
            var snapshot = new MatchSnapshot(TablePhase.PlayerTurn, MatchTable.FirstRoundNumber, MatchTable.FirstTurnNumber, TwoSeatCount, SeatNumberOne, MatchTable.ActionPointsPerTurn, SampleRules.ThreeBuildings, flowers, caterpillars, null);
            MatchTable table = MatchTable.RestoreFromSnapshot(snapshot, SampleRules.TwoSeatRules);

            return table;
        }

        private static Caterpillar CreateSeatOneCaterpillar(BoardCell caterpillarCell)
        {
            var caterpillar = new Caterpillar(SeatNumberOne, caterpillarCell);

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
