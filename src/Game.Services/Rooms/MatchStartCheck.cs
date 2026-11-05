using System.Collections.Generic;

using Game.Contracts;
using Game.Match;

namespace Game.Services
{
    internal sealed record MatchStartCheck(StartMatchStatus Status, IReadOnlyList<MatchEntrant> Entrants);
}
