using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Match.Tests
{
    internal sealed class RecordingMatchParticipantRepository : IMatchParticipantRepository
    {
        private readonly Lock _participantLock = new Lock();
        private readonly TaskCompletionSource<IReadOnlyList<MatchParticipant>> _firstResults = new TaskCompletionSource<IReadOnlyList<MatchParticipant>>(TaskCreationOptions.RunContinuationsAsynchronously);

        private IReadOnlyList<MatchParticipant> _lastSeatedPlayers = Array.Empty<MatchParticipant>();

        internal IReadOnlyList<MatchParticipant> LastSeatedPlayers
        {
            get
            {
                lock (_participantLock)
                {
                    return _lastSeatedPlayers;
                }
            }
        }

        internal Task<IReadOnlyList<MatchParticipant>> FirstResults => _firstResults.Task;

        public Task AddAsync(int matchId, IReadOnlyList<MatchParticipant> seatedPlayers)
        {
            lock (_participantLock)
            {
                _lastSeatedPlayers = seatedPlayers;
            }

            Task adding = Task.CompletedTask;

            return adding;
        }

        public Task WriteResultsAsync(int matchId, IReadOnlyList<MatchParticipant> playerResults)
        {
            _firstResults.TrySetResult(playerResults);
            Task resultWriting = Task.CompletedTask;

            return resultWriting;
        }
    }
}
