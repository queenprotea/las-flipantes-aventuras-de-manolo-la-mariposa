using System;
using System.Collections.Generic;

using Xunit;

namespace Game.Domain.Tests
{
    public sealed class RoundScoreCalculatorTests
    {
        private const int SeatNumberOne = 1;
        private const int SeatNumberTwo = 2;
        private const int RoundNumberOne = 1;
        private const int RoundNumberTwo = 2;
        private const int LevelOne = 1;
        private const int LevelTwo = 2;
        private const int SixPoints = 6;
        private const int ThreePoints = 3;

        private static readonly BoardCell _westCell = new BoardCell(2, 1);
        private static readonly BoardCell _middleCell = new BoardCell(2, 2);
        private static readonly BoardCell _eastCell = new BoardCell(2, 3);
        private static readonly IReadOnlyList<Planter> _threeCellPlanter = new Board(new Flower[] { new Flower(_westCell, LevelOne), new Flower(_middleCell, LevelTwo), new Flower(_eastCell, LevelOne) }).FindPlanters();

        [Fact]
        public void Test_ScoreSeat_WithOneCaterpillarOnLevelTwo_MultipliesSurfaceByLevel()
        {
            var calculator = new RoundScoreCalculator(_threeCellPlanter, new Caterpillar[] { new Caterpillar(SeatNumberOne, _middleCell) }, null);

            SeatRoundScore roundScore = calculator.ScoreSeat(SeatNumberOne, RoundNumberOne);

            Assert.Equal(SixPoints, roundScore.PlanterPoints);
            Assert.Equal(1, roundScore.ScoredPlanterCount);
        }

        [Fact]
        public void Test_ScoreSeat_WithTwoOwnCaterpillarsInOnePlanter_ScoresNothing()
        {
            var calculator = new RoundScoreCalculator(_threeCellPlanter, new Caterpillar[] { new Caterpillar(SeatNumberOne, _middleCell), new Caterpillar(SeatNumberOne, _westCell) }, null);

            SeatRoundScore roundScore = calculator.ScoreSeat(SeatNumberOne, RoundNumberOne);

            Assert.Equal(0, roundScore.TotalPoints);
            Assert.Equal(0, roundScore.ScoredPlanterCount);
        }

        [Fact]
        public void Test_ScoreSeat_WithAnotherSeatInThePlanter_ScoresBothSeats()
        {
            var calculator = new RoundScoreCalculator(_threeCellPlanter, new Caterpillar[] { new Caterpillar(SeatNumberOne, _middleCell), new Caterpillar(SeatNumberTwo, _westCell) }, null);

            SeatRoundScore seatTwoScore = calculator.ScoreSeat(SeatNumberTwo, RoundNumberOne);

            Assert.Equal(ThreePoints, seatTwoScore.PlanterPoints);
        }

        [Fact]
        public void Test_ScoreSeat_AtTheButterflyLevelInItsPlanter_AddsTheRoundBonus()
        {
            var calculator = new RoundScoreCalculator(_threeCellPlanter, new Caterpillar[] { new Caterpillar(SeatNumberOne, _westCell) }, _eastCell);

            SeatRoundScore roundScore = calculator.ScoreSeat(SeatNumberOne, RoundNumberTwo);

            Assert.Equal(RoundScoreCalculator.RoundTwoButterflyPoints, roundScore.ButterflyPoints);
        }

        [Fact]
        public void Test_ScoreSeat_AtAnotherLevelThanTheButterfly_AddsNoBonus()
        {
            var calculator = new RoundScoreCalculator(_threeCellPlanter, new Caterpillar[] { new Caterpillar(SeatNumberOne, _westCell) }, _middleCell);

            SeatRoundScore roundScore = calculator.ScoreSeat(SeatNumberOne, RoundNumberTwo);

            Assert.Equal(0, roundScore.ButterflyPoints);
        }

        [Fact]
        public void Test_ScoreSeat_InRoundFour_ThrowsArgumentOutOfRangeException()
        {
            var calculator = new RoundScoreCalculator(_threeCellPlanter, Array.Empty<Caterpillar>(), null);

            Assert.Throws<ArgumentOutOfRangeException>(() => calculator.ScoreSeat(SeatNumberOne, MatchRules.RoundCount + 1));
        }
    }
}
