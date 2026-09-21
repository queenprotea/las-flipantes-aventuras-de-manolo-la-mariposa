using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

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
            VerticalStackPanel card = Card(Theme.LoginCardWidth);
            card.VerticalAlignment = VerticalAlignment.Center;
            card.Widgets.Add(CardHeader(TextKeys.Login.Title, TextKeys.Login.Hint));
            card.Widgets.Add(new LabeledTextBox(TextKeys.Login.UsernameLabel, false));

            var passwordField = new VerticalStackPanel { Spacing = Theme.FieldLabelSpacing };
            passwordField.Widgets.Add(new LabeledTextBox(TextKeys.Login.PasswordLabel, true));
            _errorLabel = Error(TextKeys.Login.InvalidCredentials);
            passwordField.Widgets.Add(_errorLabel);
            card.Widgets.Add(passwordField);

            LocalizedButton logInButton = PrimaryButton(TextKeys.Login.LogInButton);
            LocalizedButton createAccountButton = SecondaryButton(TextKeys.Login.CreateAccountButton);
            logInButton.Click += OnLogInClick;
            createAccountButton.Click += OnCreateAccountClick;
            HorizontalStackPanel actions = Row();
            actions.Widgets.Add(logInButton);
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
