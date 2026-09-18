using Myra.Graphics2D.UI;

using Torres.Client.Localization;

namespace Torres.Client.Screens
{
    internal sealed class ProfileScreen : Screen
    {
        internal ProfileScreen()
            : base(TextKeys.Profile.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            var panel = Card();

            HorizontalStackPanel avatarRow = Row();
            avatarRow.Widgets.Add(SecondaryButton(TextKeys.Profile.ChangeAvatarButton));
            avatarRow.Widgets.Add(Hint(TextKeys.Profile.AvatarFormatHint));
            panel.Widgets.Add(avatarRow);

            panel.Widgets.Add(Title(TextKeys.Profile.AccountTitle));
            panel.Widgets.Add(Hint(TextKeys.Profile.AccountHint));
            panel.Widgets.Add(new LabeledTextBox(TextKeys.Profile.UsernameLabel, false));
            panel.Widgets.Add(new LabeledTextBox(TextKeys.Profile.EmailLabel, false));

            LocalizedButton saveButton = PrimaryButton(TextKeys.Profile.SaveButton);
            LocalizedButton settingsButton = SecondaryButton(TextKeys.Profile.AccountSettingsButton);
            settingsButton.Click += OnSettingsClick;
            HorizontalStackPanel actions = Row();
            actions.Widgets.Add(saveButton);
            actions.Widgets.Add(settingsButton);
            panel.Widgets.Add(actions);

            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += OnBackClick;
            panel.Widgets.Add(backButton);
            return panel;
        }

        private void OnSettingsClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.AccountSettings;
        }

        private void OnBackClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
