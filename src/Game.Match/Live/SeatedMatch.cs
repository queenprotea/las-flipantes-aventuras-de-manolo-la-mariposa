using System.Collections.Generic;

namespace Game.Match
{
    internal sealed record SeatedMatch(int MatchId, string RoomCode, IReadOnlyList<SeatedEntrant> SeatedEntrants);
}
