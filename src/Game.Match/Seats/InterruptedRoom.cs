using System.Collections.Generic;

namespace Game.Match
{
    public sealed record InterruptedRoom(string RoomCode, IReadOnlyList<MatchEntrant> Entrants);
}
