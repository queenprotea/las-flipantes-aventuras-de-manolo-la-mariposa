using Game.Contracts;

namespace Game.Match
{
    public sealed record MatchRecipientView(int EntrantId, int MatchId, MatchView MatchView);
}
