using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Connection;

namespace Torres.Client.Tests
{
    internal sealed class RecordingAccountConnection : IAccountConnection
    {
        internal Task<LoginResult> LoginReply { get; set; } = Task.FromResult(new LoginResult());

        internal Task<GuestAccessResult> EnterAsGuestReply { get; set; } = Task.FromResult(new GuestAccessResult());

        internal Task<RegistrationResult> RequestRegistrationCodeReply { get; set; } = Task.FromResult(default(RegistrationResult));

        internal Task<RegistrationConfirmationResult> ConfirmRegistrationReply { get; set; } = Task.FromResult(new RegistrationConfirmationResult());

        internal Task<RecoveryCodeRequestResult> RequestRecoveryCodeReply { get; set; } = Task.FromResult(default(RecoveryCodeRequestResult));

        internal Task<RecoveryCodeVerificationResult> VerifyRecoveryCodeReply { get; set; } = Task.FromResult(default(RecoveryCodeVerificationResult));

        internal Task<PasswordResetResult> ResetPasswordReply { get; set; } = Task.FromResult(default(PasswordResetResult));

        internal List<string> CalledMethods { get; } = new List<string>();

        public Task<LoginResult> LoginAsync(string username, string password)
        {
            CalledMethods.Add(nameof(LoginAsync));

            return LoginReply;
        }

        public Task<GuestAccessResult> EnterAsGuestAsync(string guestAlias)
        {
            CalledMethods.Add(nameof(EnterAsGuestAsync));

            return EnterAsGuestReply;
        }

        public Task<RegistrationResult> RequestRegistrationCodeAsync(string username, string email, string password)
        {
            CalledMethods.Add(nameof(RequestRegistrationCodeAsync));

            return RequestRegistrationCodeReply;
        }

        public Task<RegistrationConfirmationResult> ConfirmRegistrationAsync(string email, string code)
        {
            CalledMethods.Add(nameof(ConfirmRegistrationAsync));

            return ConfirmRegistrationReply;
        }

        public Task<RecoveryCodeRequestResult> RequestRecoveryCodeAsync(string email)
        {
            CalledMethods.Add(nameof(RequestRecoveryCodeAsync));

            return RequestRecoveryCodeReply;
        }

        public Task<RecoveryCodeVerificationResult> VerifyRecoveryCodeAsync(string email, string code)
        {
            CalledMethods.Add(nameof(VerifyRecoveryCodeAsync));

            return VerifyRecoveryCodeReply;
        }

        public Task<PasswordResetResult> ResetPasswordAsync(string email, string code, string newPassword)
        {
            CalledMethods.Add(nameof(ResetPasswordAsync));

            return ResetPasswordReply;
        }

        public void ReleaseChannel()
        {
            CalledMethods.Add(nameof(ReleaseChannel));
        }
    }
}
