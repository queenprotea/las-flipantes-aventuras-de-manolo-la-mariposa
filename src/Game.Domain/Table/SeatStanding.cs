using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    public sealed record SeatStanding(int SeatNumber, IReadOnlyList<int> RoundScores, int ScoredPlanterCount, bool IsRetired)
    {
        public int TotalScore => RoundScores.Sum();
    }
}
