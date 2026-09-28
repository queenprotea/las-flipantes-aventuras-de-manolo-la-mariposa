using System;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class LanguageScreen : Screen
    {
        private const string CurrentMark = "✓";
        private const int NoteSpacing = 10;

        private readonly LanguagePreference _languagePreference;
        private readonly LocalizedButton _spanishOption = LanguageOptionButton(LanguagePreference.Available[0].NameKey);
        private readonly LocalizedButton _englishOption = LanguageOptionButton(LanguagePreference.Available[1].NameKey);
        private readonly Label _spanishMark;
        private readonly Label _englishMark;

        internal LanguageScreen(LanguagePreference languagePreference)
            : base(TextKeys.Language.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(languagePreference);

            _languagePreference = languagePreference;
            _spanishMark = _spanishOption.SetTrailingMark(CurrentMark, Theme.MintInk);
            _englishMark = _englishOption.SetTrailingMark(CurrentMark, Theme.MintInk);
        }

        protected override Widget Build()
        {
            VerticalStackPanel card = Card(Sizes.LanguageCardWidth);
            card.Widgets.Add(CardHeader(TextKeys.Language.Title, TextKeys.Language.Hint));

            _spanishOption.Click += SpanishOptionOnClick;
            _englishOption.Click += EnglishOptionOnClick;

            var options = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            options.Widgets.Add(_spanishOption);
            options.Widgets.Add(_englishOption);
            card.Widgets.Add(options);

            LocalizedLabel savedHint = Hint(TextKeys.Language.SavedHint);
            savedHint.Margin = new Thickness(0, NoteSpacing, 0, 0);
            card.Widgets.Add(savedHint);

            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackButton);
            backButton.Click += BackButtonOnClick;
            HorizontalStackPanel actions = Row();
            actions.Widgets.Add(backButton);
            card.Widgets.Add(actions);

            UpdateMarks();

            return card;
        }

        private static LocalizedButton LanguageOptionButton(string textKey)
        {
            return new LocalizedButton(textKey)
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
        }

        private void UpdateMarks()
        {
            bool isSpanish = _languagePreference.IsCurrent(LanguagePreference.Available[0]);
            ApplySelection(_spanishOption, _spanishMark, isSpanish);
            ApplySelection(_englishOption, _englishMark, !isSpanish);
        }

        private static void ApplySelection(LocalizedButton option, Label mark, bool isSelected)
        {
            mark.Visible = isSelected;
            option.Background = isSelected ? Theme.MintTintBrush : Theme.SurfaceBrush;
            option.Border = isSelected ? Theme.MintLineBrush : Theme.LineBrush;
        }

        private void SpanishOptionOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            _languagePreference.Change(LanguagePreference.Available[0]);
            UpdateMarks();
        }

        private void EnglishOptionOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            _languagePreference.Change(LanguagePreference.Available[1]);
            UpdateMarks();
        }

        private void BackButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
