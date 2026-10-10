using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class RoomsScreen : Screen
    {
        private const int EmptyStateSpacing = 4;

        internal RoomsScreen()
            : base(TextKeys.Rooms.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = CreatePage();
            page.Spacing = Metrics.FieldSpacing;
            page.Widgets.Add(BuildSearchRow());
            page.Widgets.Add(BuildAvailableRoomsHeader());

            page.Widgets.Add(BuildRoomsEmptyState());
            page.Widgets.Add(CreateHint(TextKeys.Rooms.PrivateRoomsHint));

            var filler = new Panel();
            page.Widgets.Add(filler);
            StackPanel.SetProportionType(filler, ProportionType.Fill);

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(CreateBackBar(backButton));

            return page;
        }

        private HorizontalStackPanel BuildSearchRow()
        {
            HorizontalStackPanel searchRow = CreateButtonRow();

            var searchField = new LabeledTextBox(TextKeys.Rooms.SearchPlaceholder, false);
            LocalizedButton searchButton = CreateSecondaryButton(TextKeys.Rooms.SearchButton);
            searchButton.VerticalAlignment = VerticalAlignment.Bottom;
            LocalizedButton createRoomButton = CreatePrimaryButton(TextKeys.Rooms.CreateRoomButton);
            createRoomButton.VerticalAlignment = VerticalAlignment.Bottom;
            createRoomButton.Click += CreateRoomButtonOnClick;

            searchRow.Widgets.Add(searchField);
            searchRow.Widgets.Add(searchButton);
            searchRow.Widgets.Add(createRoomButton);
            StackPanel.SetProportionType(searchField, ProportionType.Fill);

            return searchRow;
        }

        private static HorizontalStackPanel BuildAvailableRoomsHeader()
        {
            HorizontalStackPanel roomsHeader = CreateButtonRow();

            LocalizedLabel title = CreateTitle(TextKeys.Rooms.AvailableRoomsTitle);
            title.VerticalAlignment = VerticalAlignment.Center;

            Panel publicOnlyChip = CreateStatusBadge(TextKeys.Rooms.PublicOnlyBadge);

            LocalizedButton refreshButton = CreateSmallSecondaryButton(TextKeys.Rooms.RefreshButton);
            refreshButton.VerticalAlignment = VerticalAlignment.Center;

            var spacer = new Panel();
            roomsHeader.Widgets.Add(title);
            roomsHeader.Widgets.Add(publicOnlyChip);
            roomsHeader.Widgets.Add(spacer);
            roomsHeader.Widgets.Add(refreshButton);
            StackPanel.SetProportionType(spacer, ProportionType.Fill);

            return roomsHeader;
        }

        private static Panel BuildRoomsEmptyState()
        {
            var emptyState = new VerticalStackPanel
            {
                Spacing = EmptyStateSpacing,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var title = new LocalizedLabel(TextKeys.Rooms.EmptyStateTitle)
            {
                Font = Fonts.Body,
                TextColor = Theme.Ink,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            LocalizedLabel hint = CreateHint(TextKeys.Rooms.EmptyStateHint);
            hint.HorizontalAlignment = HorizontalAlignment.Center;
            emptyState.Widgets.Add(title);
            emptyState.Widgets.Add(hint);

            var emptyStateFrame = new Panel
            {
                Padding = Metrics.CardPadding,
                Background = Theme.SurfaceSunkenBrush,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            emptyStateFrame.Widgets.Add(emptyState);

            return emptyStateFrame;
        }

        private void CreateRoomButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.CreateRoom;
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
