using System.Collections.Generic;

namespace Game.Match
{
    public sealed record MatchRuleSet(IReadOnlyList<RoundRule> RoundRules, ActionPointCosts ActionPointCosts);
}
