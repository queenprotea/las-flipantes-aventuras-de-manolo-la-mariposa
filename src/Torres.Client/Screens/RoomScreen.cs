using System.Collections.Generic;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class RoomScreen : Screen
    {
        private readonly bool _isPublic = true;
        private readonly List<RoomPlayer> _players = CreateSamplePlayers();

        internal RoomScreen()
            : base(TextKeys.Room.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = CreatePage();
            page.Spacing = Metrics.FieldSpacing;

            page.Widgets.Add(BuildTopBar());
            LocalizedLabel hostHint = CreateHint(TextKeys.Room.HostOnlyHint);
            hostHint.HorizontalAlignment = HorizontalAlignment.Right;
            page.Widgets.Add(hostHint);
            page.Widgets.Add(BuildPlayersHeader());
            page.Widgets.Add(BuildPlayersList());
            page.Widgets.Add(BuildRoomRankingSection());

            return page;
        }

        private HorizontalStackPanel BuildTopBar()
        {
            HorizontalStackPanel roomActionsBar = CreateButtonRow();
            roomActionsBar.HorizontalAlignment = HorizontalAlignment.Stretch;

            var roomCodeLabel = new Label
            {
                Text = PreviewSamples.LobbyRoomCode,
                Font = Fonts.Title,
                TextColor = Theme.MintInk,
                VerticalAlignment = VerticalAlignment.Center,
            };
            roomActionsBar.Widgets.Add(roomCodeLabel);

            LocalizedButton copyButton = CreateSmallSecondaryButton(TextKeys.Room.CopyButton);
            copyButton.VerticalAlignment = VerticalAlignment.Center;
            roomActionsBar.Widgets.Add(copyButton);

            string visibilityBadgeKey = _isPublic ? TextKeys.Room.PublicBadge : TextKeys.Room.PrivateBadge;
            roomActionsBar.Widgets.Add(CreateStatusBadge(visibilityBadgeKey));
            roomActionsBar.VerticalAlignment = VerticalAlignment.Top;

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
            roomActionsBar.Widgets.Add(startButton);

            return roomActionsBar;
        }

        private HorizontalStackPanel BuildPlayersHeader()
        {
            HorizontalStackPanel playersHeader = CreateButtonRow();
            playersHeader.Widgets.Add(CreateTitle(TextKeys.Room.PlayersTitle));

            string occupancyText = LocalizedText.Format(TextKeys.Room.OccupancyLabel, _players.Count);
            playersHeader.Widgets.Add(CreateMutedLabel(occupancyText));

            int activePlayerCount = CountActivePlayers();
            if (activePlayerCount != _players.Count)
            {
                string activeText = LocalizedText.Format(TextKeys.Room.ActiveCountLabel, activePlayerCount);
                playersHeader.Widgets.Add(CreateMutedLabel(activeText));
            }

            return playersHeader;
        }

        private int CountActivePlayers()
        {
            int activePlayerCount = 0;
            foreach (RoomPlayer player in _players)
            {
                if (!player.IsDisconnected)
                {
                    activePlayerCount++;
                }
            }

            return activePlayerCount;
        }

        private Widget BuildPlayersList()
        {
            var playersList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (RoomPlayer player in _players)
            {
                playersList.Widgets.Add(BuildPlayerRow(player));
            }

            return playersList;
        }

        private Panel BuildPlayerRow(RoomPlayer player)
        {
            HorizontalStackPanel itemRow = CreateButtonRow();

            HorizontalStackPanel nameLine = CreateButtonRow();
            nameLine.Widgets.Add(CreateUsernameLabel(player.Username));
            if (player.IsHost)
            {
                nameLine.Widgets.Add(CreateStatusBadge(TextKeys.Room.HostBadge));
            }
            if (player.IsGuest)
            {
                nameLine.Widgets.Add(CreateStatusBadge(TextKeys.Room.GuestBadge));
            }
            itemRow.Widgets.Add(nameLine);
            StackPanel.SetProportionType(nameLine, ProportionType.Fill);

            if (player.IsDisconnected)
            {
                itemRow.Widgets.Add(CreateStatusBadge(TextKeys.Room.DisconnectedStatus));
            }

            if (!player.IsHost)
            {
                LocalizedButton removeButton = CreateDestructiveButton(TextKeys.Room.RemovePlayerToolTip);
                itemRow.Widgets.Add(removeButton);
            }

            Panel playerRow = CreateItemCard(itemRow);

            return playerRow;
        }

        private Widget BuildRoomRankingSection()
        {
            var rankingSection = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
            rankingSection.Widgets.Add(CreateTitle(TextKeys.Room.RoomRankingTitle));
            rankingSection.Widgets.Add(CreateEmptyState(TextKeys.Room.EmptyRankingTitle, TextKeys.Room.EmptyRankingHint));

            return rankingSection;
        }

        private static Label CreateMutedLabel(string labelText)
        {
            var mutedLabel = new Label
            {
                Text = labelText,
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return mutedLabel;
        }

        private void InviteButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.InvitePlayers;
        }

        private void LeaveButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Rooms;
        }

        private static List<RoomPlayer> CreateSamplePlayers()
        {
            var samplePlayers = new List<RoomPlayer>
            {
                new RoomPlayer(PreviewSamples.AnaPlayer, isHost: true, isGuest: false, isDisconnected: false),
                new RoomPlayer(PreviewSamples.LuisPlayer, isHost: false, isGuest: false, isDisconnected: false),
                new RoomPlayer(PreviewSamples.SofiaPlayer, isHost: false, isGuest: true, isDisconnected: true),
            };

            return samplePlayers;
        }

        private sealed class RoomPlayer
        {
            internal RoomPlayer(string username, bool isHost, bool isGuest, bool isDisconnected)
            {
                Username = username;
                IsHost = isHost;
                IsGuest = isGuest;
                IsDisconnected = isDisconnected;
            }

            internal string Username { get; }

            internal bool IsHost { get; }

            internal bool IsGuest { get; }

            internal bool IsDisconnected { get; }
        }
    }
}
