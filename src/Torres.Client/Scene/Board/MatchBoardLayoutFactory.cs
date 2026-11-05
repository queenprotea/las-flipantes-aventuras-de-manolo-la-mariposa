using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game.Contracts;

namespace Torres.Client.Scene
{
    internal sealed class MatchBoardLayoutFactory
    {
        private const char FirstPlanterSymbol = 'A';
        private const float FirstSeatHeadingDegrees = 290f;
        private const float SeatHeadingStepDegrees = 90f;

        internal BoardLayout Create(MatchView matchView)
        {
            var planterRows = new List<string>();
            var levelRows = new List<string>();
            for (var row = 0; row < BoardLayout.CellsPerSide; row++)
            {
                planterRows.Add(BuildPlanterRow(matchView.Flowers, row));
                levelRows.Add(BuildLevelRow(matchView.Flowers, row));
            }

            List<BoardCaterpillar> caterpillars = matchView.Caterpillars.Select(caterpillar => ToBoardCaterpillar(matchView.Seats, caterpillar)).ToList();
            var boardLayout = new BoardLayout(planterRows, levelRows, caterpillars);

            return boardLayout;
        }

        private static string BuildPlanterRow(IReadOnlyList<MatchFlower> flowers, int row)
        {
            var planterRow = new StringBuilder(new string(BoardLayout.GroundSymbol, BoardLayout.CellsPerSide));
            foreach (MatchFlower flower in flowers.Where(flower => flower.Cell.Row == row))
            {
                planterRow[flower.Cell.Column] = (char)(FirstPlanterSymbol + flower.PlanterNumber - 1);
            }

            string planterText = planterRow.ToString();

            return planterText;
        }

        private static string BuildLevelRow(IReadOnlyList<MatchFlower> flowers, int row)
        {
            var levelRow = new StringBuilder(new string(BoardLayout.GroundSymbol, BoardLayout.CellsPerSide));
            foreach (MatchFlower flower in flowers.Where(flower => flower.Cell.Row == row))
            {
                levelRow[flower.Cell.Column] = (char)(BoardLayout.LevelZeroSymbol + flower.Level);
            }

            string levelText = levelRow.ToString();

            return levelText;
        }

        private static BoardCaterpillar ToBoardCaterpillar(IReadOnlyList<MatchSeat> seats, MatchCaterpillar caterpillar)
        {
            SeatColor seatColor = seats.First(seat => seat.SeatNumber == caterpillar.SeatNumber).Color;
            float headingDegrees = FirstSeatHeadingDegrees + (SeatHeadingStepDegrees * (caterpillar.SeatNumber - 1));
            var boardCaterpillar = new BoardCaterpillar(seatColor, MatchCellConversion.ToBoardCell(caterpillar.Cell), headingDegrees);

            return boardCaterpillar;
        }
    }
}
