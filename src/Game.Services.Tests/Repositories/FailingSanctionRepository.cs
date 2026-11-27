using System;
using System.Threading.Tasks;

using Game.Persistence;

using Npgsql;

namespace Game.Services.Tests
{
    internal sealed class FailingSanctionRepository : ISanctionRepository
    {
        private const string FailureMessage = "The test database is down.";

        public Task<Sanction?> FindInForceAsync(int playerId, DateTime nowUtc)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<bool> HasPermanentAsync(int playerId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<DateTime?> FindLatestThresholdAsync(int playerId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<int> CountAsync(int playerId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task CreateAsync(Sanction newSanction)
        {
            throw new NpgsqlException(FailureMessage);
        }
    }
}
