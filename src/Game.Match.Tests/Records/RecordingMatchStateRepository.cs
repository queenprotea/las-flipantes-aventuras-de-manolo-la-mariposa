using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Match.Tests
{
    internal sealed class RecordingMatchStateRepository : IMatchStateRepository
    {
        private readonly Lock _stateLock = new Lock();
        private readonly List<MatchState> _savedStates = new List<MatchState>();
        private readonly List<int> _deletedMatchIds = new List<int>();

        internal List<MatchState> StoredStates { get; } = new List<MatchState>();

        internal IReadOnlyList<MatchState> SavedStates
        {
            get
            {
                List<MatchState> savedStates;
                lock (_stateLock)
                {
                    savedStates = _savedStates.ToList();
                }

                return savedStates;
            }
        }

        internal IReadOnlyList<int> DeletedMatchIds
        {
            get
            {
                List<int> deletedMatchIds;
                lock (_stateLock)
                {
                    deletedMatchIds = _deletedMatchIds.ToList();
                }

                return deletedMatchIds;
            }
        }

        public Task SaveAsync(MatchState savedState)
        {
            lock (_stateLock)
            {
                _savedStates.Add(savedState);
            }

            Task saving = Task.CompletedTask;

            return saving;
        }

        public Task DeleteAsync(int matchId)
        {
            lock (_stateLock)
            {
                _deletedMatchIds.Add(matchId);
            }

            Task deleting = Task.CompletedTask;

            return deleting;
        }

        public Task<List<MatchState>> ListAsync()
        {
            Task<List<MatchState>> listing = Task.FromResult(StoredStates.ToList());

            return listing;
        }
    }
}
