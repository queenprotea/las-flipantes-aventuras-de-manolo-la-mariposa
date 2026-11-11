using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    public sealed class RoundScoreCalculator
    {
        public const int RoundOneButterflyPoints = 5;
        public const int RoundTwoButterflyPoints = 10;
        public const int RoundThreeButterflyPoints = 15;

        private const int SingleCaterpillarCount = 1;

        private static readonly int[] _butterflyPointsByRound = { RoundOneButterflyPoints, RoundTwoButterflyPoints, RoundThreeButterflyPoints };

        private readonly IReadOnlyList<Planter> _planters;
        private readonly IReadOnlyList<Caterpillar> _caterpillars;
        private readonly BoardCell? _butterflyCell;

        public RoundScoreCalculator(IReadOnlyList<Planter> planters, IReadOnlyList<Caterpillar> caterpillars, BoardCell? butterflyCell)
        {
            ArgumentNullException.ThrowIfNull(planters);
            ArgumentNullException.ThrowIfNull(caterpillars);

            _planters = planters;
            _caterpillars = caterpillars;
            _butterflyCell = butterflyCell;
        }

        public SeatRoundScore ScoreSeat(int seatNumber, int roundNumber)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(roundNumber, MatchTable.FirstRoundNumber);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(roundNumber, MatchRules.RoundCount);

            List<Planter> scoredPlanters = _planters.Where(planter => CountCaterpillarsOf(planter, seatNumber) == SingleCaterpillarCount).ToList();
            int planterPoints = scoredPlanters.Sum(planter => CalculatePlanterPoints(planter, seatNumber));
            int butterflyPoints = 0;
            if (IsWithButterfly(seatNumber))
            {
                butterflyPoints = _butterflyPointsByRound[roundNumber - MatchTable.FirstRoundNumber];
            }

            var seatRoundScore = new SeatRoundScore(planterPoints, butterflyPoints, scoredPlanters.Count);

            return seatRoundScore;
        }

        private int CountCaterpillarsOf(Planter planter, int seatNumber)
        {
            int caterpillarCount = planter.Flowers.Count(flower => HasCaterpillarOf(seatNumber, flower.Cell));

            return caterpillarCount;
        }

        private int CalculatePlanterPoints(Planter planter, int seatNumber)
        {
            int caterpillarLevel = planter.Flowers.First(flower => HasCaterpillarOf(seatNumber, flower.Cell)).Level;
            int planterPoints = planter.CellCount * caterpillarLevel;

            return planterPoints;
        }

        private bool IsWithButterfly(int seatNumber)
        {
            Planter? butterflyPlanter = _planters.FirstOrDefault(IsButterflyPlanter);
            bool isWithButterfly = false;
            if (butterflyPlanter is not null)
            {
                int butterflyLevel = butterflyPlanter.Flowers.First(IsUnderButterfly).Level;
                isWithButterfly = butterflyPlanter.Flowers.Any(flower => (flower.Level == butterflyLevel) && HasCaterpillarOf(seatNumber, flower.Cell));
            }

            return isWithButterfly;
        }

        private bool IsButterflyPlanter(Planter planter)
        {
            bool isButterflyPlanter = planter.Flowers.Any(IsUnderButterfly);

            return isButterflyPlanter;
        }

        private bool IsUnderButterfly(Flower flower)
        {
            bool isUnderButterfly = flower.Cell == _butterflyCell;

            return isUnderButterfly;
        }

        private bool HasCaterpillarOf(int seatNumber, BoardCell examinedCell)
        {
            bool hasCaterpillar = _caterpillars.Contains(new Caterpillar(seatNumber, examinedCell));

            return hasCaterpillar;
        }
    }
}
