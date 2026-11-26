using System.Collections.Generic;

using Game.Contracts;

namespace Torres.Client.Flows
{
    internal sealed record GlobalRankingOutcome(List<RankingEntry>? Entries);
}
