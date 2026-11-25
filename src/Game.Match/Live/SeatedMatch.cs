using System.Collections.Generic;

using Game.Domain;

namespace Game.Match
{
    internal sealed record SeatedMatch(int MatchId, string RoomCode, IReadOnlyList<SeatedEntrant> SeatedEntrants, StartingLayout StartingLayout);
}
