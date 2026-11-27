using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Services.Tests
{
    internal sealed class RecordingRoomRecordRepository : IRoomRecordRepository
    {
        private const int NoRoomsClosed = 0;

        private readonly Lock _recordLock = new Lock();
        private readonly List<int> _closedRoomIds = new List<int>();

        private int _lastRoomId;
        private int _creationAttempts;

        internal int RefusedCreations { get; set; }

        internal int CreationAttempts
        {
            get
            {
                lock (_recordLock)
                {
                    return _creationAttempts;
                }
            }
        }

        internal IReadOnlyList<int> ClosedRoomIds
        {
            get
            {
                IReadOnlyList<int> closedRoomIds;
                lock (_recordLock)
                {
                    closedRoomIds = _closedRoomIds.ToArray();
                }

                return closedRoomIds;
            }
        }

        public Task<int?> CreateAsync(string code, string visibility)
        {
            int? createdRoomId = null;
            lock (_recordLock)
            {
                _creationAttempts++;
                if (_creationAttempts > RefusedCreations)
                {
                    _lastRoomId++;
                    createdRoomId = _lastRoomId;
                }
            }

            Task<int?> creation = Task.FromResult(createdRoomId);

            return creation;
        }

        public Task CloseAsync(int roomId, DateTime closedAtUtc)
        {
            lock (_recordLock)
            {
                _closedRoomIds.Add(roomId);
            }

            Task closing = Task.CompletedTask;

            return closing;
        }

        public Task<int> CloseAllOpenAsync(DateTime closedAtUtc)
        {
            Task<int> closing = Task.FromResult(NoRoomsClosed);

            return closing;
        }
    }
}
