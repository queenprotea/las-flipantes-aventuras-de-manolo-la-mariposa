using System.ServiceModel;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Threading;

namespace Torres.Client.Connection
{
    internal sealed class AccountConnection : IAccountConnection
    {
        private readonly ChannelFactory<IAccountService> _channelFactory;

        private IAccountService? _openChannel;

        internal AccountConnection(ChannelFactory<IAccountService> channelFactory)
        {
            _channelFactory = channelFactory;
        }

        public Task<LoginResult> LoginAsync(string username, string password)
        {
            Task<LoginResult> login = OpenChannel().LoginAsync(username, password);

            return login;
        }

        public Task<GuestAccessResult> EnterAsGuestAsync(string guestAlias)
        {
            IAccountService accountChannel = OpenChannel();
            Task<GuestAccessResult> guestAccess = new BackgroundCall<string, GuestAccessResult>(accountChannel.EnterAsGuest, guestAlias).StartAsync();

            return guestAccess;
        }

        public Task<RegistrationResult> RequestRegistrationCodeAsync(string username, string email, string password)
        {
            Task<RegistrationResult> codeRequest = OpenChannel().RequestRegistrationCodeAsync(username, email, password);

            return codeRequest;
        }

        public Task<RegistrationConfirmationResult> ConfirmRegistrationAsync(string email, string code)
        {
            Task<RegistrationConfirmationResult> confirmation = OpenChannel().ConfirmRegistrationAsync(email, code);

            return confirmation;
        }

        public Task<RecoveryCodeRequestResult> RequestRecoveryCodeAsync(string email)
        {
            Task<RecoveryCodeRequestResult> codeRequest = OpenChannel().RequestRecoveryCodeAsync(email);

            return codeRequest;
        }

        public Task<RecoveryCodeVerificationResult> VerifyRecoveryCodeAsync(string email, string code)
        {
            Task<RecoveryCodeVerificationResult> codeVerification = OpenChannel().VerifyRecoveryCodeAsync(email, code);

            return codeVerification;
        }

        public Task<PasswordResetResult> ResetPasswordAsync(string email, string code, string newPassword)
        {
            Task<PasswordResetResult> passwordReset = OpenChannel().ResetPasswordAsync(email, code, newPassword);

            return passwordReset;
        }

        public void ReleaseChannel()
        {
            if (_openChannel is ICommunicationObject usedChannel)
            {
                usedChannel.Abort();
            }

            _openChannel = null;
        }

        private IAccountService OpenChannel()
        {
            ReleaseChannel();
            IAccountService openedChannel = _channelFactory.CreateChannel();
            _openChannel = openedChannel;

            return openedChannel;
        }
    }
}
