using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    public sealed class MatchRules
    {
        public const int RoundCount = 3;

        private readonly List<RoundTurnPlan> _roundTurnPlans;

        public MatchRules(IReadOnlyList<RoundTurnPlan> roundTurnPlans, ActionCosts actionCosts)
        {
            _roundTurnPlans = roundTurnPlans.ToList();
            ActionCosts = actionCosts;
        }

        public ActionCosts ActionCosts { get; }

        public int GetTurnCount(int roundNumber)
        {
            RoundTurnPlan roundTurnPlan = GetRoundTurnPlan(roundNumber);
            int turnCount = roundTurnPlan.TurnCount;

            return turnCount;
        }

        public int GetBuildCount(int roundNumber, int turnRoundNumber)
        {
            RoundTurnPlan roundTurnPlan = GetRoundTurnPlan(roundNumber);

            int buildCount = roundTurnPlan.BuildCountsByTurn[turnRoundNumber - MatchTable.FirstTurnRoundNumber];

            return buildCount;
        }

        private RoundTurnPlan GetRoundTurnPlan(int roundNumber)
        {
            RoundTurnPlan roundTurnPlan = _roundTurnPlans[roundNumber - MatchTable.FirstRoundNumber];

            return roundTurnPlan;
        }
    }
}
