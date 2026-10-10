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

        private readonly VerticalStackPanel _visibilityOptionsPlaceholder = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };

        private bool _isPublic = true;

        internal CreateRoomScreen()
            : base(TextKeys.CreateRoom.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel card = CreateCard(CardWidth);
            card.VerticalAlignment = VerticalAlignment.Center;
            card.Widgets.Add(CreateCardHeader(TextKeys.CreateRoom.Title, TextKeys.CreateRoom.Hint));
            card.Widgets.Add(_visibilityOptionsPlaceholder);
            RenderVisibilityOptions();
            card.Widgets.Add(CreateParagraph(TextKeys.CreateRoom.SharedRulesHint));
            card.Widgets.Add(BuildActionsRow());

            HorizontalStackPanel columns = CreateColumns();
            columns.Widgets.Add(card);
            columns.Widgets.Add(CreateAmbiencePanel());

            VerticalStackPanel page = CreatePage();
            page.Widgets.Add(columns);
            StackPanel.SetProportionType(columns, ProportionType.Fill);

            return page;
        }

        private void RenderVisibilityOptions()
        {
            LocalizedButton publicOptionButton = BuildOptionButton(TextKeys.CreateRoom.PublicOption, _isPublic);
            LocalizedButton privateOptionButton = BuildOptionButton(TextKeys.CreateRoom.PrivateOption, !_isPublic);
            publicOptionButton.Click += PublicOptionButtonOnClick;
            privateOptionButton.Click += PrivateOptionButtonOnClick;

            HorizontalStackPanel optionsRow = CreateButtonRow();
            optionsRow.Widgets.Add(BuildOptionCard(publicOptionButton, TextKeys.CreateRoom.PublicOptionHint, _isPublic));
            optionsRow.Widgets.Add(BuildOptionCard(privateOptionButton, TextKeys.CreateRoom.PrivateOptionHint, !_isPublic));

            _visibilityOptionsPlaceholder.Widgets.Clear();
            _visibilityOptionsPlaceholder.Widgets.Add(optionsRow);
        }

        private void SelectVisibility(bool isPublic)
        {
            _isPublic = isPublic;
            RenderVisibilityOptions();
        }

        private static LocalizedButton BuildOptionButton(string titleKey, bool isSelected)
        {
            LocalizedButton optionButton = isSelected ? CreatePrimaryButton(titleKey) : CreateSecondaryButton(titleKey);
            optionButton.HorizontalAlignment = HorizontalAlignment.Stretch;

            return optionButton;
        }

        private static Panel BuildOptionCard(LocalizedButton optionButton, string hintKey, bool isSelected)
        {
            var optionContent = new VerticalStackPanel { Spacing = Metrics.CardHeaderSpacing };
            optionContent.Widgets.Add(optionButton);
            optionContent.Widgets.Add(CreateHint(hintKey));

            var optionCard = new Panel
            {
                Width = OptionCardWidth,
                Padding = Metrics.CardPadding,
                Background = isSelected ? Theme.MintTintBrush : Theme.SurfaceBrush,
                Border = isSelected ? Theme.MintLineBrush : Theme.LineBrush,
                BorderThickness = Sizes.Border,
            };
            optionCard.Widgets.Add(optionContent);

            return optionCard;
        }

        private HorizontalStackPanel BuildActionsRow()
        {
            HorizontalStackPanel actionsRow = CreateButtonRow();

            LocalizedButton createButton = CreatePrimaryButton(TextKeys.CreateRoom.CreateRoomButton);
            createButton.Click += CreateButtonOnClick;
            actionsRow.Widgets.Add(createButton);

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackButton);
            backButton.Click += BackButtonOnClick;
            actionsRow.Widgets.Add(backButton);

            return actionsRow;
        }

        private void PublicOptionButtonOnClick(object sender, MyraEventArgs e)
        {
            SelectVisibility(true);
        }

        private void PrivateOptionButtonOnClick(object sender, MyraEventArgs e)
        {
            SelectVisibility(false);
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
