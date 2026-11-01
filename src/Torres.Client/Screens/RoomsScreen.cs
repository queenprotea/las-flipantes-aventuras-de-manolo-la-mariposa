using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class RoomsScreen : Screen
    {
        private const int EmptyStateSpacing = 4;

        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _session;
        private readonly LabeledTextBox _searchField = new LabeledTextBox(TextKeys.Rooms.SearchPlaceholder, false);
        private readonly VerticalStackPanel _roomsPlaceholder = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
        private readonly NoticeBox _noticeBox = new NoticeBox();

        private Task<RoomListResponse>? _listRequest;
        private Task<JoinRoomResult>? _joinRequest;
        private bool _isShowingSearch;

        internal RoomsScreen(GameSessionConnection gameSessionConnection, PlayerSession session)
            : base(TextKeys.Rooms.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(session);

            _gameSessionConnection = gameSessionConnection;
            _session = session;
        }

        internal override void Open()
        {
            _noticeBox.Hide();
            if (!_session.IsLoggedIn || !_gameSessionConnection.IsOpen)
            {
                RequestedScreen = ScreenId.Login;
            }
            else if (_session.CurrentRoom is not null)
            {
                RequestedScreen = ScreenId.Room;
            }
            else
            {
                RequestRooms();
            }
        }

        internal override void Update(GameTime gameTime)
        {
            FollowListRequest();
            FollowJoinRequest();
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = CreatePage();
            page.Spacing = Metrics.FieldSpacing;
            page.Widgets.Add(BuildSearchRow());
            page.Widgets.Add(_noticeBox);
            page.Widgets.Add(BuildAvailableRoomsHeader());
            page.Widgets.Add(_roomsPlaceholder);
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

            LocalizedButton searchButton = CreateSecondaryButton(TextKeys.Rooms.SearchButton);
            searchButton.VerticalAlignment = VerticalAlignment.Bottom;
            searchButton.Click += SearchButtonOnClick;
            LocalizedButton createRoomButton = CreatePrimaryButton(TextKeys.Rooms.CreateRoomButton);
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
            HorizontalStackPanel roomsHeader = CreateButtonRow();

            LocalizedLabel title = CreateTitle(TextKeys.Rooms.AvailableRoomsTitle);
            title.VerticalAlignment = VerticalAlignment.Center;

            Panel publicOnlyChip = CreateStatusBadge(TextKeys.Rooms.PublicOnlyBadge);

            LocalizedButton refreshButton = CreateSmallSecondaryButton(TextKeys.Rooms.RefreshButton);
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

        private void RequestRooms()
        {
            _isShowingSearch = false;
            _listRequest = _gameSessionConnection.GetRoomsAsync();
        }

        private void FollowListRequest()
        {
            if ((_listRequest is null) || !_listRequest.IsCompleted)
            {
                return;
            }

            Task<RoomListResponse> completedList = _listRequest;
            _listRequest = null;
            if (completedList.IsCompletedSuccessfully)
            {
                RenderRooms(completedList.Result.Rooms);
            }
            else
            {
                _noticeBox.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, null);
            }
        }

        private void FollowJoinRequest()
        {
            if ((_joinRequest is null) || !_joinRequest.IsCompleted)
            {
                return;
            }

            Task<JoinRoomResult> completedJoin = _joinRequest;
            _joinRequest = null;
            if (completedJoin.IsCompletedSuccessfully)
            {
                ShowJoinOutcome(completedJoin.Result);
            }
            else
            {
                _noticeBox.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, null);
            }
        }

        private void ShowJoinOutcome(JoinRoomResult joining)
        {
            switch (joining.Status)
            {
                case JoinRoomStatus.Joined:
                    EnterRoom(joining.Room);
                    break;

                case JoinRoomStatus.RoomFull:
                    ShowRefusal(TextKeys.Alerts.RoomFullTitle, TextKeys.Alerts.RoomFullBody);
                    break;

                case JoinRoomStatus.RoomNotFound:
                    ShowRefusal(TextKeys.Alerts.RoomNotFoundTitle, TextKeys.Alerts.RoomNotFoundBody);
                    break;

                case JoinRoomStatus.RoomInMatch:
                    _noticeBox.ShowTitle(TextKeys.Rooms.RoomInMatchError);
                    break;

                case JoinRoomStatus.AlreadyInRoom:
                    _noticeBox.ShowTitle(TextKeys.Rooms.AlreadyInRoomError);
                    break;

                case JoinRoomStatus.Banned:
                    _noticeBox.ShowBan(joining.BanEndsAtUtc);
                    break;

                case JoinRoomStatus.NotIdentified:
                    RequestedScreen = ScreenId.Login;
                    break;

                case JoinRoomStatus.DatabaseUnavailable:
                    _noticeBox.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, null);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(joining), joining.Status, null);
            }
        }

        private void ShowRefusal(string titleKey, string bodyKey)
        {
            _noticeBox.ShowMessage(titleKey, bodyKey, null);
            RequestRooms();
        }

        private void EnterRoom(Room? joinedRoom)
        {
            if (joinedRoom is null)
            {
                _noticeBox.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, null);
                return;
            }

            _session.EnterRoom(joinedRoom);
            RequestedScreen = ScreenId.Room;
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
            HorizontalStackPanel rowContent = CreateButtonRow();
            rowContent.Widgets.Add(new Label
            {
                Text = room.Code,
                Font = Fonts.Title,
                TextColor = Theme.MintInk,
                VerticalAlignment = VerticalAlignment.Center,
            });
            if (room.Visibility == RoomVisibility.Private)
            {
                rowContent.Widgets.Add(CreateStatusBadge(TextKeys.Room.PrivateBadge));
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

            Panel roomRow = CreateItemCard(rowContent);

            return roomRow;
        }

        private Widget BuildJoinControl(RoomSummary room)
        {
            bool isJoinable = (room.PlayerCount < room.MaxPlayers) && (room.State == RoomState.Waiting);
            if (!isJoinable)
            {
                Panel fullBadge = CreateStatusBadge(TextKeys.Rooms.FullButton);
                return fullBadge;
            }

            LocalizedButton joinButton = CreatePrimaryButton(TextKeys.Rooms.JoinButton);
            joinButton.VerticalAlignment = VerticalAlignment.Center;
            int roomId = room.RoomId;
            joinButton.Click += (clickedButton, clickArguments) => StartJoin(roomId);

            return joinButton;
        }

        private Panel BuildEmptyState()
        {
            string titleKey = _isShowingSearch ? TextKeys.Alerts.RoomNotFoundTitle : TextKeys.Rooms.EmptyStateTitle;
            string hintKey = _isShowingSearch ? TextKeys.Alerts.RoomNotFoundBody : TextKeys.Rooms.EmptyStateHint;
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
            LocalizedLabel hint = CreateHint(hintKey);
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
            if (_joinRequest is not null)
            {
                return;
            }

            _noticeBox.Hide();
            _joinRequest = _gameSessionConnection.JoinRoomAsync(roomId);
        }

        private void SearchButtonOnClick(object sender, MyraEventArgs e)
        {
            _noticeBox.Hide();
            string roomCode = _searchField.Value.Trim();
            if (roomCode.Length == 0)
            {
                RequestRooms();
            }
            else
            {
                _isShowingSearch = true;
                _listRequest = _gameSessionConnection.FindRoomAsync(roomCode);
            }
        }

        private void RefreshButtonOnClick(object sender, MyraEventArgs e)
        {
            _noticeBox.Hide();
            _searchField.Box.Text = string.Empty;
            RequestRooms();
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
