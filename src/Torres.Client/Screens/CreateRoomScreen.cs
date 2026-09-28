using System;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class CreateRoomScreen : Screen
    {
        private const int CardWidth = 460;
        private const int OptionCardWidth = 200;

        private bool _isPublic = true;
        private VerticalStackPanel? _optionsPlaceholder;

        internal CreateRoomScreen()
            : base(TextKeys.CreateRoom.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = Page();

            HorizontalStackPanel columns = Columns();

            VerticalStackPanel card = Card(CardWidth);
            card.VerticalAlignment = VerticalAlignment.Center;
            card.Widgets.Add(CardHeader(TextKeys.CreateRoom.Title, TextKeys.CreateRoom.Hint));

            var optionsPlaceholder = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
            _optionsPlaceholder = optionsPlaceholder;
            card.Widgets.Add(optionsPlaceholder);
            RenderOptions();

            card.Widgets.Add(Paragraph(TextKeys.CreateRoom.SharedRulesHint));
            card.Widgets.Add(BuildActionsRow());

            columns.Widgets.Add(card);
            columns.Widgets.Add(Ambience());

            page.Widgets.Add(columns);
            StackPanel.SetProportionType(columns, ProportionType.Fill);

            return page;
        }

        private void RenderOptions()
        {
            _optionsPlaceholder!.Widgets.Clear();

            HorizontalStackPanel optionsRow = Row();

            Panel publicCard = BuildOptionCard(
                TextKeys.CreateRoom.PublicOption,
                TextKeys.CreateRoom.PublicOptionHint,
                isSelected: _isPublic,
                onSelect: () => SelectVisibility(true));
            publicCard.Width = OptionCardWidth;
            optionsRow.Widgets.Add(publicCard);

            Panel privateCard = BuildOptionCard(
                TextKeys.CreateRoom.PrivateOption,
                TextKeys.CreateRoom.PrivateOptionHint,
                isSelected: !_isPublic,
                onSelect: () => SelectVisibility(false));
            privateCard.Width = OptionCardWidth;
            optionsRow.Widgets.Add(privateCard);

            _optionsPlaceholder.Widgets.Add(optionsRow);
        }

        private void SelectVisibility(bool isPublic)
        {
            _isPublic = isPublic;
            RenderOptions();
        }

        private static Panel BuildOptionCard(string titleKey, string hintKey, bool isSelected, Action onSelect)
        {
            LocalizedButton titleButton = isSelected ? PrimaryButton(titleKey) : SecondaryButton(titleKey);
            titleButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            titleButton.Click += (sender, arguments) => onSelect();

            var content = new VerticalStackPanel { Spacing = Metrics.CardHeaderSpacing };
            content.Widgets.Add(titleButton);
            content.Widgets.Add(Hint(hintKey));

            var card = new Panel
            {
                Padding = Metrics.CardPadding,
                Background = isSelected ? Theme.MintTintBrush : Theme.SurfaceBrush,
                Border = isSelected ? Theme.MintLineBrush : Theme.LineBrush,
                BorderThickness = Sizes.Border,
            };
            card.Widgets.Add(content);
            return card;
        }

        private HorizontalStackPanel BuildActionsRow()
        {
            HorizontalStackPanel bar = Row();

            LocalizedButton createButton = PrimaryButton(TextKeys.CreateRoom.CreateRoomButton);
            // TODO: connect to CreateRoomAsync(_isPublic).
            createButton.Click += OnCreateClick;
            bar.Widgets.Add(createButton);

            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackButton);
            backButton.Click += OnBackClick;
            bar.Widgets.Add(backButton);

            return bar;
        }

        private void OnCreateClick(object sender, MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.Room;
        }

        private void OnBackClick(object sender, MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.Rooms;
        }
    }
}
