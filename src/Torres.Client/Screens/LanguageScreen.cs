using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class LanguageScreen : Screen
    {
        private const string CurrentMarker = "»";

        private readonly LanguageService _languageService;
        private Label? _spanishMarker;
        private Label? _englishMarker;

        internal LanguageScreen(LanguageService languageService)
            : base(TextKeys.Language.HeaderLabel, true)
        {
            _languageService = languageService;
        }

        protected override Widget Build()
        {
            var panel = Card();
            panel.Widgets.Add(Title(TextKeys.Language.Title));
            panel.Widgets.Add(Hint(TextKeys.Language.Hint));

            _spanishMarker = Marker();
            _englishMarker = Marker();

            LocalizedButton spanishButton = SecondaryButton(LanguageService.Available[0].NameKey);
            LocalizedButton englishButton = SecondaryButton(LanguageService.Available[1].NameKey);
            spanishButton.Click += OnSpanishClick;
            englishButton.Click += OnEnglishClick;

            HorizontalStackPanel spanishRow = Row();
            spanishRow.Widgets.Add(spanishButton);
            spanishRow.Widgets.Add(_spanishMarker);
            HorizontalStackPanel englishRow = Row();
            englishRow.Widgets.Add(englishButton);
            englishRow.Widgets.Add(_englishMarker);

            panel.Widgets.Add(spanishRow);
            panel.Widgets.Add(englishRow);
            panel.Widgets.Add(Hint(TextKeys.Language.SavedHint));

            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackButton);
            backButton.Click += OnBackClick;
            panel.Widgets.Add(backButton);

            UpdateMarkers();
            return panel;
        }

        private static Label Marker()
        {
            return new Label { Text = CurrentMarker, TextColor = Theme.Accent, Visible = false };
        }

        private void UpdateMarkers()
        {
            bool isSpanish = _languageService.IsCurrent(LanguageService.Available[0]);
            _spanishMarker!.Visible = isSpanish;
            _englishMarker!.Visible = !isSpanish;
        }

        private void OnSpanishClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            _languageService.Change(LanguageService.Available[0]);
            UpdateMarkers();
        }

        private void OnEnglishClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            _languageService.Change(LanguageService.Available[1]);
            UpdateMarkers();
        }

        private void OnBackClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
