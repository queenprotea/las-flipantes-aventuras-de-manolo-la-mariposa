using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace Game.Persistence
{
    public sealed class RoomRecordRepository
    {
        private const string OpenCodeIndex = "ux_room_code_open";
        private const string UnexpectedViolationMessage = "The room row broke the unique index {0}.";

        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public RoomRecordRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            ArgumentNullException.ThrowIfNull(contextFactory);

            _contextFactory = contextFactory;
        }

        public async Task<int?> CreateAsync(string code, string visibility)
        {
            ArgumentNullException.ThrowIfNull(code);
            ArgumentNullException.ThrowIfNull(visibility);

            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
            var newRoom = new RoomRecord
            {
                Code = code,
                Visibility = visibility,
            };
            databaseContext.RoomRecords.Add(newRoom);
            int? createdRoomId = null;
            try
            {
                await databaseContext.SaveChangesAsync();
                createdRoomId = newRoom.RoomId;
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } uniqueViolation)
            {
                RejectUnexpectedViolation(uniqueViolation, exception);
            }

            return createdRoomId;
        }

        public async Task CloseAsync(int roomId, DateTime closedAtUtc)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomId);

            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            await databaseContext.RoomRecords
                .Where(storedRoom => (storedRoom.RoomId == roomId) && !storedRoom.ClosedAt.HasValue)
                .ExecuteUpdateAsync(roomUpdate => roomUpdate.SetProperty(storedRoom => storedRoom.ClosedAt, closedAtUtc));
        }

        public async Task<int> CloseAllOpenAsync(DateTime closedAtUtc)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            int closedCount = await databaseContext.RoomRecords
                .Where(storedRoom => !storedRoom.ClosedAt.HasValue)
                .ExecuteUpdateAsync(roomUpdate => roomUpdate.SetProperty(storedRoom => storedRoom.ClosedAt, closedAtUtc));

            return closedCount;
        }

        private static void RejectUnexpectedViolation(PostgresException uniqueViolation, DbUpdateException exception)
        {
            if (uniqueViolation.ConstraintName == OpenCodeIndex)
            {
                return;
            }

            string violationMessage = string.Format(CultureInfo.InvariantCulture, UnexpectedViolationMessage, uniqueViolation.ConstraintName);
            throw new InvalidOperationException(violationMessage, exception);
        }
    }
}
