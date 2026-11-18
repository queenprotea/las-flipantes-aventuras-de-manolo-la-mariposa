using Game.Persistence;

namespace Game.Match
{
    /// <summary>
    /// The three repositories where a match is recorded: the match itself, its seated players and the state saved at the start of every step.
    /// </summary>
    public sealed record MatchRepositories(IMatchRecordRepository MatchRecordRepository, IMatchParticipantRepository MatchParticipantRepository, IMatchStateRepository MatchStateRepository);
}
