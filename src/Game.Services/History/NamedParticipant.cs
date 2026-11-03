using Game.Persistence;

namespace Game.Services
{
    internal sealed record NamedParticipant(MatchParticipant Participation, string Username);
}
