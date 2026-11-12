using Game.Contracts;

namespace Torres.Client.Session
{
    internal sealed record PiecePlacement(MatchPiece Piece, MatchCell DestinationCell);
}
