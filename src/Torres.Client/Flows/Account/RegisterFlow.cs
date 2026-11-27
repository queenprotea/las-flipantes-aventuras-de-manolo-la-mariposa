using System;
using System.Collections.Generic;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class RegisterFlow
    {
        private readonly IAccountConnection _accountConnection;
        private readonly IGameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<RegistrationResult> _codeRequest = new PendingRequest<RegistrationResult>();
        private readonly PendingRequest<RegistrationConfirmationResult> _confirmationRequest = new PendingRequest<RegistrationConfirmationResult>();
        private readonly PendingRequest<OpenSessionResult> _sessionOpening = new PendingRequest<OpenSessionResult>();

        private PlayerIdentity? _registeredPlayer;
        private string _requestedEmail = string.Empty;
        private RegisterOutcome? _outcome;

        internal RegisterFlow(IAccountConnection accountConnection, IGameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _accountConnection = accountConnection;
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal bool IsRequestingCode => _codeRequest.IsWaiting;

        internal bool IsConfirming => _confirmationRequest.IsWaiting || _sessionOpening.IsWaiting;

        internal static ScreenId FindScreenToCancel()
        {
            ScreenId nextScreen = ScreenId.Login;

            return nextScreen;
        }

        internal IReadOnlyList<RegisterField> StartCodeRequest(string username, string email, string password)
        {
            var missingFields = new List<RegisterField>();
            AddIfBlank(missingFields, username, RegisterField.Username);
            AddIfBlank(missingFields, email, RegisterField.Email);
            AddIfBlank(missingFields, password, RegisterField.Password);
            if ((missingFields.Count == 0) && !_codeRequest.IsWaiting)
            {
                _requestedEmail = email.Trim();
                _codeRequest.Start(_accountConnection.RequestRegistrationCodeAsync(username.Trim(), _requestedEmail, password));
            }

            return missingFields;
        }

        internal bool StartConfirmation(string code)
        {
            bool isCodeMissing = string.IsNullOrWhiteSpace(code);
            if (!isCodeMissing && !IsConfirming)
            {
                _confirmationRequest.Start(_accountConnection.ConfirmRegistrationAsync(_requestedEmail, code.Trim()));
            }

            return isCodeMissing;
        }

        internal RegisterOutcome? TakeOutcome()
        {
            FollowCodeRequest();
            FollowConfirmation();
            FollowSessionOpening();
            RegisterOutcome? outcome = _outcome;
            _outcome = null;

            return outcome;
        }

        private static void AddIfBlank(List<RegisterField> missingFields, string typedValue, RegisterField field)
        {
            if (string.IsNullOrWhiteSpace(typedValue))
            {
                missingFields.Add(field);
            }
        }

        private static RegisterOutcome CreateMessage(RegisterField field, string messageKey)
        {
            var message = new RegisterOutcome(null, field, messageKey, null);

            return message;
        }

        private static RegisterField FindMessageField(RegistrationResult requestResult)
        {
            RegisterField messageField = requestResult switch
            {
                RegistrationResult.UsernameTaken or RegistrationResult.InvalidUsername => RegisterField.Username,
                RegistrationResult.EmailTaken or RegistrationResult.InvalidEmail => RegisterField.Email,
                RegistrationResult.InvalidPassword => RegisterField.Password,
                RegistrationResult.TooManyRequests or RegistrationResult.MailDeliveryFailed or RegistrationResult.DatabaseUnavailable => RegisterField.Server,
                _ => throw new ArgumentOutOfRangeException(nameof(requestResult), requestResult, null),
            };

            return messageField;
        }

        private static string FindMessageKey(RegistrationResult requestResult)
        {
            string messageKey = requestResult switch
            {
                RegistrationResult.UsernameTaken => TextKeys.Register.UsernameUnavailable,
                RegistrationResult.InvalidUsername => TextKeys.Register.UsernameInvalidFormat,
                RegistrationResult.EmailTaken => TextKeys.Register.EmailAlreadyExists,
                RegistrationResult.InvalidEmail => TextKeys.Register.EmailInvalidFormat,
                RegistrationResult.InvalidPassword => TextKeys.Register.PasswordInvalidFormat,
                RegistrationResult.TooManyRequests => TextKeys.Alerts.TooManyCodesBody,
                RegistrationResult.MailDeliveryFailed => TextKeys.Alerts.EmailFailedBody,
                RegistrationResult.DatabaseUnavailable => TextKeys.Common.ServerErrorTitle,
                _ => throw new ArgumentOutOfRangeException(nameof(requestResult), requestResult, null),
            };

            return messageKey;
        }

        private void FollowCodeRequest()
        {
            FinishedRequest<RegistrationResult>? finishedRequest = _codeRequest.TakeFinished();
            if (finishedRequest is null)
            {
                return;
            }

            _accountConnection.ReleaseChannel();
            if (!finishedRequest.IsSuccessful)
            {
                _outcome = CreateMessage(RegisterField.Server, TextKeys.Common.ServerErrorTitle);
            }
            else if (finishedRequest.Reply == RegistrationResult.CodeSent)
            {
                _outcome = new RegisterOutcome(RegisterStep.Code, null, null, null);
            }
            else
            {
                _outcome = CreateMessage(FindMessageField(finishedRequest.Reply), FindMessageKey(finishedRequest.Reply));
            }
        }

        private void FollowConfirmation()
        {
            FinishedRequest<RegistrationConfirmationResult>? finishedConfirmation = _confirmationRequest.TakeFinished();
            if (finishedConfirmation is null)
            {
                return;
            }

            _accountConnection.ReleaseChannel();
            if (finishedConfirmation.IsSuccessful && (finishedConfirmation.Reply is RegistrationConfirmationResult confirmation))
            {
                FollowConfirmationResult(confirmation);
            }
            else
            {
                _outcome = CreateMessage(RegisterField.Code, TextKeys.Common.ServerErrorTitle);
            }
        }

        private void FollowConfirmationResult(RegistrationConfirmationResult confirmation)
        {
            switch (confirmation.Status)
            {
                case RegistrationConfirmationStatus.Created:
                    OpenSession(confirmation);
                    break;

                case RegistrationConfirmationStatus.InvalidCode:
                    _outcome = CreateMessage(RegisterField.Code, TextKeys.Register.InvalidCode);
                    break;

                case RegistrationConfirmationStatus.UsernameTaken:
                    _outcome = new RegisterOutcome(RegisterStep.Details, RegisterField.Username, TextKeys.Register.UsernameUnavailable, null);
                    break;

                case RegistrationConfirmationStatus.EmailTaken:
                    _outcome = new RegisterOutcome(RegisterStep.Details, RegisterField.Email, TextKeys.Register.EmailAlreadyExists, null);
                    break;

                case RegistrationConfirmationStatus.DatabaseUnavailable:
                    _outcome = CreateMessage(RegisterField.Code, TextKeys.Common.ServerErrorTitle);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(confirmation), confirmation.Status, null);
            }
        }

        private void OpenSession(RegistrationConfirmationResult confirmation)
        {
            if ((confirmation.Player is not PlayerIdentity player) || string.IsNullOrEmpty(confirmation.SessionToken))
            {
                _outcome = CreateMessage(RegisterField.Code, TextKeys.Common.ServerErrorTitle);
                return;
            }

            if (_playerSession.IsGuest)
            {
                _gameSessionConnection.CloseSession();
                _playerSession.End();
            }

            _registeredPlayer = player;
            _sessionOpening.Start(_gameSessionConnection.OpenAsync(confirmation.SessionToken));
        }

        private void FollowSessionOpening()
        {
            FinishedRequest<OpenSessionResult>? finishedOpening = _sessionOpening.TakeFinished();
            if (finishedOpening is null)
            {
                return;
            }

            PlayerIdentity? registeredPlayer = _registeredPlayer;
            _registeredPlayer = null;
            OpenSessionResult? opening = finishedOpening.IsSuccessful ? finishedOpening.Reply : null;
            if ((registeredPlayer is not null) && (opening is not null) && (opening.Status == OpenSessionStatus.SessionOpened))
            {
                _gameSessionConnection.MarkSessionOpen();
                _playerSession.Start(registeredPlayer);
                _outcome = new RegisterOutcome(null, null, null, ScreenId.MainMenu);
            }
            else
            {
                _gameSessionConnection.AbortSessionChannel();
                _outcome = CreateMessage(RegisterField.Code, TextKeys.Common.ServerErrorTitle);
            }
        }
    }
}
