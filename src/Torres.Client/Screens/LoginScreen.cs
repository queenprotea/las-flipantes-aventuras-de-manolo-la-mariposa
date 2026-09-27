using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class LoginScreen : Screen
    {
        private readonly ChannelFactory<IAccountService> _accountChannelFactory;
        private readonly PlayerSession _session;
        private readonly LabeledTextBox _usernameField = new LabeledTextBox(TextKeys.Login.UsernameLabel, false);
        private readonly LabeledTextBox _passwordField = new LabeledTextBox(TextKeys.Login.PasswordLabel, true);
        private readonly LocalizedLabel _errorLabel = Error(TextKeys.Login.InvalidCredentials);
        private readonly LocalizedButton _logInButton = PrimaryButton(TextKeys.Login.LogInButton);

        private IAccountService? _accountChannel;
        private Task<LoginResult>? _login;

        internal LoginScreen(ChannelFactory<IAccountService> accountChannelFactory, PlayerSession session)
            : base(TextKeys.Login.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(accountChannelFactory);
            ArgumentNullException.ThrowIfNull(session);

            _accountChannelFactory = accountChannelFactory;
            _session = session;
        }

        internal override void Update(GameTime gameTime)
        {
            if ((_login is null) || !_login.IsCompleted)
            {
                return;
            }

            Task<LoginResult> login = _login;
            _login = null;
            _logInButton.Enabled = true;
            AbortChannel();
            ShowOutcome(login);
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = Columns();
            columns.Widgets.Add(BuildCard());
            columns.Widgets.Add(Ambience());
            StackPanel.SetProportionType(columns.Widgets[1], ProportionType.Fill);

            VerticalStackPanel page = Page();
            page.Widgets.Add(columns);
            StackPanel.SetProportionType(columns, ProportionType.Fill);

            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += OnBackClick;
            page.Widgets.Add(BackBar(backButton));
            return page;
        }

        private VerticalStackPanel BuildCard()
        {
            VerticalStackPanel card = Card(Sizes.LoginCardWidth);
            card.VerticalAlignment = VerticalAlignment.Center;
            card.Widgets.Add(CardHeader(TextKeys.Login.Title, TextKeys.Login.Hint));
            card.Widgets.Add(_usernameField);

            var passwordField = new VerticalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            passwordField.Widgets.Add(_passwordField);
            passwordField.Widgets.Add(_errorLabel);
            card.Widgets.Add(passwordField);

            LocalizedButton createAccountButton = SecondaryButton(TextKeys.Login.CreateAccountButton);
            _logInButton.Click += OnLogInClick;
            createAccountButton.Click += OnCreateAccountClick;
            HorizontalStackPanel actions = Row();
            actions.Widgets.Add(_logInButton);
            actions.Widgets.Add(createAccountButton);
            card.Widgets.Add(actions);

            LocalizedButton forgotButton = LinkButton(TextKeys.Login.ForgotAccessLink);
            LocalizedButton guestButton = LinkButton(TextKeys.Login.PlayAsGuestLink);
            forgotButton.Click += OnForgotClick;
            guestButton.Click += OnGuestClick;
            HorizontalStackPanel links = Row();
            links.Widgets.Add(forgotButton);
            links.Widgets.Add(guestButton);
            card.Widgets.Add(links);
            return card;
        }

        private void OnLogInClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            _errorLabel.Visible = false;

            if (string.IsNullOrWhiteSpace(_usernameField.Value) || string.IsNullOrEmpty(_passwordField.Value))
            {
                ShowError(TextKeys.Common.RequiredField);
                return;
            }

            _logInButton.Enabled = false;
            _accountChannel = _accountChannelFactory.CreateChannel();
            _login = _accountChannel.LoginAsync(_usernameField.Value.Trim(), _passwordField.Value);
        }

        private void ShowOutcome(Task<LoginResult> login)
        {
            if (!login.IsCompletedSuccessfully)
            {
                ShowError(TextKeys.Common.ServerErrorTitle);
                return;
            }

            LoginResult result = login.Result;
            switch (result.Status)
            {
                case LoginStatus.LoggedIn:
                    EnterAs(result.Player);
                    break;

                case LoginStatus.InvalidCredentials:
                    ShowError(TextKeys.Login.InvalidCredentials);
                    break;

                case LoginStatus.DatabaseUnavailable:
                    ShowError(TextKeys.Common.ServerErrorTitle);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(login), result.Status, null);
            }
        }

        private void EnterAs(PlayerIdentity player)
        {
            _session.Start(player);
            _passwordField.Box.Text = string.Empty;
            RequestedScreen = ScreenId.MainMenu;
        }

        private void ShowError(string textKey)
        {
            _errorLabel.TextKey = textKey;
            _errorLabel.Visible = true;
        }

        private void AbortChannel()
        {
            if (_accountChannel is ICommunicationObject channel)
            {
                channel.Abort();
            }

            _accountChannel = null;
        }

        private void OnCreateAccountClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.Register;
        }

        private void OnForgotClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.PasswordRecovery;
        }

        private void OnGuestClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.GuestAccess;
        }

        private void OnBackClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
