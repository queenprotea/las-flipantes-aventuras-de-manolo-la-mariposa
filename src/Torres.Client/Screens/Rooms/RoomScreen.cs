using System.Collections.Generic;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Avatars;
using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class RoomScreen : Screen
    {
        private static readonly IReadOnlyList<RankingColumn> _rankingStatColumns = new RankingColumn[]
        {
            new RankingColumn(TextKeys.GlobalRanking.PointsColumn, Sizes.RoomRankingStatColumnWidth),
            new RankingColumn(TextKeys.GlobalRanking.WinsColumn, Sizes.RoomRankingStatColumnWidth),
        };

        private readonly RoomFlow _roomFlow;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly VerticalStackPanel _topBarPlaceholder = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
        private readonly VerticalStackPanel _playersPlaceholder = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
        private readonly VerticalStackPanel _rankingPlaceholder = new VerticalStackPanel();
        private readonly ChatSection _chatSection;
        private readonly AvatarViewSet _avatarViews;

        private Room? _shownRoom;

        internal RoomScreen(RoomFlow roomFlow, ChatFlow chatFlow, AvatarCache avatarCache)
            : base(TextKeys.Room.HeaderLabel, true)
        {
            _roomFlow = roomFlow;
            _chatSection = new ChatSection(chatFlow, Sizes.RoomSideColumnWidth);
            _avatarViews = new AvatarViewSet(avatarCache, Sizes.SmallAvatarSize);
        }

        internal void ShowNotice(string noticeKey)
        {
            _noticeBox.ShowTitle(noticeKey);
        }

        internal override void Open()
        {
            _noticeBox.Hide();
            HideConfirmation();
            _shownRoom = null;
            RequestedScreen = _roomFlow.FindRedirect();

            _chatSection.Open();
        }

        internal override void Update(GameTime gameTime)
        {
            Room? currentRoom = _roomFlow.CurrentRoom;
            if ((currentRoom is not null) && !ReferenceEquals(currentRoom, _shownRoom))
            {
                Render(currentRoom);
            }

            RoomOutcome? outcome = _roomFlow.TakeOutcome();
            if (outcome is not null)
            {
                ShowOutcome(outcome);
            }

            _chatSection.Update();
            _avatarViews.Refresh();
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = PanelFactory.CreatePage();
            page.Spacing = Metrics.FieldSpacing;
            page.Widgets.Add(_noticeBox);
            page.Widgets.Add(ConfirmationPlaceholder);
            page.Widgets.Add(_topBarPlaceholder);

            HorizontalStackPanel columns = BuildColumns();
            page.Widgets.Add(columns);
            StackPanel.SetProportionType(columns, ProportionType.Fill);

            return page;
        }

        private HorizontalStackPanel BuildColumns()
        {
            HorizontalStackPanel columns = PanelFactory.CreateColumns();
            columns.Widgets.Add(_playersPlaceholder);
            StackPanel.SetProportionType(_playersPlaceholder, ProportionType.Fill);
            columns.Widgets.Add(BuildSideColumn());

            return columns;
        }

        private VerticalStackPanel BuildSideColumn()
        {
            var sideColumn = new VerticalStackPanel
            {
                Spacing = Metrics.FieldSpacing,
                Width = Sizes.RoomSideColumnWidth,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            sideColumn.Widgets.Add(BuildRoomRankingSection());
            sideColumn.Widgets.Add(_chatSection.Root);
            StackPanel.SetProportionType(_chatSection.Root, ProportionType.Fill);

            return sideColumn;
        }

        private void Render(Room room)
        {
            _shownRoom = room;
            _avatarViews.Clear();
            bool isViewerHost = room.HostMemberId == room.ViewerMemberId;
            _topBarPlaceholder.Widgets.Clear();
            _topBarPlaceholder.Widgets.Add(BuildTopBar(room, isViewerHost));

            _playersPlaceholder.Widgets.Clear();
            _playersPlaceholder.Widgets.Add(BuildPlayersHeader(room));
            _playersPlaceholder.Widgets.Add(BuildPlayersList(room, isViewerHost));
            if (!isViewerHost)
            {
                _playersPlaceholder.Widgets.Add(LabelFactory.CreateHint(TextKeys.Room.HostOnlyHint));
            }

            ShowRanking(room);
        }

        private HorizontalStackPanel BuildTopBar(Room room, bool isViewerHost)
        {
            HorizontalStackPanel roomActionsBar = PanelFactory.CreateButtonRow();
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

            LocalizedButton copyButton = ButtonFactory.CreateSmallSecondaryButton(TextKeys.Room.CopyButton);
            copyButton.VerticalAlignment = VerticalAlignment.Center;
            copyButton.Click += CopyButtonOnClick;
            roomActionsBar.Widgets.Add(copyButton);

            string visibilityBadgeKey = room.Visibility == RoomVisibility.Public ? TextKeys.Room.PublicBadge : TextKeys.Room.PrivateBadge;
            roomActionsBar.Widgets.Add(PanelFactory.CreateStatusBadge(visibilityBadgeKey));

            var spacer = new Panel();
            roomActionsBar.Widgets.Add(spacer);
            StackPanel.SetProportionType(spacer, ProportionType.Fill);

            LocalizedButton inviteButton = ButtonFactory.CreateSecondaryButton(TextKeys.Room.InvitePlayersButton);
            inviteButton.Visible = _roomFlow.HasAccount && (room.State != RoomState.InMatch);
            inviteButton.Click += InviteButtonOnClick;
            roomActionsBar.Widgets.Add(inviteButton);

            LocalizedButton leaveButton = ButtonFactory.CreateDestructiveButton(TextKeys.Room.LeaveButton);
            leaveButton.Visible = room.State != RoomState.InMatch;
            leaveButton.Click += LeaveButtonOnClick;
            roomActionsBar.Widgets.Add(leaveButton);

            LocalizedButton startMatchButton = ButtonFactory.CreatePrimaryButton(TextKeys.Room.StartMatchButton);
            startMatchButton.Visible = isViewerHost;
            startMatchButton.Enabled = room.CanViewerStartMatch;
            startMatchButton.Click += StartMatchButtonOnClick;
            roomActionsBar.Widgets.Add(startMatchButton);

            return roomActionsBar;
        }

        private static HorizontalStackPanel BuildPlayersHeader(Room room)
        {
            HorizontalStackPanel playersHeader = PanelFactory.CreateButtonRow();
            playersHeader.Widgets.Add(LabelFactory.CreateTitle(TextKeys.Room.PlayersTitle));

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
            HorizontalStackPanel itemRow = PanelFactory.CreateButtonRow();

            HorizontalStackPanel nameLine = PanelFactory.CreateButtonRow();
            nameLine.Widgets.Add(PanelFactory.CreateSeatSwatch(member.Color));
            string? avatarReference = member.MemberId == room.ViewerMemberId ? _roomFlow.ViewerAvatarReference : member.AvatarReference;
            nameLine.Widgets.Add(_avatarViews.Create(avatarReference).Root);
            nameLine.Widgets.Add(LabelFactory.CreateUsernameLabel(member.DisplayName));
            if (member.MemberId == room.ViewerMemberId)
            {
                nameLine.Widgets.Add(CreateMutedLabel(TextKeys.Common.YouTag, null));
            }

            if (member.MemberId == room.HostMemberId)
            {
                nameLine.Widgets.Add(PanelFactory.CreateStatusBadge(TextKeys.Room.HostBadge));
            }

            if (member.IsGuest)
            {
                nameLine.Widgets.Add(PanelFactory.CreateStatusBadge(TextKeys.Room.GuestBadge));
            }

            itemRow.Widgets.Add(nameLine);
            StackPanel.SetProportionType(nameLine, ProportionType.Fill);
            if (!member.IsActive)
            {
                itemRow.Widgets.Add(PanelFactory.CreateStatusBadge(TextKeys.Room.DisconnectedStatus));
            }

            if (canBeKicked)
            {
                LocalizedButton kickButton = ButtonFactory.CreateDestructiveButton(TextKeys.Room.RemovePlayerToolTip);
                kickButton.Click += (clickedButton, clickArguments) => AskKickConfirmation(member);
                itemRow.Widgets.Add(kickButton);
            }

            Panel playerRow = PanelFactory.CreateItemCard(itemRow);

            return playerRow;
        }

        private VerticalStackPanel BuildRoomRankingSection()
        {
            var rankingSection = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
            rankingSection.Widgets.Add(LabelFactory.CreateTitle(TextKeys.Room.RoomRankingTitle));
            rankingSection.Widgets.Add(_rankingPlaceholder);

            return rankingSection;
        }

        private void ShowRanking(Room room)
        {
            _rankingPlaceholder.Widgets.Clear();
            if (room.Ranking.Count == 0)
            {
                _rankingPlaceholder.Widgets.Add(CreateEmptyRankingState());
            }
            else
            {
                int? stretchedTableWidth = null;
                var rankingTable = new RankingTableView(stretchedTableWidth, Sizes.RoomRankingRankColumnWidth, _rankingStatColumns, ToRankingRows(room));
                _rankingPlaceholder.Widgets.Add(rankingTable.Root);
            }
        }

        private static VerticalStackPanel CreateEmptyRankingState()
        {
            VerticalStackPanel emptyRankingState = PanelFactory.CreateEmptyState(TextKeys.Room.EmptyRankingTitle, TextKeys.Room.EmptyRankingHint);
            emptyRankingState.Width = null;
            emptyRankingState.HorizontalAlignment = HorizontalAlignment.Stretch;
            emptyRankingState.Padding = Metrics.EmptyRoomRankingPadding;
            emptyRankingState.Background = Theme.SurfaceSunkenBrush;
            emptyRankingState.Border = Theme.StrongLineBrush;
            emptyRankingState.BorderThickness = Sizes.Border;

            return emptyRankingState;
        }

        private static List<RankingRow> ToRankingRows(Room room)
        {
            var rankingRows = new List<RankingRow>();
            for (var i = 0; i < room.Ranking.Count; i++)
            {
                RoomRankingEntry entry = room.Ranking[i];
                bool isViewer = entry.MemberId == room.ViewerMemberId;
                rankingRows.Add(new RankingRow(i + 1, entry.DisplayName, isViewer, entry.Color, new int[] { entry.Points, entry.Wins }));
            }

            return rankingRows;
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

            LocalizedButton kickConfirmButton = ButtonFactory.CreateDestructiveButton(TextKeys.Alerts.RemoveConfirmButton);
            kickConfirmButton.Click += (clickedButton, clickArguments) => StartKick(member.MemberId);
            LocalizedButton cancelButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.CancelButton);
            cancelButton.Click += CancelButtonOnClick;
            AddConfirmationButtons(kickConfirmButton, cancelButton);
        }

        private void AskLeaveConfirmation()
        {
            _noticeBox.Hide();
            ShowConfirmation(TextKeys.Alerts.LeaveRoomTitle, TextKeys.Alerts.LeaveRoomBody);

            LocalizedButton leaveConfirmButton = ButtonFactory.CreateDestructiveButton(TextKeys.Alerts.LeaveConfirmButton);
            leaveConfirmButton.Click += LeaveConfirmButtonOnClick;
            LocalizedButton stayButton = ButtonFactory.CreateSecondaryButton(TextKeys.Alerts.StayButton);
            stayButton.Click += StayButtonOnClick;
            AddConfirmationButtons(leaveConfirmButton, stayButton);
        }

        private void StartLeave()
        {
            HideConfirmation();
            _roomFlow.Leave();
        }

        private void StartKick(int targetMemberId)
        {
            HideConfirmation();
            _roomFlow.Kick(targetMemberId);
        }

        private void ShowOutcome(RoomOutcome outcome)
        {
            if (outcome.Notice is Notice notice)
            {
                DisplayNotice(_noticeBox, notice);
            }

            if (outcome.BannedPlayer is StartMatchResult bannedResult)
            {
                ShowBannedPlayer(bannedResult);
            }

            if (outcome.NextScreen is ScreenId nextScreen)
            {
                RequestedScreen = nextScreen;
            }
        }

        private void ShowBannedPlayer(StartMatchResult bannedResult)
        {
            if (bannedResult.IsBanPermanent)
            {
                _noticeBox.ShowMessage(TextKeys.Alerts.SuspensionTitle, TextKeys.Room.PlayerPermanentlyBannedNotice, new string[] { bannedResult.BannedDisplayName });
            }
            else
            {
                _noticeBox.ShowMessage(TextKeys.Alerts.SuspensionTitle, TextKeys.Room.PlayerBannedNotice, new string[] { bannedResult.BannedDisplayName, DisplayFormats.FormatRemainingTime(bannedResult.BanTimeLeft) });
            }
        }

        private void StartMatchButtonOnClick(object sender, MyraEventArgs e)
        {
            _noticeBox.Hide();
            _roomFlow.StartMatch();
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
            if (_roomFlow.NeedsLeaveConfirmation)
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
