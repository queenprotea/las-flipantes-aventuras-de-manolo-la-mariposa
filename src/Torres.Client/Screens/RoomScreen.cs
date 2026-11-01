using System;
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
    internal sealed class RoomScreen : Screen
    {
        private const int LoneMemberCount = 1;

        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _session;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly VerticalStackPanel _contentPlaceholder = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };

        private Room? _shownRoom;
        private Task<LeaveRoomResult>? _leaveRequest;
        private Task<KickMemberResult>? _kickRequest;

        internal RoomScreen(GameSessionConnection gameSessionConnection, PlayerSession session)
            : base(TextKeys.Room.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(session);

            _gameSessionConnection = gameSessionConnection;
            _session = session;
        }

        internal override void Open()
        {
            _noticeBox.Hide();
            HideConfirmation();
            _shownRoom = null;
            if (_session.CurrentRoom is null)
            {
                RequestedScreen = ScreenId.MainMenu;
            }
        }

        internal override void Update(GameTime gameTime)
        {
            Room? currentRoom = _session.CurrentRoom;
            if ((currentRoom is not null) && !ReferenceEquals(currentRoom, _shownRoom))
            {
                Render(currentRoom);
            }

            FollowLeaveRequest();
            FollowKickRequest();
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = CreatePage();
            page.Spacing = Metrics.FieldSpacing;
            page.Widgets.Add(_noticeBox);
            page.Widgets.Add(ConfirmationPlaceholder);
            page.Widgets.Add(_contentPlaceholder);

            return page;
        }

        private void Render(Room room)
        {
            _shownRoom = room;
            bool isViewerHost = room.HostMemberId == room.ViewerMemberId;
            _contentPlaceholder.Widgets.Clear();
            _contentPlaceholder.Widgets.Add(BuildTopBar(room, isViewerHost));
            if (!isViewerHost)
            {
                LocalizedLabel hostHint = CreateHint(TextKeys.Room.HostOnlyHint);
                hostHint.HorizontalAlignment = HorizontalAlignment.Right;
                _contentPlaceholder.Widgets.Add(hostHint);
            }

            _contentPlaceholder.Widgets.Add(BuildPlayersHeader(room));
            _contentPlaceholder.Widgets.Add(BuildPlayersList(room, isViewerHost));
            _contentPlaceholder.Widgets.Add(BuildRoomRankingSection());
        }

        private HorizontalStackPanel BuildTopBar(Room room, bool isViewerHost)
        {
            HorizontalStackPanel roomActionsBar = CreateButtonRow();
            roomActionsBar.HorizontalAlignment = HorizontalAlignment.Stretch;
            roomActionsBar.VerticalAlignment = VerticalAlignment.Top;

            var roomCodeLabel = new Label
            {
                Text = room.Code,
                Font = Fonts.Title,
                TextColor = Theme.MintInk,
                VerticalAlignment = VerticalAlignment.Center,
            };
            roomActionsBar.Widgets.Add(roomCodeLabel);

            LocalizedButton copyButton = CreateSmallSecondaryButton(TextKeys.Room.CopyButton);
            copyButton.VerticalAlignment = VerticalAlignment.Center;
            copyButton.Click += CopyButtonOnClick;
            roomActionsBar.Widgets.Add(copyButton);

            string visibilityBadgeKey = room.Visibility == RoomVisibility.Public ? TextKeys.Room.PublicBadge : TextKeys.Room.PrivateBadge;
            roomActionsBar.Widgets.Add(CreateStatusBadge(visibilityBadgeKey));

            var spacer = new Panel();
            roomActionsBar.Widgets.Add(spacer);
            StackPanel.SetProportionType(spacer, ProportionType.Fill);

            LocalizedButton inviteButton = CreateSecondaryButton(TextKeys.Room.InvitePlayersButton);
            inviteButton.Click += InviteButtonOnClick;
            roomActionsBar.Widgets.Add(inviteButton);

            LocalizedButton leaveButton = CreateDestructiveButton(TextKeys.Room.LeaveButton);
            leaveButton.Click += LeaveButtonOnClick;
            roomActionsBar.Widgets.Add(leaveButton);

            LocalizedButton startButton = CreatePrimaryButton(TextKeys.Room.StartMatchButton);
            startButton.Visible = isViewerHost;
            startButton.Enabled = false;
            roomActionsBar.Widgets.Add(startButton);

            return roomActionsBar;
        }

        private static HorizontalStackPanel BuildPlayersHeader(Room room)
        {
            HorizontalStackPanel playersHeader = CreateButtonRow();
            playersHeader.Widgets.Add(CreateTitle(TextKeys.Room.PlayersTitle));

            playersHeader.Widgets.Add(CreateMutedLabel(TextKeys.Room.OccupancyLabel, new[] { DisplayFormats.FormatInteger(room.Members.Count), DisplayFormats.FormatInteger(room.MaxPlayers) }));

            int activeMemberCount = room.Members.FindAll(IsActiveMember).Count;
            if (activeMemberCount != room.Members.Count)
            {
                playersHeader.Widgets.Add(CreateMutedLabel(TextKeys.Room.ActiveCountLabel, new[] { DisplayFormats.FormatInteger(activeMemberCount) }));
            }

            return playersHeader;
        }

        private static bool IsActiveMember(RoomMember member)
        {
            bool isActive = member.IsActive;

            return isActive;
        }

        private VerticalStackPanel BuildPlayersList(Room room, bool isViewerHost)
        {
            var playersList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (RoomMember member in room.Members)
            {
                bool canBeKicked = isViewerHost && (member.MemberId != room.ViewerMemberId);
                playersList.Widgets.Add(BuildPlayerRow(room, member, canBeKicked));
            }

            return playersList;
        }

        private Panel BuildPlayerRow(Room room, RoomMember member, bool canBeKicked)
        {
            HorizontalStackPanel itemRow = CreateButtonRow();

            HorizontalStackPanel nameLine = CreateButtonRow();
            nameLine.Widgets.Add(BuildColorSwatch(member.Color));
            nameLine.Widgets.Add(CreateUsernameLabel(member.DisplayName));
            if (member.MemberId == room.ViewerMemberId)
            {
                nameLine.Widgets.Add(CreateMutedLabel(TextKeys.Common.YouTag, null));
            }

            if (member.MemberId == room.HostMemberId)
            {
                nameLine.Widgets.Add(CreateStatusBadge(TextKeys.Room.HostBadge));
            }

            if (member.IsGuest)
            {
                nameLine.Widgets.Add(CreateStatusBadge(TextKeys.Room.GuestBadge));
            }

            itemRow.Widgets.Add(nameLine);
            StackPanel.SetProportionType(nameLine, ProportionType.Fill);
            if (!member.IsActive)
            {
                itemRow.Widgets.Add(CreateStatusBadge(TextKeys.Room.DisconnectedStatus));
            }

            if (canBeKicked)
            {
                LocalizedButton kickButton = CreateDestructiveButton(TextKeys.Room.RemovePlayerToolTip);
                kickButton.Click += (clickedButton, clickArguments) => AskKickConfirmation(member);
                itemRow.Widgets.Add(kickButton);
            }

            Panel playerRow = CreateItemCard(itemRow);

            return playerRow;
        }

        private static Panel BuildColorSwatch(SeatColor color)
        {
            var colorSwatch = new Panel
            {
                Width = Sizes.SeatSwatchSize,
                Height = Sizes.SeatSwatchSize,
                Background = SeatColors.BrushFor(color),
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return colorSwatch;
        }

        private static VerticalStackPanel BuildRoomRankingSection()
        {
            var rankingSection = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
            rankingSection.Widgets.Add(CreateTitle(TextKeys.Room.RoomRankingTitle));
            rankingSection.Widgets.Add(CreateEmptyState(TextKeys.Room.EmptyRankingTitle, TextKeys.Room.EmptyRankingHint));

            return rankingSection;
        }

        private static LocalizedLabel CreateMutedLabel(string textKey, string[]? textArguments)
        {
            var mutedLabel = new LocalizedLabel(textKey)
            {
                TextArguments = textArguments,
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return mutedLabel;
        }

        private void AskKickConfirmation(RoomMember member)
        {
            _noticeBox.Hide();
            NoticeBox question = ShowConfirmation(TextKeys.Alerts.RemovePlayerTitle, TextKeys.Alerts.RemovePlayerBody);
            question.SetTitleArguments(new[] { member.DisplayName });

            LocalizedButton kickConfirmButton = CreateDestructiveButton(TextKeys.Alerts.RemoveConfirmButton);
            kickConfirmButton.Click += (clickedButton, clickArguments) => StartKick(member.MemberId);
            LocalizedButton cancelButton = CreateSecondaryButton(TextKeys.Common.CancelButton);
            cancelButton.Click += CancelButtonOnClick;
            AddConfirmationButtons(kickConfirmButton, cancelButton);
        }

        private void AskLeaveConfirmation()
        {
            _noticeBox.Hide();
            ShowConfirmation(TextKeys.Alerts.LeaveRoomTitle, TextKeys.Alerts.LeaveRoomBody);

            LocalizedButton leaveConfirmButton = CreateDestructiveButton(TextKeys.Alerts.LeaveConfirmButton);
            leaveConfirmButton.Click += LeaveConfirmButtonOnClick;
            LocalizedButton stayButton = CreateSecondaryButton(TextKeys.Alerts.StayButton);
            stayButton.Click += StayButtonOnClick;
            AddConfirmationButtons(leaveConfirmButton, stayButton);
        }

        private void StartLeave()
        {
            HideConfirmation();
            _leaveRequest ??= _gameSessionConnection.LeaveRoomAsync();
        }

        private void StartKick(int targetMemberId)
        {
            HideConfirmation();
            _kickRequest ??= _gameSessionConnection.KickMemberAsync(targetMemberId);
        }

        private void FollowLeaveRequest()
        {
            if ((_leaveRequest is null) || !_leaveRequest.IsCompleted)
            {
                return;
            }

            Task<LeaveRoomResult> completedLeave = _leaveRequest;
            _leaveRequest = null;
            if (completedLeave.IsCompletedSuccessfully)
            {
                _session.LeaveRoom();
                RequestedScreen = ScreenId.MainMenu;
            }
            else
            {
                _noticeBox.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, null);
            }
        }

        private void FollowKickRequest()
        {
            if ((_kickRequest is null) || !_kickRequest.IsCompleted)
            {
                return;
            }

            Task<KickMemberResult> completedKick = _kickRequest;
            _kickRequest = null;
            if (!completedKick.IsCompletedSuccessfully)
            {
                _noticeBox.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, null);
            }
            else if (completedKick.Result == KickMemberResult.NotHost)
            {
                _noticeBox.ShowTitle(TextKeys.Room.HostOnlyHint);
            }
        }

        private void CopyButtonOnClick(object sender, MyraEventArgs e)
        {
            if (_shownRoom is not null)
            {
                TextCopy.Clipboard.SetText(_shownRoom.Code);
            }
        }

        private void InviteButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.InvitePlayers;
        }

        private void LeaveButtonOnClick(object sender, MyraEventArgs e)
        {
            if (_shownRoom?.Members.Count == LoneMemberCount)
            {
                AskLeaveConfirmation();
            }
            else
            {
                StartLeave();
            }
        }

        private void LeaveConfirmButtonOnClick(object sender, MyraEventArgs e)
        {
            StartLeave();
        }

        private void StayButtonOnClick(object sender, MyraEventArgs e)
        {
            HideConfirmation();
        }

        private void CancelButtonOnClick(object sender, MyraEventArgs e)
        {
            HideConfirmation();
        }
    }
}
