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
        private readonly LocalizedLabel _usernameMessage = Error(TextKeys.Common.RequiredField);
        private readonly LocalizedLabel _emailMessage = Error(TextKeys.Common.RequiredField);
        private readonly LocalizedLabel _passwordMessage = Error(TextKeys.Common.RequiredField);
        private readonly LocalizedLabel _serverMessage = Error(TextKeys.Common.ServerErrorTitle);
        private readonly LocalizedButton _createAccountButton = PrimaryButton(TextKeys.Register.CreateAccountButton);

        private IAccountService? _accountChannel;
        private Task<RegistrationResult>? _registration;

        internal RegisterScreen(ChannelFactory<IAccountService> accountChannelFactory)
            : base(TextKeys.Register.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(accountChannelFactory);

            _accountChannelFactory = accountChannelFactory;
        }

        internal override void Update(GameTime gameTime)
        {
            if ((_registration is null) || (!_registration.IsCompleted))
            {
                return;
            }

            Task<RegistrationResult> registration = _registration;
            _registration = null;
            _createAccountButton.Enabled = true;
            ShowOutcome(registration);
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = Columns();
            columns.Widgets.Add(BuildCard());
            columns.Widgets.Add(Ambience());
            StackPanel.SetProportionType(columns.Widgets[1], ProportionType.Fill);

            return columns;
        }

        private VerticalStackPanel BuildCard()
        {
            VerticalStackPanel card = Card(Sizes.RegisterCardWidth);
            card.VerticalAlignment = VerticalAlignment.Center;
            card.Widgets.Add(CardHeader(TextKeys.Register.Title, TextKeys.Register.Hint));
            card.Widgets.Add(BuildField(_usernameField, _usernameMessage));
            card.Widgets.Add(BuildField(_emailField, _emailMessage));
            card.Widgets.Add(BuildField(_passwordField, _passwordMessage));
            card.Widgets.Add(_serverMessage);

            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackButton);
            _createAccountButton.Click += CreateAccountButtonOnClick;
            backButton.Click += BackButtonOnClick;
            HorizontalStackPanel actions = Row();
            actions.Widgets.Add(_createAccountButton);
            actions.Widgets.Add(backButton);
            card.Widgets.Add(actions);

            return card;
        }

        private static VerticalStackPanel BuildField(LabeledTextBox field, LocalizedLabel message)
        {
            var group = new VerticalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            group.Widgets.Add(field);
            group.Widgets.Add(message);

            return group;
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
            _registration = _accountChannel.RegisterAsync(_usernameField.Value, _emailField.Value, _passwordField.Value);
        }

        private static bool ReportIfEmpty(LabeledTextBox field, LocalizedLabel message)
        {
            bool isEmpty = string.IsNullOrWhiteSpace(field.Value);
            message.TextKey = TextKeys.Common.RequiredField;
            message.Visible = isEmpty;

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

        private void ShowResult(RegistrationResult result)
        {
            switch (result)
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
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }

        private static void ShowMessage(LocalizedLabel message, string textKey)
        {
            message.TextKey = textKey;
            message.Visible = true;
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
