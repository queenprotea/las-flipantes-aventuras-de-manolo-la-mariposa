using System.Collections.Generic;

using Game.Contracts;

using Torres.Client.Scene;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class MatchBoardLayoutFactoryTests
    {
        private const int FlowerRow = 3;
        private const int FlowerColumn = 3;
        private const int CaterpillarRow = 5;
        private const int CaterpillarColumn = 5;
        private const int SeatNumberTwo = 2;
        private const float SeatTwoHeadingDegrees = 380f;

        private static readonly BoardCell _caterpillarCell = new BoardCell(CaterpillarRow, CaterpillarColumn);

        private readonly MatchView _matchView = new MatchView
        {
            Flowers = new List<MatchFlower>
            {
                new MatchFlower { Cell = new MatchCell { Row = FlowerRow, Column = FlowerColumn }, PlanterNumber = SampleBoards.SecondPlanterNumber, Level = SampleBoards.FlowerLevelTwo },
            },
            Caterpillars = new List<MatchCaterpillar>
            {
                new MatchCaterpillar { Cell = new MatchCell { Row = CaterpillarRow, Column = CaterpillarColumn }, SeatNumber = SeatNumberTwo },
            },
            Seats = new List<MatchSeat>
            {
                new MatchSeat { SeatNumber = SeatNumberTwo, Color = SeatColor.Blue, RoundScores = new List<MatchRoundScore>() },
            },
        };

        [Fact]
        public void Test_Create_WithAFlower_KeepsItsLevel()
        {
            BoardLayout layout = MatchBoardLayoutFactory.Create(_matchView);

            int flowerLevel = layout.GetFlowerLevel(SampleBoards.CellOfSecondPlanter);
            Assert.Equal(SampleBoards.FlowerLevelTwo, flowerLevel);
        }

        [Fact]
        public void Test_Create_WithAFlower_KeepsItsPlanter()
        {
            BoardLayout layout = MatchBoardLayoutFactory.Create(_matchView);

            int planterNumber = layout.GetPlanterNumber(SampleBoards.CellOfSecondPlanter);
            Assert.Equal(SampleBoards.SecondPlanterNumber, planterNumber);
        }

        [Fact]
        public void Test_Create_WithAFlower_LeavesTheRestAsGround()
        {
            BoardLayout layout = MatchBoardLayoutFactory.Create(_matchView);

            bool isPlanter = layout.IsPlanter(SampleBoards.GroundCell);
            Assert.False(isPlanter);
        }

        [Fact]
        public void Test_Create_WithACaterpillar_AddsOneCaterpillar()
        {
            BoardLayout layout = MatchBoardLayoutFactory.Create(_matchView);

            Assert.Single(layout.Caterpillars);
        }

        [Fact]
        public void Test_Create_WithACaterpillar_PlacesItOnItsCell()
        {
            BoardLayout layout = MatchBoardLayoutFactory.Create(_matchView);

            BoardCaterpillar caterpillar = layout.Caterpillars[0];
            Assert.Equal(_caterpillarCell, caterpillar.Cell);
        }

        [Fact]
        public void Test_Create_WithACaterpillar_PaintsItWithTheColorOfItsSeat()
        {
            BoardLayout layout = MatchBoardLayoutFactory.Create(_matchView);

            BoardCaterpillar caterpillar = layout.Caterpillars[0];
            Assert.Equal(SeatColor.Blue, caterpillar.SeatColor);
        }

        [Fact]
        public void Test_Create_WithACaterpillarOfSeatTwo_TurnsItToTheHeadingOfSeatTwo()
        {
            BoardLayout layout = MatchBoardLayoutFactory.Create(_matchView);

            BoardCaterpillar caterpillar = layout.Caterpillars[0];
            Assert.Equal(SeatTwoHeadingDegrees, caterpillar.HeadingDegrees);
        }
    }
}
