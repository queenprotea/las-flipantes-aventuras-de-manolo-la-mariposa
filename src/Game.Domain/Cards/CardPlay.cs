using System.Collections.Generic;

namespace Game.Domain
{
    public sealed record CardPlay(ActionCard Card, IReadOnlyList<BoardCell> AffectedCells);
}
