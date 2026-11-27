using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    public sealed class RoundTurnPlan
    {
        public RoundTurnPlan(IReadOnlyList<int> buildCountsByTurn)
        {
            BuildCountsByTurn = buildCountsByTurn.ToList();
        }

        public IReadOnlyList<int> BuildCountsByTurn { get; }

        public int TurnCount => BuildCountsByTurn.Count;
    }
}
