using Game.Domain;

namespace Game.Match
{
    internal readonly record struct TableStep(TablePhase Phase, int RoundNumber, int TurnNumber, int ActiveSeatNumber);
}
