using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace Game.Persistence
{
    public sealed class RoomInvitationRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public RoomInvitationRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            ArgumentNullException.ThrowIfNull(contextFactory);

            _contextFactory = contextFactory;
        }

        public Task<InvitationCreationOutcome> CreateAsync(RoomInvitationRecord newInvitation)
        {
            ArgumentNullException.ThrowIfNull(newInvitation);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newInvitation.RoomId);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newInvitation.InviterPlayerId);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newInvitation.InvitedPlayerId);
            ArgumentException.ThrowIfNullOrWhiteSpace(newInvitation.Channel);

            Task<InvitationCreationOutcome> creation = StoreAsync(newInvitation);

            return creation;
        }

        public Task<List<RoomInvitationRecord>> ListForInviteeAsync(int invitedPlayerId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(invitedPlayerId);

            Task<List<RoomInvitationRecord>> invitationsReading = ReadForInviteeAsync(invitedPlayerId);

            return invitationsReading;
        }

        public Task<List<int>> ListInviteeIdsAsync(int roomId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomId);

            Task<List<int>> inviteeIdsReading = ReadInviteeIdsAsync(roomId);

            return inviteeIdsReading;
        }

        public Task<bool> ExistsAsync(int roomId, int invitedPlayerId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomId);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(invitedPlayerId);

            Task<bool> existenceReading = ReadExistenceAsync(roomId, invitedPlayerId);

            return existenceReading;
        }

        public Task<bool> DeleteAsync(int roomId, int invitedPlayerId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomId);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(invitedPlayerId);

            Task<bool> deletion = RemoveAsync(roomId, invitedPlayerId);

            return deletion;
        }

        private async Task<InvitationCreationOutcome> StoreAsync(RoomInvitationRecord newInvitation)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
            databaseContext.RoomInvitationRecords.Add(newInvitation);
            InvitationCreationOutcome creationOutcome = InvitationCreationOutcome.Created;
            try
            {
                await databaseContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                creationOutcome = InvitationCreationOutcome.AlreadyPending;
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
            {
                creationOutcome = InvitationCreationOutcome.PlayerMissing;
            }

            return creationOutcome;
        }

        private async Task<List<RoomInvitationRecord>> ReadForInviteeAsync(int invitedPlayerId)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            List<RoomInvitationRecord> invitations = await databaseContext.RoomInvitationRecords
                .AsNoTracking()
                .Where(storedInvitation => storedInvitation.InvitedPlayerId == invitedPlayerId)
                .OrderByDescending(storedInvitation => storedInvitation.CreatedAt)
                .ToListAsync();

            return invitations;
        }

        private async Task<List<int>> ReadInviteeIdsAsync(int roomId)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            List<int> inviteeIds = await databaseContext.RoomInvitationRecords
                .Where(storedInvitation => storedInvitation.RoomId == roomId)
                .Select(storedInvitation => storedInvitation.InvitedPlayerId)
                .ToListAsync();

            return inviteeIds;
        }

        private async Task<bool> ReadExistenceAsync(int roomId, int invitedPlayerId)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            bool exists = await databaseContext.RoomInvitationRecords
                .AnyAsync(storedInvitation => (storedInvitation.RoomId == roomId) && (storedInvitation.InvitedPlayerId == invitedPlayerId));

            return exists;
        }

        private async Task<bool> RemoveAsync(int roomId, int invitedPlayerId)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            int deletedCount = await databaseContext.RoomInvitationRecords
                .Where(storedInvitation => (storedInvitation.RoomId == roomId) && (storedInvitation.InvitedPlayerId == invitedPlayerId))
                .ExecuteDeleteAsync();
            bool wasDeleted = deletedCount > 0;

            return wasDeleted;
        }
    }
}
