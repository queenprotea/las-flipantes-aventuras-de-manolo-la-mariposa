using System.Collections.Generic;

namespace Game.Match
{
    public sealed record RoundRule(IReadOnlyList<int> BuildCountsByTurn);
}
