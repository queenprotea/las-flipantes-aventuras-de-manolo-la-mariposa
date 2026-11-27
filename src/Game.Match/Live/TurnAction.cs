using Game.Contracts;
using Game.Domain;

namespace Game.Match
{
    internal sealed record TurnAction(TurnActionKind Kind)
    {
        internal MatchPiece Piece { get; init; }

        internal BoardCell TargetCell { get; init; }

        internal CaterpillarMove Move { get; init; }

        internal CardPlay? CardPlay { get; init; }
    }
}
