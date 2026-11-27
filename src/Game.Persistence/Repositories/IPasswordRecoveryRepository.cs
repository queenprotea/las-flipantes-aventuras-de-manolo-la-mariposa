using System.Threading.Tasks;

namespace Game.Persistence
{
    public interface IPasswordRecoveryRepository
    {
        Task ReplaceAsync(PasswordRecovery newRecovery);

        Task<PasswordRecovery?> FindAsync(int playerId);

        Task<bool> CompleteAsync(int playerId, string newPasswordHash);
    }
}
