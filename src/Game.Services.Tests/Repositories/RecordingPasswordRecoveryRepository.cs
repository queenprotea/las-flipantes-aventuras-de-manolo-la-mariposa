using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Services.Tests
{
    internal sealed class RecordingPasswordRecoveryRepository : IPasswordRecoveryRepository
    {
        internal Dictionary<int, PasswordRecovery> RecoveriesByPlayerId { get; } = new Dictionary<int, PasswordRecovery>();

        internal Dictionary<int, string> NewPasswordHashesByPlayerId { get; } = new Dictionary<int, string>();

        public Task ReplaceAsync(PasswordRecovery newRecovery)
        {
            RecoveriesByPlayerId[newRecovery.PlayerId] = newRecovery;
            Task replacing = Task.CompletedTask;

            return replacing;
        }

        public Task<PasswordRecovery?> FindAsync(int playerId)
        {
            RecoveriesByPlayerId.TryGetValue(playerId, out PasswordRecovery? storedRecovery);
            Task<PasswordRecovery?> finding = Task.FromResult(storedRecovery);

            return finding;
        }

        public Task<bool> CompleteAsync(int playerId, string newPasswordHash)
        {
            bool wasCompleted = RecoveriesByPlayerId.Remove(playerId);
            if (wasCompleted)
            {
                NewPasswordHashesByPlayerId[playerId] = newPasswordHash;
            }

            Task<bool> completion = Task.FromResult(wasCompleted);

            return completion;
        }
    }
}
