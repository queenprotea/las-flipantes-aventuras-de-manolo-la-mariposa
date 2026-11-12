using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    public sealed class MatchRules
    {
        public const int RoundCount = 3;

        private const string RoundCountMessage = "The rules must describe exactly three rounds.";

        private readonly IReadOnlyList<RoundTurnPlan> _roundTurnPlans;

        public MatchRules(IReadOnlyList<RoundTurnPlan> roundTurnPlans, ActionCosts actionCosts)
        {
            ArgumentNullException.ThrowIfNull(roundTurnPlans);
            ArgumentNullException.ThrowIfNull(actionCosts);
            if ((roundTurnPlans.Count != RoundCount) || roundTurnPlans.Any(roundTurnPlan => roundTurnPlan is null))
            {
                throw new ArgumentException(RoundCountMessage, nameof(roundTurnPlans));
            }

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
            ArgumentOutOfRangeException.ThrowIfLessThan(turnRoundNumber, MatchTable.FirstTurnRoundNumber);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(turnRoundNumber, roundTurnPlan.TurnCount);

            int buildCount = roundTurnPlan.BuildCountsByTurn[turnRoundNumber - MatchTable.FirstTurnRoundNumber];

            return buildCount;
        }

        private RoundTurnPlan GetRoundTurnPlan(int roundNumber)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(roundNumber, MatchTable.FirstRoundNumber);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(roundNumber, RoundCount);

            RoundTurnPlan roundTurnPlan = _roundTurnPlans[roundNumber - MatchTable.FirstRoundNumber];

            return roundTurnPlan;
        }
    }
}
