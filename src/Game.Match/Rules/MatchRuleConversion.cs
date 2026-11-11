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
            ArgumentNullException.ThrowIfNull(ruleSet.ActionPointCosts, nameof(ruleSet));

            List<RoundLayout> roundLayouts = ruleSet.RoundRules.Select(ToRoundLayout).ToList();
            ActionPointCosts actionPointCosts = ruleSet.ActionPointCosts;
            var actionCosts = new ActionCosts(actionPointCosts.CaterpillarPlacement, actionPointCosts.Movement, actionPointCosts.LevelClimb, actionPointCosts.FlowerLevelAddition);
            var matchRules = new MatchRules(roundLayouts, actionCosts);

            return matchRules;
        }

        private static RoundLayout ToRoundLayout(RoundRule roundRule)
        {
            var roundLayout = new RoundLayout(roundRule.BuildCountsByTurn);

            return roundLayout;
        }
    }
}
