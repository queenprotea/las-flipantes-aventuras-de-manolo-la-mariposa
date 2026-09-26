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

            if (invalidField is not null)
            {
                return invalidField.Value;
            }
            
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

        private static RegistrationResult? FindInvalidField(string username, string email, string password)
        {
            RegistrationResult? result = null;

            if ((username is null) || !_usernamePattern.IsMatch(username))
            {
                result = RegistrationResult.InvalidUsername;
                return result;
            }
            else if ((email is null) || (email.IndexOf('@') <= 0) || (email.Length > LongestEmail))
            {
                result = RegistrationResult.InvalidEmail;
                return result;
            }
            else if ((password is null) || !_passwordPattern.IsMatch(password))
            {
                result = RegistrationResult.InvalidPassword;
                return result;
            }

            return result;
        }
    }
}
