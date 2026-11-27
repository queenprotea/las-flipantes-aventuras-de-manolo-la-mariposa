using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Game.Persistence
{
    public sealed class PasswordRecoveryRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public PasswordRecoveryRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public Task ReplaceAsync(PasswordRecovery newRecovery)
        {
            Task replacement = StoreAsync(newRecovery);

            return replacement;
        }

        public Task<PasswordRecovery?> FindAsync(int playerId)
        {
            Task<PasswordRecovery?> recoveryReading = ReadAsync(playerId);

            return recoveryReading;
        }

        public Task<bool> CompleteAsync(int playerId, string newPasswordHash)
        {
            Task<bool> completion = WriteCompletionAsync(playerId, newPasswordHash);

            return completion;
        }

        private async Task StoreAsync(PasswordRecovery newRecovery)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
            PasswordRecovery? storedRecovery = await databaseContext.PasswordRecoveries
                .SingleOrDefaultAsync(candidate => candidate.PlayerId == newRecovery.PlayerId);
            if (storedRecovery is null)
            {
                databaseContext.PasswordRecoveries.Add(newRecovery);
            }
            else
            {
                storedRecovery.Code = newRecovery.Code;
                storedRecovery.CreatedAt = newRecovery.CreatedAt;
            }

            await databaseContext.SaveChangesAsync();
        }

        private async Task<PasswordRecovery?> ReadAsync(int playerId)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            PasswordRecovery? recovery = await databaseContext.PasswordRecoveries
                .AsNoTracking()
                .SingleOrDefaultAsync(storedRecovery => storedRecovery.PlayerId == playerId);

            return recovery;
        }

        private async Task<bool> WriteCompletionAsync(int playerId, string newPasswordHash)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
            PasswordRecovery? pendingRecovery = await databaseContext.PasswordRecoveries
                .SingleOrDefaultAsync(candidate => candidate.PlayerId == playerId);
            Player? storedPlayer = await databaseContext.Players
                .SingleOrDefaultAsync(candidate => candidate.PlayerId == playerId);
            if ((pendingRecovery is null) || (storedPlayer is null))
            {
                return false;
            }

            storedPlayer.PasswordHash = newPasswordHash;
            databaseContext.PasswordRecoveries.Remove(pendingRecovery);
            await databaseContext.SaveChangesAsync();

            return true;
        }
    }
}
