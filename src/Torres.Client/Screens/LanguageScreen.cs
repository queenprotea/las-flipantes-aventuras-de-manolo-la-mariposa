using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    /// <summary>
    /// PT-20 del prototipo: la lista de idiomas, con el que está en uso marcado en mentolado.
    /// </summary>
    internal sealed class LanguageScreen : Screen
    {
        private const string CurrentMark = "✓";
        private const int NoteSpacing = 10;

        private readonly LanguageService _languageService;
        private LocalizedButton? _spanishOption;
        private LocalizedButton? _englishOption;
        private Label? _spanishMark;
        private Label? _englishMark;

        internal LanguageScreen(LanguageService languageService)
            : base(TextKeys.Language.HeaderLabel, true)
        {
            _languageService = languageService;
        }

        protected override Widget Build()
        {
            VerticalStackPanel card = Card(Theme.LanguageCardWidth);
            card.Widgets.Add(CardHeader(TextKeys.Language.Title, TextKeys.Language.Hint));

            _spanishOption = LanguageOptionButton(LanguageService.Available[0].NameKey);
            _englishOption = LanguageOptionButton(LanguageService.Available[1].NameKey);
            _spanishMark = _spanishOption.SetTrailingMark(CurrentMark, Theme.MintInk);
            _englishMark = _englishOption.SetTrailingMark(CurrentMark, Theme.MintInk);
            _spanishOption.Click += OnSpanishClick;
            _englishOption.Click += OnEnglishClick;

            var options = new VerticalStackPanel { Spacing = Theme.ListSpacing };
            options.Widgets.Add(_spanishOption);
            options.Widgets.Add(_englishOption);
            card.Widgets.Add(options);

            LocalizedLabel savedHint = Hint(TextKeys.Language.SavedHint);
            savedHint.Margin = new Thickness(0, NoteSpacing, 0, 0);
            card.Widgets.Add(savedHint);

            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackButton);
            backButton.Click += OnBackClick;
            HorizontalStackPanel actions = Row();
            actions.Widgets.Add(backButton);
            card.Widgets.Add(actions);

            UpdateMarks();
            return card;
        }

        private static LocalizedButton LanguageOptionButton(string textKey)
        {
            // .langlist button: caja de 11px por 14px con borde --line, sobre fondo blanco.
            return new LocalizedButton(textKey)
            {
                LabelColor = Theme.Ink,
                LabelFont = Fonts.Control,
                Padding = Theme.ListItemPadding,
                Background = Theme.SurfaceBrush,
                OverBackground = Theme.MintTintBrush,
                PressedBackground = Theme.MintTintBrush,
                Border = Theme.LineBrush,
                BorderThickness = Theme.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
        }

        private void UpdateMarks()
        {
            bool isSpanish = _languageService.IsCurrent(LanguageService.Available[0]);
            ApplySelection(_spanishOption!, _spanishMark!, isSpanish);
            ApplySelection(_englishOption!, _englishMark!, !isSpanish);
        }

        private void ApplySelection(LocalizedButton option, Label mark, bool isSelected)
        {
            mark.Visible = isSelected;
            option.Background = isSelected ? Theme.MintTintBrush : Theme.SurfaceBrush;
            option.Border = isSelected ? Theme.MintLineBrush : Theme.LineBrush;
        }

        private void OnSpanishClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            _languageService.Change(LanguageService.Available[0]);
            UpdateMarks();
        }

        private void OnEnglishClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            _languageService.Change(LanguageService.Available[1]);
            UpdateMarks();
        }

        private void OnBackClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
