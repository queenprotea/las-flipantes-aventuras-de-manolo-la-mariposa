using System.Collections.Generic;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class RoomsScreen : Screen
    {
        private const int EmptyStateSpacing = 4;

        private readonly RoomsFlow _roomsFlow;
        private readonly LabeledTextBox _searchField = new LabeledTextBox(TextKeys.Rooms.SearchPlaceholder, false, AccountLimits.RoomCodeLength);
        private readonly VerticalStackPanel _roomsPlaceholder = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
        private readonly NoticeBox _noticeBox = new NoticeBox();


        internal RoomsScreen(RoomsFlow roomsFlow)
            : base(TextKeys.Rooms.HeaderLabel, true)
        {
            _roomsFlow = roomsFlow;
        }

        internal override void Open()
        {
            _noticeBox.Hide();
            RequestedScreen = _roomsFlow.Open();
        }

        internal override void Update(GameTime gameTime)
        {
            RoomsOutcome? outcome = _roomsFlow.TakeOutcome();
            if (outcome is not null)
            {
                Show(outcome);
            }
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = PanelFactory.CreatePage();
            page.Spacing = Metrics.FieldSpacing;
            page.Widgets.Add(BuildSearchRow());
            page.Widgets.Add(_noticeBox);
            page.Widgets.Add(BuildAvailableRoomsHeader());
            page.Widgets.Add(_roomsPlaceholder);
            page.Widgets.Add(LabelFactory.CreateHint(TextKeys.Rooms.PrivateRoomsHint));

            var filler = new Panel();
            page.Widgets.Add(filler);
            StackPanel.SetProportionType(filler, ProportionType.Fill);

            LocalizedButton backButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(PanelFactory.CreateBackBar(backButton));

            return page;
        }

        private HorizontalStackPanel BuildSearchRow()
        {
            HorizontalStackPanel searchRow = PanelFactory.CreateButtonRow();

            LocalizedButton searchButton = ButtonFactory.CreateSecondaryButton(TextKeys.Rooms.SearchButton);
            searchButton.VerticalAlignment = VerticalAlignment.Bottom;
            searchButton.Click += SearchButtonOnClick;
            LocalizedButton createRoomButton = ButtonFactory.CreatePrimaryButton(TextKeys.Rooms.CreateRoomButton);
            createRoomButton.VerticalAlignment = VerticalAlignment.Bottom;
            createRoomButton.Click += CreateRoomButtonOnClick;

            searchRow.Widgets.Add(_searchField);
            searchRow.Widgets.Add(searchButton);
            searchRow.Widgets.Add(createRoomButton);
            StackPanel.SetProportionType(_searchField, ProportionType.Fill);

            return searchRow;
        }

        private HorizontalStackPanel BuildAvailableRoomsHeader()
        {
            HorizontalStackPanel roomsHeader = PanelFactory.CreateButtonRow();

            LocalizedLabel title = LabelFactory.CreateTitle(TextKeys.Rooms.AvailableRoomsTitle);
            title.VerticalAlignment = VerticalAlignment.Center;

            Panel publicOnlyChip = PanelFactory.CreateStatusBadge(TextKeys.Rooms.PublicOnlyBadge);

            LocalizedButton refreshButton = ButtonFactory.CreateSmallSecondaryButton(TextKeys.Rooms.RefreshButton);
            refreshButton.VerticalAlignment = VerticalAlignment.Center;
            refreshButton.Click += RefreshButtonOnClick;

            var spacer = new Panel();
            roomsHeader.Widgets.Add(title);
            roomsHeader.Widgets.Add(publicOnlyChip);
            roomsHeader.Widgets.Add(spacer);
            roomsHeader.Widgets.Add(refreshButton);
            StackPanel.SetProportionType(spacer, ProportionType.Fill);

            return roomsHeader;
        }

        private void Show(RoomsOutcome outcome)
        {
            if (outcome.Rooms is List<RoomSummary> rooms)
            {
                RenderRooms(rooms);
            }

            if (outcome.Notice is Notice notice)
            {
                DisplayNotice(_noticeBox, notice);
            }

            if (outcome.NextScreen is ScreenId nextScreen)
            {
                RequestedScreen = nextScreen;
            }
        }

        private void RenderRooms(List<RoomSummary> rooms)
        {
            _roomsPlaceholder.Widgets.Clear();
            if (rooms.Count == 0)
            {
                _roomsPlaceholder.Widgets.Add(BuildEmptyState());
            }

            foreach (RoomSummary room in rooms)
            {
                _roomsPlaceholder.Widgets.Add(BuildRoomRow(room));
            }
        }

        private Panel BuildRoomRow(RoomSummary room)
        {
            HorizontalStackPanel rowContent = PanelFactory.CreateButtonRow();
            rowContent.Widgets.Add(new Label
            {
                Text = room.Code,
                Font = Fonts.Title,
                TextColor = Theme.MintInk,
                VerticalAlignment = VerticalAlignment.Center,
            });
            if (room.Visibility == RoomVisibility.Private)
            {
                rowContent.Widgets.Add(PanelFactory.CreateStatusBadge(TextKeys.Room.PrivateBadge));
            }

            var hostLabel = new LocalizedLabel(TextKeys.Rooms.HostLabel)
            {
                Font = Fonts.Body,
                TextColor = Theme.Ink,
                VerticalAlignment = VerticalAlignment.Center,
                TextArguments = new[] { room.HostDisplayName },
            };
            rowContent.Widgets.Add(hostLabel);
            StackPanel.SetProportionType(hostLabel, ProportionType.Fill);

            var occupancyLabel = new LocalizedLabel(TextKeys.Rooms.OccupancyBadge)
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
                TextArguments = new[] { DisplayFormats.FormatInteger(room.PlayerCount), DisplayFormats.FormatInteger(room.MaxPlayers) },
            };
            rowContent.Widgets.Add(occupancyLabel);
            rowContent.Widgets.Add(BuildJoinControl(room));

            Panel roomRow = PanelFactory.CreateItemCard(rowContent);

            return roomRow;
        }

        private Widget BuildJoinControl(RoomSummary room)
        {
            bool isJoinable = (room.PlayerCount < room.MaxPlayers) && (room.State == RoomState.Waiting);
            if (!isJoinable)
            {
                Panel fullBadge = PanelFactory.CreateStatusBadge(TextKeys.Rooms.FullButton);
                return fullBadge;
            }

            LocalizedButton joinButton = ButtonFactory.CreatePrimaryButton(TextKeys.Rooms.JoinButton);
            joinButton.VerticalAlignment = VerticalAlignment.Center;
            int roomId = room.RoomId;
            joinButton.Click += (clickedButton, clickArguments) => StartJoin(roomId);

            return joinButton;
        }

        private Panel BuildEmptyState()
        {
            string titleKey = _roomsFlow.IsShowingSearch ? TextKeys.Alerts.RoomNotFoundTitle : TextKeys.Rooms.EmptyStateTitle;
            string hintKey = _roomsFlow.IsShowingSearch ? TextKeys.Alerts.RoomNotFoundBody : TextKeys.Rooms.EmptyStateHint;
            var emptyState = new VerticalStackPanel
            {
                Spacing = EmptyStateSpacing,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var title = new LocalizedLabel(titleKey)
            {
                Font = Fonts.Body,
                TextColor = Theme.Ink,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            LocalizedLabel hint = LabelFactory.CreateHint(hintKey);
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

        private void StartJoin(int roomId)
        {
            if (_roomsFlow.StartJoin(roomId))
            {
                _noticeBox.Hide();
            }
        }

        private void SearchButtonOnClick(object sender, MyraEventArgs e)
        {
            _noticeBox.Hide();
            _roomsFlow.Search(_searchField.Value);
        }

        private void RefreshButtonOnClick(object sender, MyraEventArgs e)
        {
            _noticeBox.Hide();
            _searchField.Box.Text = string.Empty;
            _roomsFlow.RequestRooms();
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
