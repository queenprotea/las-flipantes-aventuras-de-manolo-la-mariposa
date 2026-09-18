using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MainMenuScreen : Screen
    {
        private readonly LanguageService _languageService;
        private LocalizedButton? _exitButton;
        private LocalizedButton? _languageButton;

        internal MainMenuScreen(LanguageService languageService)
            : base(TextKeys.MainMenu.HeaderLabel, true)
        {
            _languageService = languageService;
        }

        internal bool WasExitRequested { get; private set; }

        protected override Widget Build()
        {
            var panel = Card();
            panel.Widgets.Add(Disabled(TextKeys.MainMenu.RoomsButton));
            panel.Widgets.Add(Disabled(TextKeys.MainMenu.InvitationsButton));
            panel.Widgets.Add(Disabled(TextKeys.MainMenu.FriendsButton));
            panel.Widgets.Add(Disabled(TextKeys.MainMenu.HistoryButton));
            panel.Widgets.Add(Disabled(TextKeys.MainMenu.RankingButton));

            _exitButton = SecondaryButton(TextKeys.MainMenu.ExitButton);
            _exitButton.Click += OnExitClick;
            panel.Widgets.Add(_exitButton);

            _languageButton = PrimaryButton(TextKeys.MainMenu.LanguageButton);
            _languageButton.TextArgumentKey = _languageService.Current.NameKey;
            _languageButton.Click += OnLanguageClick;
            panel.Widgets.Add(_languageButton);
            return panel;
        }

        internal void FollowLanguage()
        {
            if (_languageButton is null)
            {
                return;
            }

            _languageButton.TextArgumentKey = _languageService.Current.NameKey;
            _languageButton.RefreshText();
        }

        private static LocalizedButton Disabled(string textKey)
        {
            return new LocalizedButton(textKey) { LabelColor = Theme.MutedText, Enabled = false };
        }

        private void OnExitClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            WasExitRequested = true;
        }

        private void OnLanguageClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.Language;
        }
    }
}
