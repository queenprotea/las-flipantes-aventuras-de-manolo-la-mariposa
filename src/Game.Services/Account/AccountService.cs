using System.Globalization;
using System.Threading.Tasks;

using CoreWCF;

using Game.Contracts;
using Game.Persistence;

using log4net;

using Npgsql;

namespace Game.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public sealed class AccountService : IAccountService
    {
        private const string LoginReadFailedMessage = "The database could not be read during a log in.";
        private const string PlayerLoggedInMessage = "Player {0} logged in.";
        private const string BannedLoginRefusedMessage = "Player {0} was refused at log in because of a permanent ban.";
        private const string GuestEnteredMessage = "Guest {0} entered.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(AccountService));

        private readonly PlayerRepository _playerRepository;
        private readonly SanctionRepository _sanctionRepository;
        private readonly SessionTokenIssuer _sessionTokenIssuer;
        private readonly GuestIdIssuer _guestIdIssuer;
        private readonly AccountRegistration _accountRegistration;
        private readonly AccessRecovery _accessRecovery;

        public AccountService(PlayerRepository playerRepository, SanctionRepository sanctionRepository, SessionTokenIssuer sessionTokenIssuer, GuestIdIssuer guestIdIssuer, AccountRegistration accountRegistration, AccessRecovery accessRecovery)
        {
            _playerRepository = playerRepository;
            _sanctionRepository = sanctionRepository;
            _sessionTokenIssuer = sessionTokenIssuer;
            _guestIdIssuer = guestIdIssuer;
            _accountRegistration = accountRegistration;
            _accessRecovery = accessRecovery;
        }

        /// <inheritdoc/>
        public Task<RegistrationResult> RequestRegistrationCodeAsync(string username, string email, string password)
        {
            Task<RegistrationResult> codeRequest = _accountRegistration.RequestCodeAsync(username, email, password);

            return codeRequest;
        }

        /// <inheritdoc/>
        public Task<RegistrationConfirmationResult> ConfirmRegistrationAsync(string email, string code)
        {
            Task<RegistrationConfirmationResult> confirmation = _accountRegistration.ConfirmAsync(email, code);

            return confirmation;
        }

        /// <inheritdoc/>
        public Task<RecoveryCodeRequestResult> RequestRecoveryCodeAsync(string email)
        {
            Task<RecoveryCodeRequestResult> codeRequest = _accessRecovery.RequestCodeAsync(email);

            return codeRequest;
        }

        /// <inheritdoc/>
        public Task<RecoveryCodeVerificationResult> VerifyRecoveryCodeAsync(string email, string code)
        {
            Task<RecoveryCodeVerificationResult> verification = _accessRecovery.VerifyCodeAsync(email, code);

            return verification;
        }

        /// <inheritdoc/>
        public Task<PasswordResetResult> ResetPasswordAsync(string email, string code, string newPassword)
        {
            Task<PasswordResetResult> reset = _accessRecovery.ResetPasswordAsync(email, code, newPassword);

            return reset;
        }

        /// <inheritdoc/>
        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            LoginResult loginResult;
            if (!AreUsableCredentials(username, password))
            {
                loginResult = new LoginResult { Status = LoginStatus.InvalidCredentials };
            }
            else
            {
                loginResult = await AuthenticateAsync(username, password);
            }

            return loginResult;
        }

        /// <inheritdoc/>
        public GuestAccessResult EnterAsGuest(string guestAlias)
        {
            string trimmedGuestAlias = (guestAlias ?? string.Empty).Trim();
            GuestAccessStatus? aliasRejection = FindAliasRejection(trimmedGuestAlias);
            if (aliasRejection is GuestAccessStatus rejectedStatus)
            {
                var rejectedResult = new GuestAccessResult { Status = rejectedStatus };
                return rejectedResult;
            }

            int guestId = _guestIdIssuer.Issue();
            _log.InfoFormat(CultureInfo.InvariantCulture, GuestEnteredMessage, guestId);
            var enteredResult = new GuestAccessResult
            {
                Status = GuestAccessStatus.Entered,
                SessionToken = _sessionTokenIssuer.IssueForGuest(guestId, trimmedGuestAlias),
            };

            return enteredResult;
        }

        private static bool AreUsableCredentials(string? username, string? password)
        {
            bool hasUsableUsername = !string.IsNullOrWhiteSpace(username) && (username.Length <= AccountLimits.LongestUsername);
            bool hasUsablePassword = !string.IsNullOrEmpty(password) && (password.Length <= AccountLimits.LongestPassword);
            bool areUsable = hasUsableUsername && hasUsablePassword;

            return areUsable;
        }

        private async Task<LoginResult> AuthenticateAsync(string username, string password)
        {
            LoginResult loginResult;
            try
            {
                Player? player = await _playerRepository.FindByUsernameAsync(username);
                loginResult = await AdmitAsync(player, password);
            }
            catch (NpgsqlException exception)
            {
                _log.Error(LoginReadFailedMessage, exception);
                loginResult = new LoginResult { Status = LoginStatus.DatabaseUnavailable };
            }

            return loginResult;
        }

        private async Task<LoginResult> AdmitAsync(Player? player, string password)
        {
            LoginResult loginResult;
            if ((player is null) || !BCrypt.Net.BCrypt.Verify(password, player.PasswordHash))
            {
                loginResult = new LoginResult { Status = LoginStatus.InvalidCredentials };
            }
            else if (await _sanctionRepository.HasPermanentAsync(player.PlayerId))
            {
                _log.InfoFormat(CultureInfo.InvariantCulture, BannedLoginRefusedMessage, player.PlayerId);
                loginResult = new LoginResult { Status = LoginStatus.AccountBanned };
            }
            else
            {
                loginResult = BuildLoggedInResult(player);
            }

            return loginResult;
        }

        private LoginResult BuildLoggedInResult(Player player)
        {
            _log.InfoFormat(CultureInfo.InvariantCulture, PlayerLoggedInMessage, player.PlayerId);
            var loggedInResult = new LoginResult
            {
                Status = LoginStatus.LoggedIn,
                Player = new PlayerIdentity
                {
                    PlayerId = player.PlayerId,
                    Username = player.Username,
                    Email = player.Email,
                },
                SessionToken = _sessionTokenIssuer.Issue(player.PlayerId, player.Username),
            };

            return loggedInResult;
        }

        private static GuestAccessStatus? FindAliasRejection(string trimmedGuestAlias)
        {
            GuestAccessStatus? aliasRejection = null;
            if (trimmedGuestAlias.Length == 0)
            {
                aliasRejection = GuestAccessStatus.AliasRequired;
            }
            else if (trimmedGuestAlias.Length > AccountLimits.LongestGuestAlias)
            {
                aliasRejection = GuestAccessStatus.AliasTooLong;
            }

            return aliasRejection;
        }
    }
}
