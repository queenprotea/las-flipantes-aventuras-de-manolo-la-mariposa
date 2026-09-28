using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class CreateRoomScreen : Screen
    {
        private const int OptionSpacing = 10;
        private const int OptionTextSpacing = 2;
        private const int RadioMarkSize = 18;
        private const int RadioDotSize = 7;
        private const int OptionsPerRow = 2;
        private const int OptionsWidth = Sizes.CreateRoomCardWidth - (2 * Metrics.CardPaddingSize) - (2 * Sizes.BorderSize);
        private const int OptionWidth = (OptionsWidth - OptionSpacing) / OptionsPerRow;

        private static readonly Thickness _optionPadding = new Thickness(13);

        private readonly Button _publicOption;
        private readonly Button _privateOption;
        private readonly Panel _publicMark;
        private readonly Panel _privateMark;

        internal CreateRoomScreen()
            : base(TextKeys.CreateRoom.HeaderLabel, true)
        {
            _publicMark = BuildRadioMark();
            _privateMark = BuildRadioMark();
            _publicOption = BuildRoomTypeOption(TextKeys.CreateRoom.PublicOption, TextKeys.CreateRoom.PublicOptionHint, _publicMark);
            _privateOption = BuildRoomTypeOption(TextKeys.CreateRoom.PrivateOption, TextKeys.CreateRoom.PrivateOptionHint, _privateMark);
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = Columns();
            Panel ambience = Ambience();
            columns.Widgets.Add(BuildCreateCard());
            columns.Widgets.Add(ambience);
            StackPanel.SetProportionType(ambience, ProportionType.Fill);

            return columns;
        }

        private VerticalStackPanel BuildCreateCard()
        {
            VerticalStackPanel card = Card(Sizes.CreateRoomCardWidth);
            card.VerticalAlignment = VerticalAlignment.Center;

            var options = new HorizontalStackPanel
            {
                Spacing = OptionSpacing,
            };
            _publicOption.Click += PublicOptionOnClick;
            _privateOption.Click += PrivateOptionOnClick;
            options.Widgets.Add(_publicOption);
            options.Widgets.Add(_privateOption);
            SelectRoomType(true);

            card.Widgets.Add(CardHeader(TextKeys.CreateRoom.Title, TextKeys.CreateRoom.Hint));
            card.Widgets.Add(options);
            card.Widgets.Add(Hint(TextKeys.CreateRoom.SharedRulesHint));
            card.Widgets.Add(BuildActionButtons());

            return card;
        }

        private static Button BuildRoomTypeOption(string titleKey, string hintKey, Panel radioMark)
        {
            var texts = new VerticalStackPanel
            {
                Spacing = OptionTextSpacing,
            };
            texts.Widgets.Add(new LocalizedLabel(titleKey)
            {
                Font = Fonts.Control,
                TextColor = Theme.Ink,
            });
            texts.Widgets.Add(Hint(hintKey));

            var content = new HorizontalStackPanel
            {
                Spacing = OptionSpacing,
            };
            content.Widgets.Add(texts);
            content.Widgets.Add(radioMark);
            StackPanel.SetProportionType(texts, ProportionType.Fill);

            var option = new Button
            {
                Content = content,
                Width = OptionWidth,
                Padding = _optionPadding,
                BorderThickness = Sizes.Border,
            };

            return option;
        }

        private static Panel BuildRadioMark()
        {
            var mark = new Panel
            {
                Width = RadioMarkSize,
                Height = RadioMarkSize,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Top,
            };
            mark.Widgets.Add(new Panel
            {
                Width = RadioDotSize,
                Height = RadioDotSize,
                Background = Theme.MintInkBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });

            return mark;
        }

        private HorizontalStackPanel BuildActionButtons()
        {
            LocalizedButton createButton = PrimaryButton(TextKeys.CreateRoom.CreateRoomButton);
            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackButton);
            createButton.Click += CreateButtonOnClick;
            backButton.Click += BackButtonOnClick;

            HorizontalStackPanel actions = Row();
            actions.Widgets.Add(createButton);
            actions.Widgets.Add(backButton);

            return actions;
        }

        private void SelectRoomType(bool isPublic)
        {
            ShowOptionState(_publicOption, _publicMark, isPublic);
            ShowOptionState(_privateOption, _privateMark, !isPublic);
        }

        private static void ShowOptionState(Button option, Panel radioMark, bool isSelected)
        {
            option.Background = isSelected ? Theme.MintTintBrush : Theme.SurfaceBrush;
            option.OverBackground = option.Background;
            option.PressedBackground = option.Background;
            option.Border = isSelected ? Theme.MintLineBrush : Theme.StrongLineBrush;

            radioMark.Background = isSelected ? Theme.MintBrush : Theme.SurfaceBrush;
            radioMark.Border = isSelected ? Theme.MintLineBrush : Theme.StrongLineBrush;
            radioMark.Widgets[0].Visible = isSelected;
        }

        private void PublicOptionOnClick(object sender, MyraEventArgs e)
        {
            SelectRoomType(true);
        }

        private void PrivateOptionOnClick(object sender, MyraEventArgs e)
        {
            SelectRoomType(false);
        }

        private void CreateButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Room;
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Rooms;
        }
    }
}
