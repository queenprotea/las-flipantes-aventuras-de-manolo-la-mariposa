using System;
using System.Collections.Generic;
using System.Linq;

using Game.Domain;

namespace Game.Match
{
    internal static class MatchRuleConversion
    {
        internal static MatchRules ToMatchRules(MatchRuleSet ruleSet)
        {
            ArgumentNullException.ThrowIfNull(ruleSet);
            ArgumentNullException.ThrowIfNull(ruleSet.RoundRules, nameof(ruleSet));

            List<RoundTurnPlan> roundTurnPlans = ruleSet.RoundRules.Select(ToRoundTurnPlan).ToList();
            ActionPointCosts actionPointCosts = ruleSet.ActionPointCosts;
            var actionCosts = new ActionCosts(actionPointCosts.CaterpillarPlacement, actionPointCosts.Movement, actionPointCosts.LevelClimb, actionPointCosts.FlowerLevelAddition);
            var matchRules = new MatchRules(roundTurnPlans, actionCosts);

            return matchRules;
        }

        private static RoundTurnPlan ToRoundTurnPlan(RoundRule roundRule)
        {
            var roundTurnPlan = new RoundTurnPlan(roundRule.BuildCountsByTurn);

            return roundTurnPlan;
        }
    }
}
