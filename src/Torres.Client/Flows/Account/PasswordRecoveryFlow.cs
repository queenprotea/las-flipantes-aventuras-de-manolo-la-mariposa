using System;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Localization;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class PasswordRecoveryFlow
    {
        private const char EmailSeparator = '@';

        private readonly AccountConnection _accountConnection;
        private readonly PendingRequest<RecoveryCodeRequestResult> _codeRequest = new PendingRequest<RecoveryCodeRequestResult>();
        private readonly PendingRequest<RecoveryCodeVerificationResult> _codeVerification = new PendingRequest<RecoveryCodeVerificationResult>();
        private readonly PendingRequest<PasswordResetResult> _passwordReset = new PendingRequest<PasswordResetResult>();

        private string _requestedEmail = string.Empty;
        private string _verifiedCode = string.Empty;
        private PasswordRecoveryOutcome? _outcome;

        internal PasswordRecoveryFlow(AccountConnection accountConnection)
        {
            _accountConnection = accountConnection;
        }

        internal bool IsSendingCode => _codeRequest.IsWaiting;

        internal bool IsVerifying => _codeVerification.IsWaiting;

        internal bool IsSaving => _passwordReset.IsWaiting;

        internal bool StartCodeRequest(string email)
        {
            string typedEmail = email.Trim();
            bool isValid = typedEmail.Contains(EmailSeparator);
            if (isValid && !_codeRequest.IsWaiting)
            {
                _requestedEmail = typedEmail;
                _codeRequest.Start(_accountConnection.RequestRecoveryCodeAsync(_requestedEmail));
            }

            return isValid;
        }

        internal bool StartVerification(string code)
        {
            string typedCode = code.Trim();
            bool isTyped = typedCode.Length > 0;
            if (isTyped && !_codeVerification.IsWaiting)
            {
                _verifiedCode = typedCode;
                _codeVerification.Start(_accountConnection.VerifyRecoveryCodeAsync(_requestedEmail, _verifiedCode));
            }

            return isTyped;
        }

        internal bool StartReset(string newPassword, string repeatedPassword)
        {
            bool isRepeated = string.Equals(newPassword, repeatedPassword, StringComparison.Ordinal);
            if (isRepeated && !_passwordReset.IsWaiting)
            {
                _passwordReset.Start(_accountConnection.ResetPasswordAsync(_requestedEmail, _verifiedCode, newPassword));
            }

            return isRepeated;
        }

        internal PasswordRecoveryOutcome? TakeOutcome()
        {
            FollowCodeRequest();
            FollowCodeVerification();
            FollowPasswordReset();
            PasswordRecoveryOutcome? outcome = _outcome;
            _outcome = null;

            return outcome;
        }

        private static PasswordRecoveryOutcome CreateMessage(PasswordRecoveryStep messageStep, string messageKey)
        {
            var message = new PasswordRecoveryOutcome(null, messageStep, messageKey, null);

            return message;
        }

        private static PasswordRecoveryOutcome CreateEnding(PasswordRecoveryEnding ending)
        {
            var endingOutcome = new PasswordRecoveryOutcome(null, null, null, ending);

            return endingOutcome;
        }

        private static PasswordRecoveryOutcome ToOutcome(RecoveryCodeRequestResult requestResult)
        {
            PasswordRecoveryOutcome outcome = requestResult switch
            {
                RecoveryCodeRequestResult.CodeSent => new PasswordRecoveryOutcome(PasswordRecoveryStep.Code, null, null, null),
                RecoveryCodeRequestResult.InvalidEmail => CreateMessage(PasswordRecoveryStep.Email, TextKeys.PasswordRecovery.InvalidEmail),
                RecoveryCodeRequestResult.TooManyRequests => CreateMessage(PasswordRecoveryStep.Email, TextKeys.Alerts.TooManyCodesBody),
                RecoveryCodeRequestResult.MailDeliveryFailed => CreateMessage(PasswordRecoveryStep.Email, TextKeys.Alerts.EmailFailedBody),
                RecoveryCodeRequestResult.DatabaseUnavailable => CreateEnding(PasswordRecoveryEnding.ServerUnreachable),
                _ => throw new ArgumentOutOfRangeException(nameof(requestResult), requestResult, null),
            };

            return outcome;
        }

        private static PasswordRecoveryOutcome ToOutcome(RecoveryCodeVerificationResult verification)
        {
            PasswordRecoveryOutcome outcome = verification switch
            {
                RecoveryCodeVerificationResult.Valid => new PasswordRecoveryOutcome(PasswordRecoveryStep.Password, null, null, null),
                RecoveryCodeVerificationResult.InvalidCode => CreateMessage(PasswordRecoveryStep.Code, TextKeys.PasswordRecovery.InvalidCode),
                RecoveryCodeVerificationResult.DatabaseUnavailable => CreateEnding(PasswordRecoveryEnding.ServerUnreachable),
                _ => throw new ArgumentOutOfRangeException(nameof(verification), verification, null),
            };

            return outcome;
        }

        private static PasswordRecoveryOutcome ToOutcome(PasswordResetResult resetResult)
        {
            PasswordRecoveryOutcome outcome = resetResult switch
            {
                PasswordResetResult.Reset => CreateEnding(PasswordRecoveryEnding.PasswordReset),
                PasswordResetResult.InvalidCode => new PasswordRecoveryOutcome(PasswordRecoveryStep.Code, PasswordRecoveryStep.Code, TextKeys.PasswordRecovery.InvalidCode, null),
                PasswordResetResult.InvalidPassword => CreateMessage(PasswordRecoveryStep.Password, TextKeys.PasswordRecovery.PasswordInvalidFormat),
                PasswordResetResult.DatabaseUnavailable => CreateEnding(PasswordRecoveryEnding.ServerUnreachable),
                _ => throw new ArgumentOutOfRangeException(nameof(resetResult), resetResult, null),
            };

            return outcome;
        }

        private void FollowCodeRequest()
        {
            FinishedRequest<RecoveryCodeRequestResult>? finishedRequest = _codeRequest.TakeFinished();
            if (finishedRequest is null)
            {
                return;
            }

            _accountConnection.ReleaseChannel();
            RecoveryCodeRequestResult requestResult = finishedRequest.IsSuccessful ? finishedRequest.Reply : RecoveryCodeRequestResult.DatabaseUnavailable;
            _outcome = ToOutcome(requestResult);
        }

        private void FollowCodeVerification()
        {
            FinishedRequest<RecoveryCodeVerificationResult>? finishedVerification = _codeVerification.TakeFinished();
            if (finishedVerification is null)
            {
                return;
            }

            _accountConnection.ReleaseChannel();
            RecoveryCodeVerificationResult verification = finishedVerification.IsSuccessful ? finishedVerification.Reply : RecoveryCodeVerificationResult.DatabaseUnavailable;
            _outcome = ToOutcome(verification);
        }

        private void FollowPasswordReset()
        {
            FinishedRequest<PasswordResetResult>? finishedReset = _passwordReset.TakeFinished();
            if (finishedReset is null)
            {
                return;
            }

            _accountConnection.ReleaseChannel();
            PasswordResetResult resetResult = finishedReset.IsSuccessful ? finishedReset.Reply : PasswordResetResult.DatabaseUnavailable;
            _outcome = ToOutcome(resetResult);
        }
    }
}
