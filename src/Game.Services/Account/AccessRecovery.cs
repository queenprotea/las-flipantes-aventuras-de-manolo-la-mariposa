using System;
using System.Globalization;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Persistence;

using log4net;

using Npgsql;

namespace Game.Services
{
    public sealed class AccessRecovery
    {
        private const string CodeRequestFailedMessage = "The database could not be used while a recovery code was requested.";
        private const string VerificationFailedMessage = "The database could not be read while a recovery code was verified.";
        private const string ResetFailedMessage = "The database could not be used while a password was reset with a recovery code.";
        private const string RecoveryCodeSentMessage = "A recovery code was sent to the email of player {0}.";
        private const string PasswordResetMessage = "Player {0} set a new password with a recovery code.";
        private const string NotStartedMessage = "The verification code lifetime was not set; Start must run before passwords are recovered.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(AccessRecovery));

        private readonly PlayerRepository _playerRepository;
        private readonly PasswordRecoveryRepository _passwordRecoveryRepository;
        private readonly IEmailSender _emailSender;
        private readonly TimeProvider _timeProvider;
        private readonly EmailSendingPacer _emailSendingPacer;
        private readonly CodeAttemptCounter _codeAttemptCounter = new CodeAttemptCounter();
        private int? _codeLifetimeMinutes;

        public AccessRecovery(PlayerRepository playerRepository, PasswordRecoveryRepository passwordRecoveryRepository, IEmailSender emailSender, TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(playerRepository);
            ArgumentNullException.ThrowIfNull(passwordRecoveryRepository);
            ArgumentNullException.ThrowIfNull(emailSender);
            ArgumentNullException.ThrowIfNull(timeProvider);

            _playerRepository = playerRepository;
            _passwordRecoveryRepository = passwordRecoveryRepository;
            _emailSender = emailSender;
            _timeProvider = timeProvider;
            _emailSendingPacer = new EmailSendingPacer(timeProvider);
        }

        public void Start(int codeLifetimeMinutes)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(codeLifetimeMinutes);

            _codeLifetimeMinutes = codeLifetimeMinutes;
        }

        internal async Task<RecoveryCodeRequestResult> RequestCodeAsync(string email)
        {
            RecoveryCodeRequestResult requestResult;
            if (!EmailRules.IsValid(email))
            {
                requestResult = RecoveryCodeRequestResult.InvalidEmail;
            }
            else if (!_emailSendingPacer.Admit(email))
            {
                requestResult = RecoveryCodeRequestResult.TooManyRequests;
            }
            else
            {
                requestResult = await SendCodeOrReportAsync(email);
            }

            return requestResult;
        }

        internal async Task<RecoveryCodeVerificationResult> VerifyCodeAsync(string email, string code)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            {
                return RecoveryCodeVerificationResult.InvalidCode;
            }

            RecoveryCodeVerificationResult verification;
            try
            {
                Player? verifiedOwner = await FindVerifiedOwnerAsync(email.Trim(), code.Trim());
                verification = verifiedOwner is null ? RecoveryCodeVerificationResult.InvalidCode : RecoveryCodeVerificationResult.Valid;
            }
            catch (NpgsqlException exception)
            {
                _log.Error(VerificationFailedMessage, exception);
                verification = RecoveryCodeVerificationResult.DatabaseUnavailable;
            }

