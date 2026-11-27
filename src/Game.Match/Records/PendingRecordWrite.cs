using Game.Persistence;

namespace Game.Match
{
    internal sealed record PendingRecordWrite(FinishedMatchRecord? FinishedMatch, MatchState? SavedState);
}
