using System;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class LanguageScreen : Screen
    {
        private const int NoteSpacing = 10;

        private readonly LanguagePreference _languagePreference;
        private readonly LocalizedButton _spanishOption = CreateLanguageOptionButton(LanguagePreference.Spanish.NameKey);
        private readonly LocalizedButton _englishOption = CreateLanguageOptionButton(LanguagePreference.English.NameKey);
        private readonly Label _spanishMark;
        private readonly Label _englishMark;

        internal LanguageScreen(LanguagePreference languagePreference)
            : base(TextKeys.Language.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(languagePreference);

            _languagePreference = languagePreference;
            _spanishMark = _spanishOption.SetTrailingMark(DisplaySymbols.CheckMark, Theme.MintInk);
            _englishMark = _englishOption.SetTrailingMark(DisplaySymbols.CheckMark, Theme.MintInk);
        }

        protected override Widget Build()
        {
            VerticalStackPanel card = CreateCard(Sizes.LanguageCardWidth);
            card.Widgets.Add(CreateCardHeader(TextKeys.Language.Title, TextKeys.Language.Hint));

            _spanishOption.Click += SpanishOptionOnClick;
            _englishOption.Click += EnglishOptionOnClick;

            var languageOptions = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            languageOptions.Widgets.Add(_spanishOption);
            languageOptions.Widgets.Add(_englishOption);
            card.Widgets.Add(languageOptions);

            LocalizedLabel savedHint = CreateHint(TextKeys.Language.SavedHint);
            savedHint.Margin = new Thickness(0, NoteSpacing, 0, 0);
            card.Widgets.Add(savedHint);

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackButton);
            backButton.Click += BackButtonOnClick;
            HorizontalStackPanel navigationActions = CreateButtonRow();
            navigationActions.Widgets.Add(backButton);
            card.Widgets.Add(navigationActions);

            UpdateSelectionMarks();

            return card;
        }

        private static LocalizedButton CreateLanguageOptionButton(string textKey)
        {
            var languageOptionButton = new LocalizedButton(textKey)
            {
                LabelColor = Theme.Ink,
                LabelFont = Fonts.Control,
                Padding = Metrics.ListItemPadding,
                Background = Theme.SurfaceBrush,
                OverBackground = Theme.MintTintBrush,
                PressedBackground = Theme.MintTintBrush,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            return languageOptionButton;
        }

        private void UpdateSelectionMarks()
        {
            bool isSpanish = _languagePreference.IsCurrent(LanguagePreference.Spanish);
            ApplySelection(_spanishOption, _spanishMark, isSpanish);
            ApplySelection(_englishOption, _englishMark, !isSpanish);
        }

        private static void ApplySelection(LocalizedButton optionButton, Label selectionMark, bool isSelected)
        {
            selectionMark.Visible = isSelected;
            optionButton.Background = isSelected ? Theme.MintTintBrush : Theme.SurfaceBrush;
            optionButton.Border = isSelected ? Theme.MintLineBrush : Theme.LineBrush;
        }

        private void SpanishOptionOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            _languagePreference.Change(LanguagePreference.Spanish);
            UpdateSelectionMarks();
        }

        private void EnglishOptionOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            _languagePreference.Change(LanguagePreference.English);
            UpdateSelectionMarks();
        }

        private void BackButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
