using System.Collections.Generic;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class RoomScreen : Screen
    {
        private readonly string _roomCode = "TR-8492";
        private readonly bool _isPublic = true;
        private readonly List<RoomPlayer> _players = SamplePlayers();

        internal RoomScreen()
            : base(TextKeys.Room.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = Page();
            page.Spacing = Theme.FieldSpacing;

            page.Widgets.Add(BuildTopBar());
            LocalizedLabel hostHint = Hint(TextKeys.Room.HostOnlyHint);
            hostHint.HorizontalAlignment = HorizontalAlignment.Right;
            page.Widgets.Add(hostHint);
            page.Widgets.Add(BuildPlayersHeader());
            page.Widgets.Add(BuildPlayersList());
            page.Widgets.Add(BuildRoomRankingSection());

            return page;
        }

        private HorizontalStackPanel BuildTopBar()
        {
            HorizontalStackPanel bar = Row();
            bar.HorizontalAlignment = HorizontalAlignment.Stretch;

            var codeGroup = new Label
            {
                Text = _roomCode,
                Font = Fonts.Title,
                TextColor = Theme.MintInk,
                VerticalAlignment = VerticalAlignment.Center,
            };
            bar.Widgets.Add(codeGroup);
            
            LocalizedButton copyButton = SmallSecondaryButton(TextKeys.Room.CopyButton);
            // TODO: connect to Clipboard.SetText(_roomCode).
            copyButton.VerticalAlignment = VerticalAlignment.Center;
            bar.Widgets.Add(copyButton);

            bar.Widgets.Add(BuildBadge(_isPublic ? TextKeys.Room.PublicBadge : TextKeys.Room.PrivateBadge));
            bar.VerticalAlignment = VerticalAlignment.Top;

            var spacer = new Panel();
            bar.Widgets.Add(spacer);
            StackPanel.SetProportionType(spacer, ProportionType.Fill);

            LocalizedButton inviteButton = SecondaryButton(TextKeys.Room.InvitePlayersButton);
            inviteButton.Click += OnInviteClick;
            bar.Widgets.Add(inviteButton);

            LocalizedButton leaveButton = DestructiveButton(TextKeys.Room.LeaveButton);
            leaveButton.Click += OnLeaveClick;
            bar.Widgets.Add(leaveButton);

            LocalizedButton startButton = PrimaryButton(TextKeys.Room.StartMatchButton);
            // TODO: connect to StartMatchAsync (solo el anfitrión).
            bar.Widgets.Add(startButton);

            return bar;
        }

        private HorizontalStackPanel BuildPlayersHeader()
        {
            HorizontalStackPanel header = Row();
            header.Widgets.Add(Title(TextKeys.Room.PlayersTitle));

            string occupancyText = LocalizedText.Format(TextKeys.Room.OccupancyLabel, _players.Count);
            header.Widgets.Add(PlainMutedLabel(occupancyText));

            int activeCount = ActivePlayerCount();
            if (activeCount != _players.Count)
            {
                string activeText = LocalizedText.Format(TextKeys.Room.ActiveCountLabel, activeCount);
                header.Widgets.Add(PlainMutedLabel(activeText));
            }

            return header;
        }

        private int ActivePlayerCount()
        {
            int active = 0;
            foreach (RoomPlayer player in _players)
            {
                if (!player.IsDisconnected)
                {
                    active++;
                }
            }
            return active;
        }

        private Widget BuildPlayersList()
        {
            var list = new VerticalStackPanel { Spacing = Theme.ListSpacing };
            foreach (RoomPlayer player in _players)
            {
                list.Widgets.Add(BuildPlayerRow(player));
            }
            return list;
        }

        private Panel BuildPlayerRow(RoomPlayer player)
        {
            HorizontalStackPanel itemRow = Row();

            HorizontalStackPanel nameLine = Row();
            nameLine.Widgets.Add(NameLabel(player.Username));
            if (player.IsHost)
            {
                nameLine.Widgets.Add(BuildBadge(TextKeys.Room.HostBadge));
            }
            if (player.IsGuest)
            {
                nameLine.Widgets.Add(BuildBadge(TextKeys.Room.GuestBadge));
            }
            itemRow.Widgets.Add(nameLine);
            StackPanel.SetProportionType(nameLine, ProportionType.Fill);

            if (player.IsDisconnected)
            {
                itemRow.Widgets.Add(BuildBadge(TextKeys.Room.DisconnectedStatus));
            }

            if (!player.IsHost)
            {
                LocalizedButton removeButton = DestructiveButton(TextKeys.Room.RemovePlayerToolTip);
                // TODO: connect to RemovePlayerAsync (solo el anfitrión puede verlo/usarlo).
                itemRow.Widgets.Add(removeButton);
            }

            return BuildItemCard(itemRow);
        }

        private Widget BuildRoomRankingSection()
        {
            var section = new VerticalStackPanel { Spacing = Theme.FieldSpacing };
            section.Widgets.Add(Title(TextKeys.Room.RoomRankingTitle));
            // TODO: reemplazar por la tabla real cuando haya partidas jugadas en esta sala.
            section.Widgets.Add(EmptyState(TextKeys.Room.EmptyRankingTitle, TextKeys.Room.EmptyRankingHint));
            return section;
        }

        private static Panel BuildItemCard(Widget content)
        {
            var card = new Panel
            {
                Padding = Theme.ListItemPadding,
                Background = Theme.SurfaceAltBrush,
                Border = Theme.LineBrush,
                BorderThickness = Theme.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            card.Widgets.Add(content);
            return card;
        }

        private static Panel BuildBadge(string textKey)
        {
            var badge = new Panel
            {
                Padding = Theme.PillButtonPadding,
                Background = Theme.MintTintBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Theme.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };
            badge.Widgets.Add(new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.MintInk,
            });
            return badge;
        }

        private static Label PlainMutedLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        private static Label NameLabel(string username)
        {
            return new Label
            {
                Text = username,
                Font = Fonts.Body,
                TextColor = Theme.Ink,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }
        
        private void OnInviteClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.InvitePlayers;
        }

        private void OnLeaveClick(object sender, MyraEventArgs arguments)
        {
            // TODO: connect to LeaveRoomAsync.
            RequestedScreen = ScreenId.Rooms;
        }

        private static List<RoomPlayer> SamplePlayers() => new()
        {
            new RoomPlayer("ana_torres", isHost: true, isGuest: false, isDisconnected: false),
            new RoomPlayer("luis_m", isHost: false, isGuest: false, isDisconnected: false),
            new RoomPlayer("sofia99", isHost: false, isGuest: true, isDisconnected: true),
        };

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