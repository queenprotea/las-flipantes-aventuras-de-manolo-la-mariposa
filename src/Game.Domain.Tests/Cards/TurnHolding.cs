using System.Collections.Generic;

namespace Game.Domain.Tests
{
    internal sealed record TurnHolding(IReadOnlyList<ActionCard> Hand, int ActionPointsLeft);
}
