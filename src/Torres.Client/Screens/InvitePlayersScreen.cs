using System;
using System.Collections.Generic;
using System.Linq;

using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class InvitePlayersScreen : Screen
    {
        private readonly List<InviteCandidate> _friends = SampleFriends();

        private string _searchTerm = string.Empty;
        private VerticalStackPanel? _listPlaceholder;
        private LabeledTextBox? _searchField;

        internal InvitePlayersScreen()
            : base(TextKeys.InvitePlayers.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = Page();
            page.Spacing = Metrics.FieldSpacing;

            page.Widgets.Add(BuildSearchRow());
            page.Widgets.Add(Title(TextKeys.InvitePlayers.MyFriendsTab));

            var placeholder = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            _listPlaceholder = placeholder;
            page.Widgets.Add(placeholder);
            StackPanel.SetProportionType(placeholder, ProportionType.Fill);

            Render();

            LocalizedButton emailButton = SecondaryButton(TextKeys.InvitePlayers.InviteByEmailButton);
            // TODO: connect to InviteByEmailAsync.
            page.Widgets.Add(emailButton);

            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackToRoomButton);
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
                _searchTerm = _searchField!.Value;
                Render();
            };

            row.Widgets.Add(_searchField);
            StackPanel.SetProportionType(_searchField, ProportionType.Fill);
            return row;
        }

        private void Render()
        {
            _listPlaceholder!.Widgets.Clear();

            if (_friends.Count == 0)
            {
                _listPlaceholder.Widgets.Add(EmptyState(TextKeys.Friends.EmptyStateTitle, TextKeys.Friends.EmptyStateHint));
                return;
            }

            List<InviteCandidate> matches = string.IsNullOrWhiteSpace(_searchTerm)
                ? _friends
                : _friends.Where(candidate => candidate.Username.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

            if (matches.Count == 0)
            {
                _listPlaceholder.Widgets.Add(EmptyState(TextKeys.Friends.NoResultsTitle, TextKeys.Friends.NoResultsHint));
                return;
            }

            foreach (InviteCandidate candidate in matches)
            {
                HorizontalStackPanel itemRow = Row();

                Label nameLabel = NameLabel(candidate.Username);
                itemRow.Widgets.Add(nameLabel);
                StackPanel.SetProportionType(nameLabel, ProportionType.Fill);

                itemRow.Widgets.Add(candidate.IsInvited ? BuildInvitedBadge() : BuildInviteButton());

                _listPlaceholder.Widgets.Add(BuildItemCard(itemRow));
            }
        }

        private static LocalizedButton BuildInviteButton()
        {
            LocalizedButton inviteButton = PrimaryButton(TextKeys.InvitePlayers.InviteButton);
            // TODO: connect to InvitePlayerAsync.
            return inviteButton;
        }

        private static Panel BuildInvitedBadge()
        {
            var badge = new Panel
            {
                Padding = Metrics.PillButtonPadding,
                Background = Theme.MintTintBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };
            badge.Widgets.Add(new LocalizedLabel(TextKeys.InvitePlayers.InvitedBadge)
            {
                Font = Fonts.Small,
                TextColor = Theme.MintInk,
            });
            return badge;
        }

        private static Panel BuildItemCard(Widget content)
        {
            var card = new Panel
            {
                Padding = Metrics.ListItemPadding,
                Background = Theme.SurfaceAltBrush,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            card.Widgets.Add(content);
            return card;
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

        private void OnBackClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.Room;
        }

        private static List<InviteCandidate> SampleFriends() => new()
        {
            new InviteCandidate("luis_m", isInvited: false),
            new InviteCandidate("sofia99", isInvited: true),
            new InviteCandidate("dgomez", isInvited: false),
        };

        private sealed class InviteCandidate
        {
            internal InviteCandidate(string username, bool isInvited)
            {
                Username = username;
                IsInvited = isInvited;
            }

            internal string Username { get; }
            internal bool IsInvited { get; }
        }
    }
}
