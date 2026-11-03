using System.Collections.Generic;

using Game.Persistence;

namespace Game.Services
{
    internal sealed record PlayedMatch(MatchRecord Match, IReadOnlyList<NamedParticipant> Participants);
}
