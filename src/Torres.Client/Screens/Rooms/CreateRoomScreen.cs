using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class CreateRoomScreen : Screen
    {
        private const int CardWidth = 460;
        private const int OptionCardWidth = 200;

        private readonly VerticalStackPanel _visibilityOptionsPlaceholder = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly LocalizedButton _createButton = ButtonFactory.CreatePrimaryButton(TextKeys.CreateRoom.CreateRoomButton);
        private readonly CreateRoomFlow _createRoomFlow;

        private bool _isPublic = true;

        internal CreateRoomScreen(CreateRoomFlow createRoomFlow)
            : base(TextKeys.CreateRoom.HeaderLabel, true)
        {
            _createRoomFlow = createRoomFlow;
        }

        internal override void Open()
        {
            _noticeBox.Hide();
            RequestedScreen = _createRoomFlow.FindRedirect();
        }

        internal override void Update(GameTime gameTime)
        {
            ScreenOutcome? outcome = _createRoomFlow.TakeOutcome();
            if (outcome?.Notice is Notice notice)
            {
                DisplayNotice(_noticeBox, notice);
            }

            if (outcome?.NextScreen is ScreenId nextScreen)
            {
                RequestedScreen = nextScreen;
            }

            _createButton.Enabled = !_createRoomFlow.IsCreating;
        }

        protected override Widget Build()
        {
            VerticalStackPanel card = PanelFactory.CreateCard(CardWidth);
            card.VerticalAlignment = VerticalAlignment.Center;
            card.Widgets.Add(PanelFactory.CreateCardHeader(TextKeys.CreateRoom.Title, TextKeys.CreateRoom.Hint));
            card.Widgets.Add(_visibilityOptionsPlaceholder);
            RenderVisibilityOptions();
            card.Widgets.Add(LabelFactory.CreateParagraph(TextKeys.CreateRoom.SharedRulesHint));
            card.Widgets.Add(_noticeBox);
            card.Widgets.Add(BuildActionsRow());

            HorizontalStackPanel columns = PanelFactory.CreateColumns();
            columns.Widgets.Add(card);
            columns.Widgets.Add(PanelFactory.CreateAmbiencePanel());

            VerticalStackPanel page = PanelFactory.CreatePage();
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

            HorizontalStackPanel optionsRow = PanelFactory.CreateButtonRow();
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
            LocalizedButton optionButton = isSelected ? ButtonFactory.CreatePrimaryButton(titleKey) : ButtonFactory.CreateSecondaryButton(titleKey);
            optionButton.HorizontalAlignment = HorizontalAlignment.Stretch;

            return optionButton;
        }

        private static Panel BuildOptionCard(LocalizedButton optionButton, string hintKey, bool isSelected)
        {
            var optionContent = new VerticalStackPanel { Spacing = Metrics.CardHeaderSpacing };
            optionContent.Widgets.Add(optionButton);
            optionContent.Widgets.Add(LabelFactory.CreateHint(hintKey));

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
            HorizontalStackPanel actionsRow = PanelFactory.CreateButtonRow();

            _createButton.Click += CreateButtonOnClick;
            actionsRow.Widgets.Add(_createButton);

            LocalizedButton backButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.BackButton);
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
            if (_createRoomFlow.IsCreating)
            {
                return;
            }

            _noticeBox.Hide();
            _createRoomFlow.StartCreation(_isPublic);
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Rooms;
        }
    }
}
