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
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _session;
        private readonly LabeledTextBox _usernameField = new LabeledTextBox(TextKeys.Login.UsernameLabel, false);
        private readonly LabeledTextBox _passwordField = new LabeledTextBox(TextKeys.Login.PasswordLabel, true);
        private readonly LocalizedLabel _errorLabel = CreateErrorMessage(TextKeys.Login.InvalidCredentials);
        private readonly LocalizedButton _logInButton = CreatePrimaryButton(TextKeys.Login.LogInButton);
        private readonly LocalizedButton _guestButton;

        private IAccountService? _accountChannel;
        private Task<LoginResult>? _loginRequest;
        private Task<OpenSessionResult>? _sessionOpening;
        private PlayerIdentity? _loggedInPlayer;

        internal LoginScreen(ChannelFactory<IAccountService> accountChannelFactory, GameSessionConnection gameSessionConnection, PlayerSession session)
            : base(TextKeys.Login.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(accountChannelFactory);
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(session);

            _accountChannelFactory = accountChannelFactory;
            _gameSessionConnection = gameSessionConnection;
            _session = session;
            _guestButton = CreateLinkButton(TextKeys.Login.PlayAsGuestLink);
        }

        internal override void Open()
        {
            _guestButton.Visible = !_session.IsGuest;
        }

        internal override void Update(GameTime gameTime)
        {
            FollowLogin();
            FollowSessionOpening();
        }

        internal void ShowNotice(string textKey)
        {
            ShowError(textKey);
        }

        private void FollowLogin()
        {
            if ((_loginRequest is null) || !_loginRequest.IsCompleted)
            {
                return;
            }

            Task<LoginResult> completedLogin = _loginRequest;
            _loginRequest = null;
            AbortChannel();
            ShowOutcome(completedLogin);
        }

        private void FollowSessionOpening()
        {
            if ((_sessionOpening is null) || !_sessionOpening.IsCompleted)
            {
                return;
            }

            Task<OpenSessionResult> completedOpening = _sessionOpening;
            PlayerIdentity? loggedInPlayer = _loggedInPlayer;
            _sessionOpening = null;
            _loggedInPlayer = null;
            _logInButton.Enabled = true;
            bool isSessionOpen = completedOpening.IsCompletedSuccessfully
                && (completedOpening.Result.Status == OpenSessionStatus.SessionOpened);
            if (isSessionOpen && (loggedInPlayer is not null))
            {
                EnterAs(loggedInPlayer, completedOpening.Result);
            }
            else
            {
                _gameSessionConnection.Abandon();
                ShowError(TextKeys.Common.ServerErrorTitle);
            }
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = CreateColumns();
            columns.Widgets.Add(BuildCard());
            columns.Widgets.Add(CreateAmbiencePanel());
            StackPanel.SetProportionType(columns.Widgets[1], ProportionType.Fill);

            VerticalStackPanel page = CreatePage();
            page.Widgets.Add(columns);
            StackPanel.SetProportionType(columns, ProportionType.Fill);

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(CreateBackBar(backButton));

            return page;
        }

        private VerticalStackPanel BuildCard()
        {
            VerticalStackPanel card = CreateCard(Sizes.LoginCardWidth);
            card.VerticalAlignment = VerticalAlignment.Center;
            card.Widgets.Add(CreateCardHeader(TextKeys.Login.Title, TextKeys.Login.Hint));
            card.Widgets.Add(_usernameField);

            var passwordField = new VerticalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            passwordField.Widgets.Add(_passwordField);
            passwordField.Widgets.Add(_errorLabel);
            card.Widgets.Add(passwordField);

            LocalizedButton createAccountButton = CreateSecondaryButton(TextKeys.Login.CreateAccountButton);
            _logInButton.Click += LogInButtonOnClick;
            createAccountButton.Click += CreateAccountButtonOnClick;
            HorizontalStackPanel accountActions = CreateButtonRow();
            accountActions.Widgets.Add(_logInButton);
            accountActions.Widgets.Add(createAccountButton);
            card.Widgets.Add(accountActions);

            LocalizedButton forgotAccessButton = CreateLinkButton(TextKeys.Login.ForgotAccessLink);
            forgotAccessButton.Click += ForgotButtonOnClick;
            _guestButton.Click += GuestButtonOnClick;
            HorizontalStackPanel accessLinks = CreateButtonRow();
            accessLinks.Widgets.Add(forgotAccessButton);
            accessLinks.Widgets.Add(_guestButton);
            card.Widgets.Add(accessLinks);

            return card;
        }

        private void LogInButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            _errorLabel.Visible = false;

            if (string.IsNullOrWhiteSpace(_usernameField.Value) || string.IsNullOrEmpty(_passwordField.Value))
            {
                ShowError(TextKeys.Common.RequiredField);
                return;
            }

            _logInButton.Enabled = false;
            _accountChannel = _accountChannelFactory.CreateChannel();
            _loginRequest = _accountChannel.LoginAsync(_usernameField.Value.Trim(), _passwordField.Value);
        }

        private void ShowOutcome(Task<LoginResult> completedLogin)
        {
            if (!completedLogin.IsCompletedSuccessfully)
            {
                _logInButton.Enabled = true;
                ShowError(TextKeys.Common.ServerErrorTitle);
                return;
            }

            LoginResult loginResult = completedLogin.Result;
            switch (loginResult.Status)
            {
                case LoginStatus.LoggedIn:
                    OpenSession(loginResult);
                    break;

                case LoginStatus.InvalidCredentials:
                    _logInButton.Enabled = true;
                    ShowError(TextKeys.Login.InvalidCredentials);
                    break;

                case LoginStatus.DatabaseUnavailable:
                    _logInButton.Enabled = true;
                    ShowError(TextKeys.Common.ServerErrorTitle);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(completedLogin), loginResult.Status, null);
            }
        }

        private void OpenSession(LoginResult loginResult)
        {
            if ((loginResult.Player is null) || string.IsNullOrEmpty(loginResult.SessionToken))
            {
                _logInButton.Enabled = true;
                ShowError(TextKeys.Common.ServerErrorTitle);
                return;
            }

            if (_session.IsGuest)
            {
                _gameSessionConnection.CloseSession();
                _session.End();
            }

            _loggedInPlayer = loginResult.Player;
            _sessionOpening = _gameSessionConnection.OpenAsync(loginResult.SessionToken);
        }

        private void EnterAs(PlayerIdentity player, OpenSessionResult opening)
        {
            _gameSessionConnection.Start(opening);
            _session.Start(player);
            _passwordField.Box.Text = string.Empty;
            if (opening.CurrentRoom is null)
            {
                RequestedScreen = ScreenId.MainMenu;
            }
            else
            {
                _session.EnterRoom(opening.CurrentRoom);
                RequestedScreen = ScreenId.Room;
            }
        }

        private void ShowError(string textKey)
        {
            _errorLabel.TextKey = textKey;
            _errorLabel.Visible = true;
        }

        private void AbortChannel()
        {
            if (_accountChannel is ICommunicationObject openChannel)
            {
                openChannel.Abort();
            }

            _accountChannel = null;
        }

        private void CreateAccountButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Register;
        }

        private void ForgotButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.PasswordRecovery;
        }

        private void GuestButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.GuestAccess;
        }

        private void BackButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
