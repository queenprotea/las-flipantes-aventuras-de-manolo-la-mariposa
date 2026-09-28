using System;
using System.Collections.Generic;
using System.Linq;

using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class FriendsScreen : Screen
    {
        private enum FriendsTabId { Friends, Received, Sent }
        private enum SearchStatus { New, Friend, Sent, Received }

        private readonly List<FriendEntry> _friends = SampleFriends();
        private readonly List<RequestEntry> _received = SampleReceived();
        private readonly List<RequestEntry> _sent = SampleSent();
        private readonly List<SearchResultEntry> _allSearchable = SampleSearchResults();

        private FriendsTabId _activeTab = FriendsTabId.Friends;
        private string _searchTerm = string.Empty;
        private VerticalStackPanel? _contentPlaceholder;
        private LabeledTextBox? _searchField;

        internal FriendsScreen()
            : base(TextKeys.Friends.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = Page();
            page.Spacing = Theme.FieldSpacing;

            page.Widgets.Add(BuildSearchRow());

            var placeholder = new VerticalStackPanel { Spacing = Theme.FieldSpacing };
            _contentPlaceholder = placeholder;
            page.Widgets.Add(placeholder);
            StackPanel.SetProportionType(placeholder, ProportionType.Fill);

            RenderTab();

            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += OnBackClick;
            page.Widgets.Add(BackBar(backButton));

            return page;
        }

        private HorizontalStackPanel BuildSearchRow()
        {
            HorizontalStackPanel row = Row();

            _searchField = new LabeledTextBox(TextKeys.Friends.SearchPlaceholder, false);
            _searchField.Box.TextChanged += (sender, args) =>
            {
                _searchTerm = _searchField.Value;
                RenderTab();
            };

            row.Widgets.Add(_searchField);
            StackPanel.SetProportionType(_searchField, ProportionType.Fill);
            return row;
        }

        private void RenderTab()
        {
            _contentPlaceholder!.Widgets.Clear();
            _contentPlaceholder.Widgets.Add(BuildTabsRow());
            _contentPlaceholder.Widgets.Add(
                string.IsNullOrWhiteSpace(_searchTerm) ? BuildActiveTabContent() : BuildSearchResults());
        }

        private HorizontalStackPanel BuildTabsRow()
        {
            HorizontalStackPanel tabsRow = Row();
            tabsRow.Widgets.Add(BuildTabButton(TextKeys.Friends.FriendsTab, FriendsTabId.Friends));
            tabsRow.Widgets.Add(BuildTabButton(TextKeys.Friends.ReceivedTab, FriendsTabId.Received));
            tabsRow.Widgets.Add(BuildTabButton(TextKeys.Friends.SentTab, FriendsTabId.Sent));
            return tabsRow;
        }

        private LocalizedButton BuildTabButton(string textKey, FriendsTabId tab)
        {
            LocalizedButton button = tab == _activeTab ? PrimaryButton(textKey) : SecondaryButton(textKey);
            button.Click += (sender, arguments) =>
            {
                _activeTab = tab;
                RenderTab();
            };
            return button;
        }

        private Widget BuildActiveTabContent()
        {
            return _activeTab switch
            {
                FriendsTabId.Friends => BuildFriendsList(),
                FriendsTabId.Received => BuildReceivedList(),
                FriendsTabId.Sent => BuildSentList(),
                _ => BuildFriendsList(),
            };
        }

        private VerticalStackPanel BuildFriendsList()
        {
            if (_friends.Count == 0)
            {
                return EmptyState(TextKeys.Friends.EmptyStateTitle, TextKeys.Friends.EmptyStateHint);
            }

            var list = new VerticalStackPanel { Spacing = Theme.ListSpacing };
            foreach (FriendEntry friend in _friends)
            {
                HorizontalStackPanel itemRow = Row();
                itemRow.Widgets.Add(NameLabel(friend.Username));

                var spacer = new Panel { HorizontalAlignment = HorizontalAlignment.Stretch };
                StackPanel.SetProportionType(spacer, ProportionType.Part);
                itemRow.Widgets.Add(spacer);
                
                LocalizedButton inviteToRoomButton = SecondaryButton(TextKeys.Friends.InviteToRoomButton);
                // TODO: connect to InviteToRoom
                itemRow.Widgets.Add(inviteToRoomButton);
                
                LocalizedButton removeButton = DestructiveButton(TextKeys.Friends.RemoveFriendButton);
                // TODO: connect to DeleteFriend when it exists
                itemRow.Widgets.Add(removeButton);

                list.Widgets.Add(BuildItemCard(itemRow));
            }
            return list;
        }

        private Widget BuildReceivedList()
        {
            var container = new VerticalStackPanel { Spacing = Theme.FieldSpacing };
            container.Widgets.Add(Title(TextKeys.Friends.RequestsTitle));

            if (_received.Count == 0)
            {
                container.Widgets.Add(EmptyState(TextKeys.Friends.NoReceivedTitle, TextKeys.Friends.NoReceivedHint));
                return container;
            }

            var list = new VerticalStackPanel { Spacing = Theme.ListSpacing };
            foreach (RequestEntry request in _received)
            {
                HorizontalStackPanel itemRow = Row();
                itemRow.HorizontalAlignment = HorizontalAlignment.Stretch;

                var nameColumn = new VerticalStackPanel();
                nameColumn.Widgets.Add(NameLabel(request.Username));
                nameColumn.Widgets.Add(Hint(TextKeys.Friends.FriendRequestMessage));
                itemRow.Widgets.Add(nameColumn);
                
                var spacer = new Panel { HorizontalAlignment = HorizontalAlignment.Stretch };
                StackPanel.SetProportionType(spacer, ProportionType.Part);
                itemRow.Widgets.Add(spacer);

                LocalizedButton acceptButton = PrimaryButton(TextKeys.Friends.AcceptButton);
                // TODO: connect to AcceptFriendRequest
                itemRow.Widgets.Add(acceptButton);

                LocalizedButton rejectButton = SecondaryButton(TextKeys.Friends.RejectButton);
                // TODO: connect to RejectFriendRequest
                itemRow.Widgets.Add(rejectButton);

                list.Widgets.Add(BuildItemCard(itemRow));
            }
            container.Widgets.Add(list);
            return container;
        }

        private VerticalStackPanel BuildSentList()
        {
            if (_sent.Count == 0)
            {
                return EmptyState(TextKeys.Friends.NoSentTitle, TextKeys.Friends.NoSentHint);
            }

            var list = new VerticalStackPanel { Spacing = Theme.ListSpacing };
            foreach (RequestEntry request in _sent)
            {
                HorizontalStackPanel itemRow = Row();

                var nameColumn = new VerticalStackPanel();
                nameColumn.Widgets.Add(NameLabel(request.Username));
                nameColumn.Widgets.Add(Hint(TextKeys.Friends.AwaitingResponseMessage));
                itemRow.Widgets.Add(nameColumn);

                itemRow.Widgets.Add(BuildChip(TextKeys.Friends.PendingStatus));

                list.Widgets.Add(BuildItemCard(itemRow));
            }
            return list;
        }

        private VerticalStackPanel BuildSearchResults()
        {
            List<SearchResultEntry> matches = _allSearchable
                .Where(entry => entry.Username.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 0)
            {
                return EmptyState(TextKeys.Friends.NoResultsTitle, TextKeys.Friends.NoResultsHint);
            }

            var list = new VerticalStackPanel { Spacing = Theme.ListSpacing };
            foreach (SearchResultEntry result in matches)
            {
                HorizontalStackPanel itemRow = Row();
                itemRow.Widgets.Add(NameLabel(result.Username));
                itemRow.Widgets.Add(BuildSearchStatusWidget(result.Status));
                list.Widgets.Add(BuildItemCard(itemRow));
            }

            var container = new VerticalStackPanel { Spacing = Theme.FieldSpacing };
            container.Widgets.Add(list);
            container.Widgets.Add(Hint(TextKeys.Friends.SelfExcludedHint));
            return container;
        }

        private static Widget BuildSearchStatusWidget(SearchStatus status)
        {
            return status switch
            {
                SearchStatus.New => PrimaryButton(TextKeys.Friends.SendRequestButton), // TODO: connect to SendFriendRequestAsync.
                SearchStatus.Friend => BuildChip(TextKeys.Friends.AlreadyFriendsStatus),
                SearchStatus.Sent => BuildChip(TextKeys.Friends.RequestSentStatus),
                SearchStatus.Received => BuildChip(TextKeys.Friends.RequestReceivedStatus),
                _ => BuildChip(TextKeys.Friends.AlreadyFriendsStatus),
            };
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

        private static Panel BuildChip(string textKey)
        {
            var chip = new Panel
            {
                Padding = Theme.PillButtonPadding,
                Background = Theme.MintTintBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Theme.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };
            chip.Widgets.Add(new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.MintInk,
            });
            return chip;
        }

        private static Label NameLabel(string username)
        {
            return new Label { Text = username, Font = Fonts.Body, TextColor = Theme.Ink, VerticalAlignment = VerticalAlignment.Center };
        }

        private void OnBackClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.MainMenu;
        }

        private static List<FriendEntry> SampleFriends() => new()
        {
            new FriendEntry("luis_m"),
            new FriendEntry("sofia99"),
            new FriendEntry("dgomez"),
        };

        private static List<RequestEntry> SampleReceived() => new()
        {
            new RequestEntry("marco_r"),
            new RequestEntry("pau_v"),
        };

        private static List<RequestEntry> SampleSent() => new()
        {
            new RequestEntry("ines_b"),
            new RequestEntry("tomas"),
        };

        private static List<SearchResultEntry> SampleSearchResults() => new()
        {
            new SearchResultEntry("nuevo_jugador", SearchStatus.New),
            new SearchResultEntry("luis_m", SearchStatus.Friend),
            new SearchResultEntry("ines_b", SearchStatus.Sent),
            new SearchResultEntry("marco_r", SearchStatus.Received),
        };

        private sealed class FriendEntry
        {
            internal FriendEntry(string username) => Username = username;
            internal string Username { get; }
        }

        private sealed class RequestEntry
        {
            internal RequestEntry(string username) => Username = username;
            internal string Username { get; }
        }

        private sealed class SearchResultEntry
        {
            internal SearchResultEntry(string username, SearchStatus status)
            {
                Username = username;
                Status = status;
            }

            internal string Username { get; }
            internal SearchStatus Status { get; }
        }
    }
}