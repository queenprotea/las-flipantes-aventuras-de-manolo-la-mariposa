using Microsoft.Xna.Framework;

using Myra.Graphics2D.UI;

using Torres.Client.Contracts;
using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class LoginScreen : Screen
    {
        private readonly LoginFlow _loginFlow;
        private readonly LabeledTextBox _usernameField = new LabeledTextBox(TextKeys.Login.UsernameLabel, false, AccountLimits.LongestUsername);
        private readonly LabeledTextBox _passwordField = new LabeledTextBox(TextKeys.Login.PasswordLabel, true, AccountLimits.LongestPassword);
        private readonly LocalizedLabel _errorLabel = LabelFactory.CreateErrorMessage(TextKeys.Login.InvalidCredentials);
        private readonly LocalizedLabel _confirmationLabel = LabelFactory.CreateSuccessMessage(TextKeys.Login.PasswordChanged);
        private readonly NoticeBox _banNotice = new NoticeBox();
        private readonly LocalizedButton _logInButton = ButtonFactory.CreatePrimaryButton(TextKeys.Login.LogInButton);
        private readonly LocalizedButton _guestButton;

        internal LoginScreen(LoginFlow loginFlow)
            : base(TextKeys.Login.HeaderLabel, true)
        {
            _loginFlow = loginFlow;
            _guestButton = ButtonFactory.CreateLinkButton(TextKeys.Login.PlayAsGuestLink);
            _confirmationLabel.Visible = false;
        }

        internal override void Open()
        {
            _guestButton.Visible = _loginFlow.CanEnterAsGuest;
        }

        internal override void Update(GameTime gameTime)
        {
            LoginOutcome? outcome = _loginFlow.TakeOutcome();
            if (outcome is not null)
            {
                Show(outcome);
            }

            _logInButton.Enabled = !_loginFlow.IsWaiting;
        }

        internal void ShowNotice(string textKey)
        {
            ShowError(textKey);
        }

        internal void ShowConfirmation(string textKey)
        {
            _errorLabel.Visible = false;
            _confirmationLabel.TextKey = textKey;
            _confirmationLabel.Visible = true;
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = PanelFactory.CreateColumns();
            columns.Widgets.Add(BuildCard());
            columns.Widgets.Add(PanelFactory.CreateAmbiencePanel());
            StackPanel.SetProportionType(columns.Widgets[1], ProportionType.Fill);

            VerticalStackPanel page = PanelFactory.CreatePage();
            page.Widgets.Add(columns);
            StackPanel.SetProportionType(columns, ProportionType.Fill);

            LocalizedButton backButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(PanelFactory.CreateBackBar(backButton));

            return page;
        }

        private VerticalStackPanel BuildCard()
        {
            VerticalStackPanel card = PanelFactory.CreateCard(Sizes.LoginCardWidth);
            card.VerticalAlignment = VerticalAlignment.Center;
            card.Widgets.Add(PanelFactory.CreateCardHeader(TextKeys.Login.Title, TextKeys.Login.Hint));
            card.Widgets.Add(_banNotice);
            card.Widgets.Add(_confirmationLabel);
            card.Widgets.Add(_usernameField);

            var passwordField = new VerticalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            passwordField.Widgets.Add(_passwordField);
            passwordField.Widgets.Add(_errorLabel);
            card.Widgets.Add(passwordField);

            LocalizedButton createAccountButton = ButtonFactory.CreateSecondaryButton(TextKeys.Login.CreateAccountButton);
            _logInButton.Click += LogInButtonOnClick;
            createAccountButton.Click += CreateAccountButtonOnClick;
            HorizontalStackPanel accountActions = PanelFactory.CreateButtonRow();
            accountActions.Widgets.Add(_logInButton);
            accountActions.Widgets.Add(createAccountButton);
            card.Widgets.Add(accountActions);

            LocalizedButton forgotAccessButton = ButtonFactory.CreateLinkButton(TextKeys.Login.ForgotAccessLink);
            forgotAccessButton.Click += ForgotAccessButtonOnClick;
            _guestButton.Click += GuestButtonOnClick;
            HorizontalStackPanel accessLinks = PanelFactory.CreateButtonRow();
            accessLinks.Widgets.Add(forgotAccessButton);
            accessLinks.Widgets.Add(_guestButton);
            card.Widgets.Add(accessLinks);

            return card;
        }

        private void Show(LoginOutcome outcome)
        {
            if (outcome.IsBanned)
            {
                _banNotice.ShowMessage(TextKeys.Login.AccountBannedTitle, TextKeys.Login.AccountBannedDetail, null);
            }
            else if (outcome.ErrorKey is string errorKey)
            {
                ShowError(errorKey);
            }
            else if (outcome.NextScreen is ScreenId nextScreen)
            {
                _passwordField.Box.Text = string.Empty;
                RequestedScreen = nextScreen;
            }
        }

        private void ShowError(string textKey)
        {
            _confirmationLabel.Visible = false;
            _errorLabel.TextKey = textKey;
            _errorLabel.Visible = true;
        }

        private void LogInButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            _errorLabel.Visible = false;
            _confirmationLabel.Visible = false;
            _banNotice.Hide();
            string? errorKey = _loginFlow.StartLogin(_usernameField.Value, _passwordField.Value);
            if (errorKey is not null)
            {
                ShowError(errorKey);
            }
        }

        private void CreateAccountButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = LoginFlow.FindScreenToCreateAccount();
        }

        private void ForgotAccessButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = LoginFlow.FindScreenToRecoverAccess();
        }

        private void GuestButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = LoginFlow.FindScreenToPlayAsGuest();
        }

        private void BackButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = LoginFlow.FindScreenToGoBack();
        }
    }
}
