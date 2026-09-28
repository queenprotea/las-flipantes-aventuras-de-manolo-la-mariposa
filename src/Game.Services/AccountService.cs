using System;
using System.Text.RegularExpressions;
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
        private const int PasswordWorkFactor = 11;
        private const int LongestEmail = 254;

        private static readonly ILog _log = LogManager.GetLogger(typeof(AccountService));
        private static readonly Regex _usernamePattern = new Regex("^[A-Za-z0-9_]{3,20}$");
        private static readonly Regex _passwordPattern =
            new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,20}$");

        private readonly PlayerRepository _playerRepository;

        public AccountService(PlayerRepository playerRepository)
        {
            ArgumentNullException.ThrowIfNull(playerRepository);

            _playerRepository = playerRepository;
        }

        public async Task<RegistrationResult> RegisterAsync(string username, string email, string password)
        {
            RegistrationResult? invalidField = FindInvalidField(username, email, password);

            return invalidField ?? await CreateAccountAsync(username, email, password);
        }

        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                return new LoginResult { Status = LoginStatus.InvalidCredentials };
            }

            return await AuthenticateAsync(username, password);
        }

        private async Task<RegistrationResult> CreateAccountAsync(string username, string email, string password)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password, PasswordWorkFactor);
            PlayerCreationOutcome outcome;
            try
            {
                outcome = await _playerRepository.CreateAsync(username, email, passwordHash);
            }
            catch (NpgsqlException exception)
            {
                _log.Error("The database could not be written during a registration.", exception);
                return RegistrationResult.DatabaseUnavailable;
            }

            return ToRegistrationResult(outcome);
        }

        private static RegistrationResult ToRegistrationResult(PlayerCreationOutcome outcome)
        {
            switch (outcome)
            {
                case PlayerCreationOutcome.Created:
                    _log.Info("An account was created.");
                    return RegistrationResult.Created;

                case PlayerCreationOutcome.UsernameTaken:
                    return RegistrationResult.UsernameTaken;

                case PlayerCreationOutcome.EmailTaken:
                    return RegistrationResult.EmailTaken;

                default:
                    throw new InvalidOperationException($"Unknown creation outcome {outcome}.");
            }
        }

        private async Task<LoginResult> AuthenticateAsync(string username, string password)
        {
            PlayerAccount? account;
            try
            {
                account = await _playerRepository.FindByUsernameAsync(username);
            }
            catch (NpgsqlException exception)
            {
                _log.Error("The database could not be read during a log in.", exception);
                return new LoginResult { Status = LoginStatus.DatabaseUnavailable };
            }

            return VerifyCredentials(account, password);
        }

        private static LoginResult VerifyCredentials(PlayerAccount? account, string password)
        {
            if ((account is null) || !BCrypt.Net.BCrypt.Verify(password, account.PasswordHash))
            {
                return new LoginResult { Status = LoginStatus.InvalidCredentials };
            }

            _log.InfoFormat("Player {0} logged in.", account.PlayerId);

            return new LoginResult
            {
                Status = LoginStatus.LoggedIn,
                Player = new PlayerIdentity
                {
                    PlayerId = account.PlayerId,
                    Username = account.Username,
                    Email = account.Email,
                },
            };
        }

        private static RegistrationResult? FindInvalidField(string username, string email, string password)
        {
            RegistrationResult? invalidField = null;
            if ((username is null) || !_usernamePattern.IsMatch(username))
            {
                invalidField = RegistrationResult.InvalidUsername;
            }
            else if ((email is null) || (email.IndexOf('@') <= 0) || (email.Length > LongestEmail))
            {
                invalidField = RegistrationResult.InvalidEmail;
            }
            else if ((password is null) || !_passwordPattern.IsMatch(password))
            {
                invalidField = RegistrationResult.InvalidPassword;
            }

            return invalidField;
        }
    }
}
