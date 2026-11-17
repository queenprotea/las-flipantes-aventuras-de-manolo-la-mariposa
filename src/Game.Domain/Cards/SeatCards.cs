using System.Collections.Generic;

namespace Game.Domain
{
    public sealed record SeatCards(int SeatNumber, IReadOnlyList<ActionCard> Hand, IReadOnlyList<ActionCard> Deck, bool HasUsedPointsCard);
}
