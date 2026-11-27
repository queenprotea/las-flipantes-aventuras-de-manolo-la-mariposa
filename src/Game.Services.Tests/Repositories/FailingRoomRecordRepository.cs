using System;
using System.Threading.Tasks;

using Game.Persistence;

using Npgsql;

namespace Game.Services.Tests
{
    internal sealed class FailingRoomRecordRepository : IRoomRecordRepository
    {
        private const string FailureMessage = "The test database is down.";

        public Task<int?> CreateAsync(string code, string visibility)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task CloseAsync(int roomId, DateTime closedAtUtc)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<int> CloseAllOpenAsync(DateTime closedAtUtc)
        {
            throw new NpgsqlException(FailureMessage);
        }
    }
}
