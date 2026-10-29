using System;
using System.Globalization;
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
        private const string UsernameFormat = "^[A-Za-z0-9_]{3,20}$";
        private const string PasswordFormat = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,20}$";
        private const string RegistrationWriteFailedMessage = "The database could not be written during a registration.";
        private const string AccountCreatedMessage = "An account was created.";
        private const string UnknownCreationOutcomeMessage = "Unknown creation outcome {0}.";
        private const string LoginReadFailedMessage = "The database could not be read during a log in.";
        private const string PlayerLoggedInMessage = "Player {0} logged in.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(AccountService));
        private static readonly Regex _usernamePattern = new Regex(UsernameFormat);
        private static readonly Regex _passwordPattern =
            new Regex(PasswordFormat);

        private readonly PlayerRepository _playerRepository;
        private readonly SessionTokenIssuer _sessionTokenIssuer;

        public AccountService(PlayerRepository playerRepository, SessionTokenIssuer sessionTokenIssuer)
        {
            ArgumentNullException.ThrowIfNull(playerRepository);
            ArgumentNullException.ThrowIfNull(sessionTokenIssuer);

            _playerRepository = playerRepository;
            _sessionTokenIssuer = sessionTokenIssuer;
        }

        public async Task<RegistrationResult> RegisterAsync(string username, string email, string password)
        {
            RegistrationResult? invalidField = FindInvalidField(username, email, password);
            if (invalidField is RegistrationResult rejectedField)
            {
                return rejectedField;
            }

            RegistrationResult registrationResult = await CreateAccountAsync(username, email, password);

            return registrationResult;
        }

        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            LoginResult loginResult;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                loginResult = new LoginResult { Status = LoginStatus.InvalidCredentials };
            }
            else
            {
                loginResult = await AuthenticateAsync(username, password);
            }

            return loginResult;
        }

        private async Task<RegistrationResult> CreateAccountAsync(string username, string email, string password)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password, PasswordWorkFactor);
            PlayerCreationOutcome creationOutcome;
            try
            {
                creationOutcome = await _playerRepository.CreateAsync(username, email, passwordHash);
            }
            catch (NpgsqlException exception)
            {
                _log.Error(RegistrationWriteFailedMessage, exception);
                return RegistrationResult.DatabaseUnavailable;
            }

            RegistrationResult registrationResult = ToRegistrationResult(creationOutcome);
            if (registrationResult == RegistrationResult.Created)
            {
                _log.Info(AccountCreatedMessage);
            }

            return registrationResult;
        }

        private static RegistrationResult ToRegistrationResult(PlayerCreationOutcome creationOutcome)
        {
            RegistrationResult registrationResult = creationOutcome switch
            {
                PlayerCreationOutcome.Created => RegistrationResult.Created,
                PlayerCreationOutcome.UsernameTaken => RegistrationResult.UsernameTaken,
                PlayerCreationOutcome.EmailTaken => RegistrationResult.EmailTaken,
                _ => throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, UnknownCreationOutcomeMessage, creationOutcome)),
            };

            return registrationResult;
        }

        private async Task<LoginResult> AuthenticateAsync(string username, string password)
        {
            Player? player;
            try
            {
                player = await _playerRepository.FindByUsernameAsync(username);
            }
            catch (NpgsqlException exception)
            {
                _log.Error(LoginReadFailedMessage, exception);
                var unavailableResult = new LoginResult { Status = LoginStatus.DatabaseUnavailable };
                return unavailableResult;
            }

            LoginResult loginResult = VerifyCredentials(player, password);

            return loginResult;
        }

        private LoginResult VerifyCredentials(Player? player, string password)
        {
            if ((player is null) || !BCrypt.Net.BCrypt.Verify(password, player.PasswordHash))
            {
                var rejectedResult = new LoginResult { Status = LoginStatus.InvalidCredentials };
                return rejectedResult;
            }

            _log.InfoFormat(PlayerLoggedInMessage, player.PlayerId);
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
