using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Services.Tests
{
    internal sealed class RecordingMatchParticipantRepository : IMatchParticipantRepository
    {
        private readonly Lock _participantLock = new Lock();

        internal List<MatchParticipant> Participants { get; } = new List<MatchParticipant>();

        public Task<List<MatchParticipant>> ListByMatchesAsync(IReadOnlyCollection<int> matchIds)
        {
            List<MatchParticipant> listedParticipants;
            lock (_participantLock)
            {
                listedParticipants = Participants.Where(participant => matchIds.Contains(participant.MatchId)).ToList();
            }

            Task<List<MatchParticipant>> listing = Task.FromResult(listedParticipants);

            return listing;
        }

        public Task AddAsync(int matchId, IReadOnlyList<MatchParticipant> seatedPlayers)
        {
            lock (_participantLock)
            {
                Participants.AddRange(seatedPlayers);
            }

            Task adding = Task.CompletedTask;

            return adding;
        }

        public Task WriteResultsAsync(int matchId, IReadOnlyList<MatchParticipant> playerResults)
        {
            Task writing = Task.CompletedTask;

            return writing;
        }
    }
}
