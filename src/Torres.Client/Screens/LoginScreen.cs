using Myra.Graphics2D.UI;

using Torres.Client.Localization;

namespace Torres.Client.Screens
{
    internal sealed class LoginScreen : Screen
    {
        private LocalizedLabel? _errorLabel;

        internal LoginScreen()
            : base(TextKeys.Login.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            var panel = Card();
            panel.Widgets.Add(Title(TextKeys.Login.Title));
            panel.Widgets.Add(Hint(TextKeys.Login.Hint));
            panel.Widgets.Add(new LabeledTextBox(TextKeys.Login.UsernameLabel, false));
            panel.Widgets.Add(new LabeledTextBox(TextKeys.Login.PasswordLabel, true));

            _errorLabel = Error(TextKeys.Login.InvalidCredentials);
            panel.Widgets.Add(_errorLabel);

            LocalizedButton logInButton = PrimaryButton(TextKeys.Login.LogInButton);
            LocalizedButton createAccountButton = SecondaryButton(TextKeys.Login.CreateAccountButton);
            logInButton.Click += OnLogInClick;
            createAccountButton.Click += OnCreateAccountClick;
            HorizontalStackPanel actions = Row();
            actions.Widgets.Add(logInButton);
            actions.Widgets.Add(createAccountButton);
            panel.Widgets.Add(actions);

            LocalizedButton forgotButton = SecondaryButton(TextKeys.Login.ForgotAccessLink);
            LocalizedButton guestButton = SecondaryButton(TextKeys.Login.PlayAsGuestLink);
            forgotButton.Click += OnForgotClick;
            guestButton.Click += OnGuestClick;
            HorizontalStackPanel links = Row();
            links.Widgets.Add(forgotButton);
            links.Widgets.Add(guestButton);
            panel.Widgets.Add(links);

            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += OnBackClick;
            panel.Widgets.Add(backButton);
            return panel;
        }

        private void OnLogInClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            _errorLabel!.Visible = true;
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
