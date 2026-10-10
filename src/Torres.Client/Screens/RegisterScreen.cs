using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class RegisterScreen : Screen
    {
        private readonly ChannelFactory<IAccountService> _accountChannelFactory;
        private readonly LabeledTextBox _usernameField = new LabeledTextBox(TextKeys.Register.UsernameLabel, false);
        private readonly LabeledTextBox _emailField = new LabeledTextBox(TextKeys.Register.EmailLabel, false);
        private readonly LabeledTextBox _passwordField = new LabeledTextBox(TextKeys.Register.PasswordLabel, true);
        private readonly LocalizedLabel _usernameMessage = CreateErrorMessage(TextKeys.Common.RequiredField);
        private readonly LocalizedLabel _emailMessage = CreateErrorMessage(TextKeys.Common.RequiredField);
        private readonly LocalizedLabel _passwordMessage = CreateErrorMessage(TextKeys.Common.RequiredField);
        private readonly LocalizedLabel _serverMessage = CreateErrorMessage(TextKeys.Common.ServerErrorTitle);
        private readonly LocalizedButton _createAccountButton = CreatePrimaryButton(TextKeys.Register.CreateAccountButton);

        private IAccountService? _accountChannel;
        private Task<RegistrationResult>? _registrationRequest;

        internal RegisterScreen(ChannelFactory<IAccountService> accountChannelFactory)
            : base(TextKeys.Register.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(accountChannelFactory);

            _accountChannelFactory = accountChannelFactory;
        }

        internal override void Update(GameTime gameTime)
        {
            if ((_registrationRequest is null) || (!_registrationRequest.IsCompleted))
            {
                return;
            }

            Task<RegistrationResult> completedRegistration = _registrationRequest;
            _registrationRequest = null;
            _createAccountButton.Enabled = true;
            ShowOutcome(completedRegistration);
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = CreateColumns();
            columns.Widgets.Add(BuildCard());
            columns.Widgets.Add(CreateAmbiencePanel());
            StackPanel.SetProportionType(columns.Widgets[1], ProportionType.Fill);

            return columns;
        }

        private VerticalStackPanel BuildCard()
        {
            VerticalStackPanel card = CreateCard(Sizes.RegisterCardWidth);
            card.VerticalAlignment = VerticalAlignment.Center;
            card.Widgets.Add(CreateCardHeader(TextKeys.Register.Title, TextKeys.Register.Hint));
            card.Widgets.Add(BuildField(_usernameField, _usernameMessage));
            card.Widgets.Add(BuildField(_emailField, _emailMessage));
            card.Widgets.Add(BuildField(_passwordField, _passwordMessage));
            card.Widgets.Add(_serverMessage);

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackButton);
            _createAccountButton.Click += CreateAccountButtonOnClick;
            backButton.Click += BackButtonOnClick;
            HorizontalStackPanel formActions = CreateButtonRow();
            formActions.Widgets.Add(_createAccountButton);
            formActions.Widgets.Add(backButton);
            card.Widgets.Add(formActions);

            return card;
        }

        private static VerticalStackPanel BuildField(LabeledTextBox textBox, LocalizedLabel messageLabel)
        {
            var fieldGroup = new VerticalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            fieldGroup.Widgets.Add(textBox);
            fieldGroup.Widgets.Add(messageLabel);

            return fieldGroup;
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
            _accountChannel = _accountChannelFactory.CreateChannel();
            _registrationRequest = _accountChannel.RegisterAsync(_usernameField.Value, _emailField.Value, _passwordField.Value);
        }

        private static bool ReportIfEmpty(LabeledTextBox textBox, LocalizedLabel messageLabel)
        {
            bool isEmpty = string.IsNullOrWhiteSpace(textBox.Value);
            messageLabel.TextKey = TextKeys.Common.RequiredField;
            messageLabel.Visible = isEmpty;

            return isEmpty;
        }

        private void ShowOutcome(Task<RegistrationResult> registration)
        {
            if (!registration.IsCompletedSuccessfully)
            {
                AbortChannel();
                ShowMessage(_serverMessage, TextKeys.Common.ServerErrorTitle);

                return;
            }

            ShowResult(registration.Result);
        }

        private void ShowResult(RegistrationResult registrationResult)
        {
            switch (registrationResult)
            {
                case RegistrationResult.Created:
                    RequestedScreen = ScreenId.MainMenu;
                    break;

                case RegistrationResult.UsernameTaken:
                    ShowMessage(_usernameMessage, TextKeys.Register.UsernameUnavailable);
                    break;

                case RegistrationResult.EmailTaken:
                    ShowMessage(_emailMessage, TextKeys.Register.EmailAlreadyExists);
                    break;

                case RegistrationResult.InvalidUsername:
                    ShowMessage(_usernameMessage, TextKeys.Register.UsernameInvalidFormat);
                    break;

                case RegistrationResult.InvalidEmail:
                    ShowMessage(_emailMessage, TextKeys.Register.EmailInvalidFormat);
                    break;

                case RegistrationResult.InvalidPassword:
                    ShowMessage(_passwordMessage, TextKeys.Register.PasswordInvalidFormat);
                    break;

                case RegistrationResult.DatabaseUnavailable:
                    ShowMessage(_serverMessage, TextKeys.Common.ServerErrorTitle);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(registrationResult), registrationResult, null);
            }
        }

        private static void ShowMessage(LocalizedLabel messageLabel, string textKey)
        {
            messageLabel.TextKey = textKey;
            messageLabel.Visible = true;
        }

        private void AbortChannel()
        {
            if (_accountChannel is ICommunicationObject failedChannel)
            {
                failedChannel.Abort();
            }

            _accountChannel = null;
        }

        private void BackButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Login;
        }
    }
}
