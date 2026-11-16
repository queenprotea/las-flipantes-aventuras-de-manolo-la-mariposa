using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class RegisterScreen : Screen
    {
        private readonly ChannelFactory<IAccountService> _accountChannelFactory;
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly VerticalStackPanel _detailsStep = CreateCard(Sizes.RegisterCardWidth);
        private readonly VerticalStackPanel _codeStep = CreateCard(Sizes.RegisterCardWidth);
        private readonly LabeledTextBox _usernameField = new LabeledTextBox(TextKeys.Register.UsernameLabel, false, AccountLimits.LongestUsername);
        private readonly LabeledTextBox _emailField = new LabeledTextBox(TextKeys.Register.EmailLabel, false, AccountLimits.LongestEmail);
        private readonly LabeledTextBox _passwordField = new LabeledTextBox(TextKeys.Register.PasswordLabel, true, AccountLimits.LongestPassword);
        private readonly LabeledTextBox _codeField = new LabeledTextBox(TextKeys.Register.CodeLabel, false, AccountLimits.VerificationCodeLength);
        private readonly LocalizedLabel _usernameMessage = CreateErrorMessage(TextKeys.Common.RequiredField);
        private readonly LocalizedLabel _emailMessage = CreateErrorMessage(TextKeys.Common.RequiredField);
        private readonly LocalizedLabel _passwordMessage = CreateErrorMessage(TextKeys.Common.RequiredField);
        private readonly LocalizedLabel _serverMessage = CreateErrorMessage(TextKeys.Common.ServerErrorTitle);
        private readonly LocalizedLabel _codeMessage = CreateErrorMessage(TextKeys.Register.InvalidCode);
        private readonly LocalizedButton _createAccountButton = CreatePrimaryButton(TextKeys.Register.CreateAccountButton);
        private readonly LocalizedButton _confirmButton = CreatePrimaryButton(TextKeys.Register.ConfirmButton);

        private IAccountService? _accountChannel;
        private Task<RegistrationResult>? _codeRequest;
        private Task<RegistrationConfirmationResult>? _confirmationRequest;
        private Task<OpenSessionResult>? _sessionOpening;
        private PlayerIdentity? _registeredPlayer;
        private string _requestedEmail = string.Empty;

        internal RegisterScreen(ChannelFactory<IAccountService> accountChannelFactory, GameSessionConnection gameSessionConnection, PlayerSession playerSession)
            : base(TextKeys.Register.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(accountChannelFactory);
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(playerSession);

            _accountChannelFactory = accountChannelFactory;
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal override void Open()
        {
            ShowStep(_detailsStep);
        }

        internal override void Update(GameTime gameTime)
        {
            FollowCodeRequest();
            FollowConfirmation();
            FollowSessionOpening();
        }

        protected override Widget Build()
        {
            var steps = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            steps.Widgets.Add(BuildDetailsStep());
            steps.Widgets.Add(BuildCodeStep());

            HorizontalStackPanel columns = CreateColumns();
            columns.Widgets.Add(steps);
            columns.Widgets.Add(CreateAmbiencePanel());
            StackPanel.SetProportionType(columns.Widgets[1], ProportionType.Fill);
            ShowStep(_detailsStep);

            return columns;
        }

        private VerticalStackPanel BuildDetailsStep()
        {
            _detailsStep.Widgets.Add(CreateCardHeader(TextKeys.Register.Title, TextKeys.Register.Hint));
            _detailsStep.Widgets.Add(BuildField(_usernameField, _usernameMessage));
            _detailsStep.Widgets.Add(BuildField(_emailField, _emailMessage));
            _detailsStep.Widgets.Add(BuildField(_passwordField, _passwordMessage));
            _detailsStep.Widgets.Add(_serverMessage);

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackButton);
            _createAccountButton.Click += CreateAccountButtonOnClick;
            backButton.Click += BackButtonOnClick;
            HorizontalStackPanel formActions = CreateButtonRow();
            formActions.Widgets.Add(_createAccountButton);
            formActions.Widgets.Add(backButton);
            _detailsStep.Widgets.Add(formActions);

            return _detailsStep;
        }

        private VerticalStackPanel BuildCodeStep()
        {
            _codeStep.Widgets.Add(CreateCardHeader(TextKeys.Register.CodeStepTitle, TextKeys.Register.CodeStepHint));
            _codeStep.Widgets.Add(BuildField(_codeField, _codeMessage));

            LocalizedButton codeBackButton = CreateSecondaryButton(TextKeys.Common.BackButton);
            _confirmButton.Click += ConfirmButtonOnClick;
            codeBackButton.Click += CodeBackButtonOnClick;
            HorizontalStackPanel codeActions = CreateButtonRow();
            codeActions.Widgets.Add(_confirmButton);
            codeActions.Widgets.Add(codeBackButton);
            _codeStep.Widgets.Add(codeActions);

            return _codeStep;
        }

        private static VerticalStackPanel BuildField(LabeledTextBox textBox, LocalizedLabel messageLabel)
        {
            var fieldGroup = new VerticalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            fieldGroup.Widgets.Add(textBox);
            fieldGroup.Widgets.Add(messageLabel);

            return fieldGroup;
        }

        private void ShowStep(VerticalStackPanel visibleStep)
        {
            _detailsStep.Visible = visibleStep == _detailsStep;
            _codeStep.Visible = visibleStep == _codeStep;
        }

        private void CreateAccountButtonOnClick(object sender, MyraEventArgs e)
        {
            _serverMessage.Visible = false;

            bool isUsernameMissing = ReportIfEmpty(_usernameField, _usernameMessage);
            bool isEmailMissing = ReportIfEmpty(_emailField, _emailMessage);
            bool isPasswordMissing = ReportIfEmpty(_passwordField, _passwordMessage);
            if (isUsernameMissing || isEmailMissing || isPasswordMissing)
            {
                return;
            }

            _createAccountButton.Enabled = false;
            _requestedEmail = _emailField.Value.Trim();
            _accountChannel = _accountChannelFactory.CreateChannel();
            _codeRequest = _accountChannel.RequestRegistrationCodeAsync(_usernameField.Value.Trim(), _requestedEmail, _passwordField.Value);
        }

        private void ConfirmButtonOnClick(object sender, MyraEventArgs e)
        {
            bool isCodeMissing = ReportIfEmpty(_codeField, _codeMessage);
            if (isCodeMissing)
            {
                return;
            }

            _confirmButton.Enabled = false;
            _accountChannel = _accountChannelFactory.CreateChannel();
            _confirmationRequest = _accountChannel.ConfirmRegistrationAsync(_requestedEmail, _codeField.Value.Trim());
        }

        private static bool ReportIfEmpty(LabeledTextBox textBox, LocalizedLabel messageLabel)
        {
            bool isEmpty = string.IsNullOrWhiteSpace(textBox.Value);
            messageLabel.TextKey = TextKeys.Common.RequiredField;
            messageLabel.Visible = isEmpty;

            return isEmpty;
        }

        private void FollowCodeRequest()
        {
            if ((_codeRequest is null) || !_codeRequest.IsCompleted)
            {
                return;
            }

            Task<RegistrationResult> completedRequest = _codeRequest;
            _codeRequest = null;
            _createAccountButton.Enabled = true;
            AbortChannel();
            if (completedRequest.IsCompletedSuccessfully)
            {
                ShowCodeRequestResult(completedRequest.Result);
            }
            else
            {
                ShowMessage(_serverMessage, TextKeys.Common.ServerErrorTitle);
            }
        }

        private void ShowCodeRequestResult(RegistrationResult requestResult)
        {
            if (requestResult == RegistrationResult.CodeSent)
            {
                _codeField.Box.Text = string.Empty;
                _codeMessage.Visible = false;
                ShowStep(_codeStep);
                return;
            }

            LocalizedLabel messageLabel = FindMessageLabel(requestResult);
            string messageKey = FindMessageKey(requestResult);
            ShowMessage(messageLabel, messageKey);
        }

        private LocalizedLabel FindMessageLabel(RegistrationResult requestResult)
        {
            LocalizedLabel messageLabel = requestResult switch
            {
                RegistrationResult.UsernameTaken or RegistrationResult.InvalidUsername => _usernameMessage,
                RegistrationResult.EmailTaken or RegistrationResult.InvalidEmail => _emailMessage,
                RegistrationResult.InvalidPassword => _passwordMessage,
                RegistrationResult.TooManyRequests or RegistrationResult.MailDeliveryFailed or RegistrationResult.DatabaseUnavailable => _serverMessage,
                _ => throw new ArgumentOutOfRangeException(nameof(requestResult), requestResult, null),
            };

            return messageLabel;
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

        private void FollowConfirmation()
        {
            if ((_confirmationRequest is null) || !_confirmationRequest.IsCompleted)
            {
                return;
            }

            Task<RegistrationConfirmationResult> completedConfirmation = _confirmationRequest;
            _confirmationRequest = null;
            AbortChannel();
            if (completedConfirmation.IsCompletedSuccessfully)
            {
                ShowConfirmationResult(completedConfirmation.Result);
            }
            else
            {
                _confirmButton.Enabled = true;
                ShowMessage(_codeMessage, TextKeys.Common.ServerErrorTitle);
            }
        }

        private void ShowConfirmationResult(RegistrationConfirmationResult confirmation)
        {
            switch (confirmation.Status)
            {
                case RegistrationConfirmationStatus.Created:
                    OpenSession(confirmation);
                    break;

                case RegistrationConfirmationStatus.InvalidCode:
                    _confirmButton.Enabled = true;
                    ShowMessage(_codeMessage, TextKeys.Register.InvalidCode);
                    break;

                case RegistrationConfirmationStatus.UsernameTaken:
                    ReturnToDetails(_usernameMessage, TextKeys.Register.UsernameUnavailable);
                    break;

                case RegistrationConfirmationStatus.EmailTaken:
                    ReturnToDetails(_emailMessage, TextKeys.Register.EmailAlreadyExists);
                    break;

                case RegistrationConfirmationStatus.DatabaseUnavailable:
                    _confirmButton.Enabled = true;
                    ShowMessage(_codeMessage, TextKeys.Common.ServerErrorTitle);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(confirmation), confirmation.Status, null);
            }
        }

        private void ReturnToDetails(LocalizedLabel messageLabel, string messageKey)
        {
            _confirmButton.Enabled = true;
            ShowStep(_detailsStep);
            ShowMessage(messageLabel, messageKey);
        }

        private void OpenSession(RegistrationConfirmationResult confirmation)
        {
            if ((confirmation.Player is null) || string.IsNullOrEmpty(confirmation.SessionToken))
            {
                _confirmButton.Enabled = true;
                ShowMessage(_codeMessage, TextKeys.Common.ServerErrorTitle);
                return;
            }

            if (_playerSession.IsGuest)
            {
                _gameSessionConnection.CloseSession();
                _playerSession.End();
            }

            _registeredPlayer = confirmation.Player;
            _sessionOpening = _gameSessionConnection.OpenAsync(confirmation.SessionToken);
        }

        private void FollowSessionOpening()
        {
            if ((_sessionOpening is null) || !_sessionOpening.IsCompleted)
            {
                return;
            }

            Task<OpenSessionResult> completedOpening = _sessionOpening;
            PlayerIdentity? registeredPlayer = _registeredPlayer;
            _sessionOpening = null;
            _registeredPlayer = null;
            _confirmButton.Enabled = true;
            bool isSessionOpen = completedOpening.IsCompletedSuccessfully
                && (completedOpening.Result.Status == OpenSessionStatus.SessionOpened);
            if (isSessionOpen && (registeredPlayer is not null))
            {
                EnterAs(registeredPlayer, completedOpening.Result);
            }
            else
            {
                _gameSessionConnection.AbortSessionChannel();
                ShowMessage(_codeMessage, TextKeys.Common.ServerErrorTitle);
            }
        }

        private void EnterAs(PlayerIdentity player, OpenSessionResult opening)
        {
            _gameSessionConnection.BeginConnectionChecks(opening);
            _playerSession.Start(player);
            _passwordField.Box.Text = string.Empty;
            _codeField.Box.Text = string.Empty;
            RequestedScreen = ScreenId.MainMenu;
        }

        private static void ShowMessage(LocalizedLabel messageLabel, string textKey)
        {
            messageLabel.TextKey = textKey;
            messageLabel.Visible = true;
        }

        private void AbortChannel()
        {
            if (_accountChannel is ICommunicationObject usedChannel)
            {
                usedChannel.Abort();
            }

            _accountChannel = null;
        }

        private void CodeBackButtonOnClick(object sender, MyraEventArgs e)
        {
            ShowStep(_detailsStep);
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Login;
        }
    }
}
