using System;
using System.Globalization;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Persistence;

using log4net;

using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace Game.Services
{
    public sealed class AccountRegistration
    {
        private const string RegistrationReadFailedMessage = "The database could not be read while a registration code was requested.";
        private const string AccountWriteFailedMessage = "The database could not be written while a registration was confirmed; the registration stays pending.";
        private const string RegistrationCodeSentMessage = "A registration code was sent.";
        private const string AccountCreatedMessage = "Account {0} was created after its email was confirmed.";
        private const string NotStartedMessage = "The verification code lifetime was not set; Start must run before accounts are registered.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(AccountRegistration));

        private readonly IPlayerRepository _playerRepository;
        private readonly IEmailSender _emailSender;
        private readonly PendingRegistrationRegistry _pendingRegistrationRegistry;
        private readonly SessionTokenIssuer _sessionTokenIssuer;
        private readonly TimeProvider _timeProvider;
        private readonly EmailSendingPacer _emailSendingPacer;
        private int? _codeLifetimeMinutes;

        public AccountRegistration(IPlayerRepository playerRepository, IEmailSender emailSender, PendingRegistrationRegistry pendingRegistrationRegistry, SessionTokenIssuer sessionTokenIssuer, TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(playerRepository);
            ArgumentNullException.ThrowIfNull(emailSender);
            ArgumentNullException.ThrowIfNull(pendingRegistrationRegistry);
            ArgumentNullException.ThrowIfNull(sessionTokenIssuer);
            ArgumentNullException.ThrowIfNull(timeProvider);

            _playerRepository = playerRepository;
            _emailSender = emailSender;
            _pendingRegistrationRegistry = pendingRegistrationRegistry;
            _sessionTokenIssuer = sessionTokenIssuer;
            _timeProvider = timeProvider;
            _emailSendingPacer = new EmailSendingPacer(timeProvider);
        }

        public void Start(int codeLifetimeMinutes)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(codeLifetimeMinutes);

            _codeLifetimeMinutes = codeLifetimeMinutes;
        }

        internal async Task<RegistrationResult> RequestCodeAsync(string username, string email, string password)
        {
            RegistrationResult requestResult;
            if (!UsernameRules.IsValid(username))
            {
                requestResult = RegistrationResult.InvalidUsername;
            }
            else if (!EmailRules.IsValid(email))
            {
                requestResult = RegistrationResult.InvalidEmail;
            }
            else if (!PasswordRules.IsValid(password))
            {
                requestResult = RegistrationResult.InvalidPassword;
            }
            else
            {
                requestResult = await ReserveOrReportAsync(username, email, password);
            }

            return requestResult;
        }

        internal async Task<RegistrationConfirmationResult> ConfirmAsync(string email, string code)
        {
            var confirmation = new RegistrationConfirmationResult { Status = RegistrationConfirmationStatus.InvalidCode };
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            {
                return confirmation;
            }

            PendingRegistration? registration = _pendingRegistrationRegistry.Redeem(email.Trim(), code.Trim());
            if (registration is not null)
            {
                confirmation = await CreateAccountAsync(registration);
            }

            return confirmation;
        }

        private async Task<RegistrationResult> ReserveOrReportAsync(string username, string email, string password)
        {
            RegistrationResult reservationResult;
            try
            {
                reservationResult = await ReserveAsync(username, email, password);
            }
            catch (NpgsqlException exception)
            {
                _log.Error(RegistrationReadFailedMessage, exception);
                _emailSendingPacer.Release(email);
                reservationResult = RegistrationResult.DatabaseUnavailable;
            }

            return reservationResult;
        }

        private async Task<RegistrationResult> ReserveAsync(string username, string email, string password)
        {
            RegistrationResult? takenField = await FindTakenFieldAsync(username, email);
            RegistrationResult reservationResult;
            if (takenField is RegistrationResult rejectedField)
            {
                reservationResult = rejectedField;
            }
            else if (!_emailSendingPacer.Admit(email))
            {
                reservationResult = RegistrationResult.TooManyRequests;
            }
            else
            {
                reservationResult = await IssueCodeAsync(username, email, password);
            }

            return reservationResult;
        }

        private async Task<RegistrationResult?> FindTakenFieldAsync(string username, string email)
        {
            Player? usernameOwner = await _playerRepository.FindByUsernameAsync(username);
            Player? emailOwner = await _playerRepository.FindByEmailAsync(email);
            RegistrationResult? takenField = null;
            if (usernameOwner is not null)
            {
                takenField = RegistrationResult.UsernameTaken;
            }
            else if (emailOwner is not null)
            {
                takenField = RegistrationResult.EmailTaken;
            }

            return takenField;
        }

        private async Task<RegistrationResult> IssueCodeAsync(string username, string email, string password)
        {
            int codeLifetimeMinutes = RequireCodeLifetimeMinutes();
            DateTimeOffset expiresAt = _timeProvider.GetUtcNow().AddMinutes(codeLifetimeMinutes);
            var registration = new PendingRegistration(username, email, PasswordRules.Hash(password), VerificationCodeIssuer.Issue(), expiresAt);
            EmailMessage codeEmail = EmailComposer.ComposeRegistrationCode(email, registration.Code, codeLifetimeMinutes);
            bool wasSent = await _emailSender.SendAsync(codeEmail);
            RegistrationResult sendResult = RegistrationResult.MailDeliveryFailed;
            if (wasSent)
            {
                _pendingRegistrationRegistry.Hold(registration);
                _log.Info(RegistrationCodeSentMessage);
                sendResult = RegistrationResult.CodeSent;
            }
            else
            {
                _emailSendingPacer.Release(email);
            }

            return sendResult;
        }

        private async Task<RegistrationConfirmationResult> CreateAccountAsync(PendingRegistration registration)
        {
            var confirmation = new RegistrationConfirmationResult { Status = RegistrationConfirmationStatus.DatabaseUnavailable };
            try
            {
                PlayerCreationOutcome creationOutcome = await _playerRepository.CreateAsync(registration.Username, registration.Email, registration.PasswordHash);
                confirmation.Status = ToConfirmationStatus(creationOutcome);
                await AddIdentityAsync(confirmation, registration.Username);
            }
            catch (NpgsqlException exception)
            {
                HoldAfterWriteFailure(registration, exception);
            }
            catch (DbUpdateException exception)
            {
                HoldAfterWriteFailure(registration, exception);
            }

            return confirmation;
        }

        private void HoldAfterWriteFailure(PendingRegistration registration, Exception exception)
        {
            _log.Error(AccountWriteFailedMessage, exception);
            _pendingRegistrationRegistry.Hold(registration);
        }

        private async Task AddIdentityAsync(RegistrationConfirmationResult confirmation, string username)
        {
            Player? createdPlayer = confirmation.Status == RegistrationConfirmationStatus.Created
                ? await _playerRepository.FindByUsernameAsync(username)
                : null;
            if (createdPlayer is not null)
            {
                _log.InfoFormat(CultureInfo.InvariantCulture, AccountCreatedMessage, createdPlayer.PlayerId);
                confirmation.Player = new PlayerIdentity
                {
                    PlayerId = createdPlayer.PlayerId,
                    Username = createdPlayer.Username,
                    Email = createdPlayer.Email,
                };
                confirmation.SessionToken = _sessionTokenIssuer.Issue(createdPlayer.PlayerId, createdPlayer.Username);
            }
        }

        private int RequireCodeLifetimeMinutes()
        {
            int codeLifetimeMinutes = _codeLifetimeMinutes ?? throw new InvalidOperationException(NotStartedMessage);

            return codeLifetimeMinutes;
        }

        private static RegistrationConfirmationStatus ToConfirmationStatus(PlayerCreationOutcome creationOutcome)
        {
            RegistrationConfirmationStatus confirmationStatus = creationOutcome switch
            {
                PlayerCreationOutcome.Created => RegistrationConfirmationStatus.Created,
                PlayerCreationOutcome.UsernameTaken => RegistrationConfirmationStatus.UsernameTaken,
                PlayerCreationOutcome.EmailTaken => RegistrationConfirmationStatus.EmailTaken,
                _ => throw new ArgumentOutOfRangeException(nameof(creationOutcome)),
            };

            return confirmationStatus;
        }
    }
}