            return verification;
        }

        internal async Task<PasswordResetResult> ResetPasswordAsync(string email, string code, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            {
                return PasswordResetResult.InvalidCode;
            }

            PasswordResetResult resetResult = PasswordResetResult.InvalidPassword;
            if (PasswordRules.IsValid(newPassword))
            {
                resetResult = await ResetOrReportAsync(email.Trim(), code.Trim(), newPassword);
            }

            return resetResult;
        }

        private async Task<RecoveryCodeRequestResult> SendCodeOrReportAsync(string email)
        {
            RecoveryCodeRequestResult requestResult;
            try
            {
                requestResult = await SendCodeAsync(email);
            }
            catch (NpgsqlException exception)
            {
                _log.Error(CodeRequestFailedMessage, exception);
                _emailSendingPacer.Release(email);
                requestResult = RecoveryCodeRequestResult.DatabaseUnavailable;
            }

            return requestResult;
        }

        private async Task<RecoveryCodeRequestResult> SendCodeAsync(string email)
        {
            Player? owner = await _playerRepository.FindByEmailAsync(email);
            RecoveryCodeRequestResult requestResult = RecoveryCodeRequestResult.CodeSent;
            if (owner is not null)
            {
                requestResult = await IssueCodeAsync(owner);
            }

            return requestResult;
        }

        private async Task<RecoveryCodeRequestResult> IssueCodeAsync(Player owner)
        {
            int codeLifetimeMinutes = RequireCodeLifetimeMinutes();
            var newRecovery = new PasswordRecovery
            {
                PlayerId = owner.PlayerId,
                Code = VerificationCodeIssuer.Issue(),
                CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            };
            await _passwordRecoveryRepository.ReplaceAsync(newRecovery);
            _codeAttemptCounter.Forget(owner.PlayerId);

            EmailMessage recoveryEmail = EmailComposer.ComposeRecoveryCode(owner.Email, newRecovery.Code, codeLifetimeMinutes);
            bool wasSent = await _emailSender.SendAsync(recoveryEmail);
            RecoveryCodeRequestResult requestResult = RecoveryCodeRequestResult.MailDeliveryFailed;
            if (wasSent)
            {
                _log.InfoFormat(CultureInfo.InvariantCulture, RecoveryCodeSentMessage, owner.PlayerId);
                requestResult = RecoveryCodeRequestResult.CodeSent;
            }
            else
            {
                _emailSendingPacer.Release(owner.Email);
            }

            return requestResult;
        }

        private async Task<PasswordResetResult> ResetOrReportAsync(string email, string code, string newPassword)
        {
            PasswordResetResult resetResult;
            try
            {
                Player? verifiedOwner = await FindVerifiedOwnerAsync(email, code);
                resetResult = PasswordResetResult.InvalidCode;
                if (verifiedOwner is not null)
                {
                    resetResult = await CompleteResetAsync(verifiedOwner.PlayerId, newPassword);
                }
            }
            catch (NpgsqlException exception)
            {
                _log.Error(ResetFailedMessage, exception);
                resetResult = PasswordResetResult.DatabaseUnavailable;
            }

            return resetResult;
        }

        private async Task<PasswordResetResult> CompleteResetAsync(int playerId, string newPassword)
        {
            bool wasCompleted = await _passwordRecoveryRepository.CompleteAsync(playerId, PasswordRules.Hash(newPassword));
            PasswordResetResult resetResult = PasswordResetResult.InvalidCode;
            if (wasCompleted)
            {
                _codeAttemptCounter.Forget(playerId);
                _log.InfoFormat(CultureInfo.InvariantCulture, PasswordResetMessage, playerId);
                resetResult = PasswordResetResult.Reset;
            }

            return resetResult;
        }

        private async Task<Player?> FindVerifiedOwnerAsync(string email, string code)
        {
            Player? owner = await _playerRepository.FindByEmailAsync(email);
            if (owner is null)
            {
                return null;
            }

            PasswordRecovery? recovery = await _passwordRecoveryRepository.FindAsync(owner.PlayerId);
            bool isUsable = (recovery is not null) && IsUsable(recovery, code);
            if (!isUsable)
            {
                _codeAttemptCounter.RecordFailure(owner.PlayerId);
            }

            Player? verifiedOwner = isUsable ? owner : null;

            return verifiedOwner;
        }

        private bool IsUsable(PasswordRecovery recovery, string code)
        {
            int codeLifetimeMinutes = RequireCodeLifetimeMinutes();
            DateTime expiresAt = recovery.CreatedAt.AddMinutes(codeLifetimeMinutes);
            bool isCurrent = _timeProvider.GetUtcNow().UtcDateTime < expiresAt;
            bool isUsable = isCurrent
                && string.Equals(recovery.Code, code, StringComparison.Ordinal)
                && _codeAttemptCounter.HasAttemptsLeft(recovery.PlayerId);

            return isUsable;
        }

        private int RequireCodeLifetimeMinutes()
        {
            int codeLifetimeMinutes = _codeLifetimeMinutes ?? throw new InvalidOperationException(NotStartedMessage);

            return codeLifetimeMinutes;
        }
    }
}
