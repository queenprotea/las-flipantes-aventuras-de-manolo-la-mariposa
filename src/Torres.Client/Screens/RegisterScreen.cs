using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class RegisterScreen : Screen
    {
        private const int ShortestUsername = 3;
        private const int LongestUsername = 20;

        private LabeledTextBox? _usernameField;
        private LocalizedLabel? _usernameStateLabel;

        internal RegisterScreen()
            : base(TextKeys.Register.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            var panel = Card();
            panel.Widgets.Add(Title(TextKeys.Register.Title));
            panel.Widgets.Add(Hint(TextKeys.Register.Hint));

            _usernameField = new LabeledTextBox(TextKeys.Register.UsernameLabel, false);
            _usernameField.Box.TextChangedByUser += OnUsernameChanged;
            panel.Widgets.Add(_usernameField);

            _usernameStateLabel = new LocalizedLabel(TextKeys.Register.UsernameAvailable)
            {
                TextColor = Theme.Accent,
                Wrap = true,
                Visible = false,
            };
            panel.Widgets.Add(_usernameStateLabel);

            panel.Widgets.Add(new LabeledTextBox(TextKeys.Register.EmailLabel, false));
            panel.Widgets.Add(new LabeledTextBox(TextKeys.Register.PasswordLabel, true));

            LocalizedButton createAccountButton = PrimaryButton(TextKeys.Register.CreateAccountButton);
            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackButton);
            backButton.Click += OnBackClick;
            HorizontalStackPanel actions = Row();
            actions.Widgets.Add(createAccountButton);
            actions.Widgets.Add(backButton);
            panel.Widgets.Add(actions);
            return panel;
        }

        private void OnUsernameChanged(object sender, Myra.Events.MyraEventArgs arguments)
        {
            string value = _usernameField!.Value;
            _usernameStateLabel!.Visible = value.Length > 0;

            bool isWellFormed = (value.Length >= ShortestUsername) && (value.Length <= LongestUsername);
            _usernameStateLabel.TextKey = isWellFormed
                ? TextKeys.Register.UsernameAvailable
                : TextKeys.Register.UsernameUnavailable;
            _usernameStateLabel.TextColor = isWellFormed ? Theme.Accent : Theme.Danger;
        }

        private void OnBackClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.Login;
        }
    }
}
