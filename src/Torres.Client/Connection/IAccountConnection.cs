using System.Threading.Tasks;

using Game.Contracts;

namespace Torres.Client.Connection
{
    internal interface IAccountConnection
    {
        Task<LoginResult> LoginAsync(string username, string password);

        Task<GuestAccessResult> EnterAsGuestAsync(string guestAlias);

        Task<RegistrationResult> RequestRegistrationCodeAsync(string username, string email, string password);

        Task<RegistrationConfirmationResult> ConfirmRegistrationAsync(string email, string code);

        Task<RecoveryCodeRequestResult> RequestRecoveryCodeAsync(string email);

        Task<RecoveryCodeVerificationResult> VerifyRecoveryCodeAsync(string email, string code);

        Task<PasswordResetResult> ResetPasswordAsync(string email, string code, string newPassword);

        void ReleaseChannel();
    }
}
