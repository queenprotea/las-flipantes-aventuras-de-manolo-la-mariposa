using FontStashSharp.RichText;

using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class RoomScreen : Screen
    {
        private const int HeaderSpacing = 12;
        private const int PlayersHeaderSpacing = 9;
        private const int BadgeSpacing = 7;
        private const int YouTagSpacing = 4;
        private const int SideColumnSpacing = 12;
        private const int EmptyRankingSpacing = 4;
        private const int PlayerColorSize = 20;
        private const string Separator = "·";
        private const string RemovePlayerSymbol = "×";

        private const string PreviewRoomCode = "K7QM";

        private static readonly Thickness _roomCodePadding = new Thickness(16, 7);
        private static readonly Thickness _emptyRankingPadding = new Thickness(14, 20);

        private static readonly RoomPlayer[] _previewPlayers =
        {
            new RoomPlayer("jesus", true, true, false, true, Theme.MintBrush, Theme.MintAltBrush),
            new RoomPlayer("valentin", false, false, false, true, Theme.LavenderBrush, Theme.ButterTintBrush),
            new RoomPlayer("salma", false, false, true, true, Theme.ButterBrush, Theme.LavenderTintBrush),
            new RoomPlayer("scarleth", false, false, false, false, Theme.SkyBrush, Theme.SkyTintBrush),
        };

        internal RoomScreen()
            : base(TextKeys.Room.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = Columns();
            VerticalStackPanel players = BuildPlayersColumn();
            columns.Widgets.Add(players);
            columns.Widgets.Add(BuildSideColumn());
            StackPanel.SetProportionType(players, ProportionType.Fill);

            VerticalStackPanel page = Page();
            page.Spacing = Metrics.ColumnSpacing;
            page.Widgets.Add(BuildHeader());
            page.Widgets.Add(columns);
            StackPanel.SetProportionType(columns, ProportionType.Fill);

            return page;
        }

        private HorizontalStackPanel BuildHeader()
        {
            var roomCode = new Label
            {
                Text = PreviewRoomCode,
                Font = Fonts.RoomCode,
                TextColor = Theme.Ink,
                Padding = _roomCodePadding,
                Background = Theme.SurfaceAltBrush,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };

            LocalizedButton copyButton = SmallSecondaryButton(TextKeys.Room.CopyButton);
            copyButton.VerticalAlignment = VerticalAlignment.Center;

            var publicChip = new LocalizedLabel(TextKeys.Room.PublicBadge)
            {
                TextColor = Theme.MintInk,
            };

            var spacer = new Panel();
            var header = new HorizontalStackPanel
            {
                Spacing = HeaderSpacing,
            };
            header.Widgets.Add(roomCode);
            header.Widgets.Add(copyButton);
            header.Widgets.Add(Chip(publicChip, Theme.MintTintBrush, Theme.MintBrush));
            header.Widgets.Add(spacer);
            header.Widgets.Add(BuildHeaderActions());
            StackPanel.SetProportionType(spacer, ProportionType.Fill);

            return header;
        }

        private HorizontalStackPanel BuildHeaderActions()
        {
            LocalizedButton inviteButton = LavenderButton(TextKeys.Room.InvitePlayersButton);
            LocalizedButton leaveButton = SecondaryButton(TextKeys.Room.LeaveButton);
            LocalizedButton startButton = PrimaryButton(TextKeys.Room.StartMatchButton);
            leaveButton.Click += LeaveButtonOnClick;
            startButton.Click += StartButtonOnClick;

            HorizontalStackPanel actions = Row();
            actions.VerticalAlignment = VerticalAlignment.Center;
            actions.Widgets.Add(inviteButton);
            actions.Widgets.Add(leaveButton);
            actions.Widgets.Add(startButton);

            return actions;
        }

        private static VerticalStackPanel BuildPlayersColumn()
        {
            var title = new HorizontalStackPanel
            {
                Spacing = PlayersHeaderSpacing,
            };
            title.Widgets.Add(Title(TextKeys.Room.PlayersTitle));
            title.Widgets.Add(new LocalizedLabel(TextKeys.Room.OccupancyLabel)
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Bottom,
                TextArguments = new object[] { _previewPlayers.Length },
            });

            int connectedCount = CountConnectedPlayers();
            if (connectedCount < _previewPlayers.Length)
            {
                title.Widgets.Add(new Label
                {
                    Text = Separator,
                    Font = Fonts.Small,
                    TextColor = Theme.MutedInk,
                    VerticalAlignment = VerticalAlignment.Bottom,
                });
                title.Widgets.Add(new LocalizedLabel(TextKeys.Room.ActiveCountLabel)
                {
                    Font = Fonts.Small,
                    TextColor = Theme.MutedInk,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    TextArguments = new object[] { connectedCount },
                });
            }

            var list = new VerticalStackPanel
            {
                Spacing = Metrics.ItemListSpacing,
            };
            foreach (RoomPlayer player in _previewPlayers)
            {
                list.Widgets.Add(BuildPlayerRow(player));
            }

            var column = new VerticalStackPanel
            {
                Spacing = PlayersHeaderSpacing,
                VerticalAlignment = VerticalAlignment.Top,
            };
            column.Widgets.Add(title);
            column.Widgets.Add(list);

            return column;
        }

        private static int CountConnectedPlayers()
        {
            int connectedCount = 0;
            foreach (RoomPlayer player in _previewPlayers)
            {
                if (player.IsConnected)
                {
                    connectedCount++;
                }
            }

            return connectedCount;
        }

        private static HorizontalStackPanel BuildPlayerRow(RoomPlayer player)
        {
            var color = new Panel
            {
                Width = PlayerColorSize,
                Height = PlayerColorSize,
                Background = player.ColorBrush,
                VerticalAlignment = VerticalAlignment.Center,
            };

            Panel avatar = SmallAvatar(player.AvatarBrush);
            var spacer = new Panel();
            HorizontalStackPanel row = ListItem();
            row.Widgets.Add(color);
            row.Widgets.Add(avatar);
            row.Widgets.Add(BuildPlayerName(player));
            row.Widgets.Add(spacer);
            row.Widgets.Add(BuildPlayerBadges(player));
            StackPanel.SetProportionType(spacer, ProportionType.Fill);

            return row;
        }

        private static HorizontalStackPanel BuildPlayerName(RoomPlayer player)
        {
            var name = new HorizontalStackPanel
            {
                Spacing = YouTagSpacing,
                VerticalAlignment = VerticalAlignment.Center,
            };
            name.Widgets.Add(new Label
            {
                Text = player.Name,
                Font = Fonts.Control,
                TextColor = Theme.Ink,
            });

            if (player.IsYou)
            {
                name.Widgets.Add(new Label
                {
                    Text = Separator,
                    Font = Fonts.Small,
                    TextColor = Theme.MutedInk,
                });
                name.Widgets.Add(new LocalizedLabel(TextKeys.Common.YouTag)
                {
                    Font = Fonts.Small,
                    TextColor = Theme.MutedInk,
                });
            }

            return name;
        }

        private static HorizontalStackPanel BuildPlayerBadges(RoomPlayer player)
        {
            var badges = new HorizontalStackPanel
            {
                Spacing = BadgeSpacing,
                VerticalAlignment = VerticalAlignment.Center,
            };

            if (!player.IsConnected)
            {
                var disconnectedChip = new LocalizedLabel(TextKeys.Room.DisconnectedStatus)
                {
                    TextColor = Theme.BlushInk,
                };
                badges.Widgets.Add(Chip(disconnectedChip, Theme.BlushTintBrush, Theme.BlushBrush));
            }

            if (player.IsGuest)
            {
                var guestChip = new LocalizedLabel(TextKeys.Room.GuestBadge)
                {
                    TextColor = Theme.LavenderInk,
                };
                badges.Widgets.Add(Chip(guestChip, Theme.LavenderTintBrush, Theme.LavenderBrush));
            }

            if (player.IsHost)
            {
                var hostChip = new LocalizedLabel(TextKeys.Room.HostBadge)
                {
                    TextColor = Theme.ButterInk,
                };
                badges.Widgets.Add(Chip(hostChip, Theme.ButterTintBrush, Theme.ButterBrush));
            }
            else
            {
                badges.Widgets.Add(BuildRemovePlayerButton());
            }

            return badges;
        }

        private static Button BuildRemovePlayerButton()
        {
            var removeButton = new Button
            {
                Content = new Label
                {
                    Text = RemovePlayerSymbol,
                    Font = Fonts.Small,
                    TextColor = Theme.BlushInk,
                },
                Padding = Metrics.SmallButtonPadding,
                Background = Theme.BlushTintBrush,
                OverBackground = Theme.BlushTintBrush,
                PressedBackground = Theme.BlushTintBrush,
                Border = Theme.BlushBrush,
                BorderThickness = Sizes.Border,
            };

            return removeButton;
        }

        private static VerticalStackPanel BuildSideColumn()
        {
            var ranking = new VerticalStackPanel
            {
                Spacing = PlayersHeaderSpacing,
            };
            ranking.Widgets.Add(Title(TextKeys.Room.RoomRankingTitle));
            ranking.Widgets.Add(BuildEmptyRanking());

            VerticalStackPanel chat = ChatPanel();
            var column = new VerticalStackPanel
            {
                Spacing = SideColumnSpacing,
                Width = Sizes.RoomSideColumnWidth,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            column.Widgets.Add(ranking);
            column.Widgets.Add(chat);
            StackPanel.SetProportionType(chat, ProportionType.Fill);

            return column;
        }

        private static VerticalStackPanel BuildEmptyRanking()
        {
            var emptyRanking = new VerticalStackPanel
            {
                Spacing = EmptyRankingSpacing,
                Padding = _emptyRankingPadding,
                Background = Theme.SurfaceSunkenBrush,
                Border = Theme.StrongLineBrush,
                BorderThickness = Sizes.Border,
            };
            emptyRanking.Widgets.Add(new LocalizedLabel(TextKeys.Room.EmptyRankingTitle)
            {
                Font = Fonts.Body,
                TextColor = Theme.SoftInk,
                HorizontalAlignment = HorizontalAlignment.Center,
            });

            LocalizedLabel hint = Hint(TextKeys.Room.EmptyRankingHint);
            hint.TextAlign = TextHorizontalAlignment.Center;
            emptyRanking.Widgets.Add(hint);

            return emptyRanking;
        }

        private void LeaveButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Rooms;
        }

        private void StartButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MatchPreparation;
        }

        private sealed record RoomPlayer(string Name, bool IsYou, bool IsHost, bool IsGuest, bool IsConnected, IBrush ColorBrush, IBrush AvatarBrush);
    }
}
