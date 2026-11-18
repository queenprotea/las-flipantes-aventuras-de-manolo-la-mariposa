using System;
using System.Collections.Generic;

namespace Game.Match
{
    /// <summary>
    /// What the matches left in progress by a server restart need to go on: the clocks, the rules for each number of seats, and how long a match waits for its players to come back.
    /// </summary>
    public sealed record MatchResumption(MatchTimeLimits TimeLimits, IReadOnlyDictionary<int, MatchRuleSet> RuleSetsBySeatCount, TimeSpan ResumeTime);
}
