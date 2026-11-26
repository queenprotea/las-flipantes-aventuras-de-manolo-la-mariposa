using Game.Contracts;

namespace Torres.Client.Connection
{
    internal sealed record PiecePlacement(MatchPiece Piece, MatchCell DestinationCell);
}
