using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    public sealed record SeatStanding(int SeatNumber, IReadOnlyList<SeatRoundScore> RoundScores, bool IsRetired)
    {
        public int TotalScore => RoundScores.Sum(roundScore => roundScore.TotalPoints);

        public int ScoredPlanterCount => RoundScores.Sum(roundScore => roundScore.ScoredPlanterCount);
    }
}
