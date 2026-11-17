using System.Collections.Generic;

namespace Game.Match
{
    public sealed record MatchOpening(int RoomId, IReadOnlyList<MatchEntrant> Entrants);
}
