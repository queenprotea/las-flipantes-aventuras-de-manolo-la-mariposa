using System.Collections.Generic;

using Xunit;

namespace Game.Domain.Tests
{
    public sealed class RoundScoreCalculatorTests
    {
        private const int SixPoints = 6;
        private const int ThreePoints = 3;

        private static readonly BoardCell _westCell = new BoardCell(2, 1);
        private static readonly BoardCell _middleCell = new BoardCell(2, 2);
        private static readonly BoardCell _eastCell = new BoardCell(2, 3);
        private static readonly IReadOnlyList<Planter> _threeCellPlanter = new Board(new Flower[] { new Flower(_westCell, SampleLayouts.LevelOne), new Flower(_middleCell, SampleLayouts.LevelTwo), new Flower(_eastCell, SampleLayouts.LevelOne) }).FindPlanters();

        [Fact]
        public void Test_ScoreSeat_WithOneCaterpillarOnLevelTwo_MultipliesSurfaceByLevel()
        {
            var calculator = new RoundScoreCalculator(_threeCellPlanter, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, _middleCell) }, null);

            SeatRoundScore roundScore = calculator.ScoreSeat(SampleRules.SeatNumberOne, SampleRules.RoundNumberOne);

            Assert.Equal(SixPoints, roundScore.PlanterPoints);
            Assert.Equal(1, roundScore.ScoredPlanterCount);
        }

        [Fact]
        public void Test_ScoreSeat_WithTwoOwnCaterpillarsInOnePlanter_ScoresNothing()
        {
            var calculator = new RoundScoreCalculator(_threeCellPlanter, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, _middleCell), new Caterpillar(SampleRules.SeatNumberOne, _westCell) }, null);

            SeatRoundScore roundScore = calculator.ScoreSeat(SampleRules.SeatNumberOne, SampleRules.RoundNumberOne);

            Assert.Equal(0, roundScore.TotalPoints);
            Assert.Equal(0, roundScore.ScoredPlanterCount);
        }

        [Fact]
        public void Test_ScoreSeat_WithAnotherSeatInThePlanter_ScoresBothSeats()
        {
            var calculator = new RoundScoreCalculator(_threeCellPlanter, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, _middleCell), new Caterpillar(SampleRules.SeatNumberTwo, _westCell) }, null);

            SeatRoundScore seatTwoScore = calculator.ScoreSeat(SampleRules.SeatNumberTwo, SampleRules.RoundNumberOne);

            Assert.Equal(ThreePoints, seatTwoScore.PlanterPoints);
        }

        [Fact]
        public void Test_ScoreSeat_AtTheButterflyLevelInItsPlanter_AddsTheRoundBonus()
        {
            var calculator = new RoundScoreCalculator(_threeCellPlanter, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, _westCell) }, _eastCell);

            SeatRoundScore roundScore = calculator.ScoreSeat(SampleRules.SeatNumberOne, SampleRules.RoundNumberTwo);

            Assert.Equal(RoundScoreCalculator.RoundTwoButterflyPoints, roundScore.ButterflyPoints);
        }

        [Fact]
        public void Test_ScoreSeat_AtAnotherLevelThanTheButterfly_AddsNoBonus()
        {
            var calculator = new RoundScoreCalculator(_threeCellPlanter, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, _westCell) }, _middleCell);

            SeatRoundScore roundScore = calculator.ScoreSeat(SampleRules.SeatNumberOne, SampleRules.RoundNumberTwo);

            Assert.Equal(0, roundScore.ButterflyPoints);
        }
    }
}
