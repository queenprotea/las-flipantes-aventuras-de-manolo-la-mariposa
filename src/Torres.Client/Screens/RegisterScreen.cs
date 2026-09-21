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
            HorizontalStackPanel columns = Columns();
            columns.Widgets.Add(BuildCard());
            columns.Widgets.Add(Ambience());
            StackPanel.SetProportionType(columns.Widgets[1], ProportionType.Fill);
            return columns;
        }

        private VerticalStackPanel BuildCard()
        {
            VerticalStackPanel card = Card(Theme.RegisterCardWidth);
            card.VerticalAlignment = VerticalAlignment.Center;
            card.Widgets.Add(CardHeader(TextKeys.Register.Title, TextKeys.Register.Hint));

            var usernameField = new VerticalStackPanel { Spacing = Theme.FieldLabelSpacing };
            _usernameField = new LabeledTextBox(TextKeys.Register.UsernameLabel, false);
            _usernameField.Box.TextChangedByUser += OnUsernameChanged;
            usernameField.Widgets.Add(_usernameField);

            _usernameStateLabel = Success(TextKeys.Register.UsernameAvailable);
            _usernameStateLabel.Visible = false;
            usernameField.Widgets.Add(_usernameStateLabel);
            card.Widgets.Add(usernameField);

            card.Widgets.Add(new LabeledTextBox(TextKeys.Register.EmailLabel, false));
            card.Widgets.Add(new LabeledTextBox(TextKeys.Register.PasswordLabel, true));

            LocalizedButton createAccountButton = PrimaryButton(TextKeys.Register.CreateAccountButton);
            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackButton);
            backButton.Click += OnBackClick;
            HorizontalStackPanel actions = Row();
            actions.Widgets.Add(createAccountButton);
            actions.Widgets.Add(backButton);
            card.Widgets.Add(actions);
            return card;
        }

        private void OnUsernameChanged(object sender, Myra.Events.MyraEventArgs arguments)
        {
            string value = _usernameField!.Value;
            _usernameStateLabel!.Visible = value.Length > 0;

            bool isWellFormed = (value.Length >= ShortestUsername) && (value.Length <= LongestUsername);
            _usernameStateLabel.TextKey = isWellFormed
                ? TextKeys.Register.UsernameAvailable
                : TextKeys.Register.UsernameUnavailable;
            _usernameStateLabel.TextColor = isWellFormed ? Theme.MintInk : Theme.BlushInk;
        }

        private void OnBackClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.Login;
        }
    }
}
