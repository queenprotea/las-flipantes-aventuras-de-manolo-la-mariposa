using Myra.Graphics2D.UI;

using Torres.Client.Localization;

namespace Torres.Client.Screens
{
    internal sealed class AccountSettingsScreen : Screen
    {
        internal AccountSettingsScreen()
            : base(TextKeys.AccountSettings.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            var panel = Card();
            panel.Widgets.Add(Title(TextKeys.AccountSettings.DeleteAccountTitle));
            panel.Widgets.Add(Hint(TextKeys.AccountSettings.DeleteAccountHint));
            panel.Widgets.Add(DestructiveButton(TextKeys.AccountSettings.DeleteAccountButton));

            LocalizedButton backButton = SecondaryButton(TextKeys.AccountSettings.BackToProfileButton);
            backButton.Click += OnBackClick;
            panel.Widgets.Add(backButton);
            return panel;
        }

        private void OnBackClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.Profile;
        }
    }
}
