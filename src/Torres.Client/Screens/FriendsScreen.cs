using System.Collections.Generic;

using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class FriendsScreen : Screen
    {
        private const int TabSpacing = 3;
        private const int TabUnderlineHeight = 2;
        private const int TabsBottomSpacing = 16;
        private const int RequestsTitleTopSpacing = 14;
        private const int ActionSpacing = 7;

        private const string PreviewFriendName = "valentin";
        private const string PreviewRequesterName = "scarleth";
        private const string PreviewInviteeName = "salma";

        private static readonly Thickness _tabPadding = new Thickness(13, 7);

        private readonly Dictionary<FriendsSection, LocalizedLabel> _tabLabelsBySection = new Dictionary<FriendsSection, LocalizedLabel>();
        private readonly Dictionary<FriendsSection, Panel> _tabUnderlinesBySection = new Dictionary<FriendsSection, Panel>();
        private readonly Dictionary<FriendsSection, Widget> _pagesBySection = new Dictionary<FriendsSection, Widget>();

        internal FriendsScreen()
            : base(TextKeys.Friends.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            _pagesBySection.Add(FriendsSection.Friends, BuildFriendsPage());
            _pagesBySection.Add(FriendsSection.Received, BuildReceivedPage());
            _pagesBySection.Add(FriendsSection.Sent, BuildSentPage());
            _pagesBySection.Add(FriendsSection.Search, BuildSearchPage());

            var pages = new Panel();
            foreach (Widget sectionPage in _pagesBySection.Values)
            {
                pages.Widgets.Add(sectionPage);
            }

            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;

            VerticalStackPanel page = Page();
            page.Widgets.Add(BuildTabs());
            page.Widgets.Add(pages);
            page.Widgets.Add(BackBar(backButton));
            StackPanel.SetProportionType(pages, ProportionType.Fill);
            SelectSection(FriendsSection.Friends);

            return page;
        }

        private VerticalStackPanel BuildTabs()
        {
            Button friendsTab = BuildTab(FriendsSection.Friends, TextKeys.Friends.FriendsTab);
            Button receivedTab = BuildTab(FriendsSection.Received, TextKeys.Friends.ReceivedTab);
            Button sentTab = BuildTab(FriendsSection.Sent, TextKeys.Friends.SentTab);
            Button searchTab = BuildTab(FriendsSection.Search, TextKeys.Friends.SearchTab);
            friendsTab.Click += FriendsTabOnClick;
            receivedTab.Click += ReceivedTabOnClick;
            sentTab.Click += SentTabOnClick;
            searchTab.Click += SearchTabOnClick;

            var tabRow = new HorizontalStackPanel
            {
                Spacing = TabSpacing,
            };
            tabRow.Widgets.Add(friendsTab);
            tabRow.Widgets.Add(receivedTab);
            tabRow.Widgets.Add(sentTab);
            tabRow.Widgets.Add(searchTab);

            var tabs = new VerticalStackPanel
            {
                Margin = new Thickness(0, 0, 0, TabsBottomSpacing),
            };
            tabs.Widgets.Add(tabRow);
            tabs.Widgets.Add(Divider());

            return tabs;
        }

        private Button BuildTab(FriendsSection section, string textKey)
        {
            var label = new LocalizedLabel(textKey)
            {
                Font = Fonts.Control,
                Padding = _tabPadding,
            };
            var underline = new Panel
            {
                Height = TabUnderlineHeight,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            _tabLabelsBySection.Add(section, label);
            _tabUnderlinesBySection.Add(section, underline);

            var content = new VerticalStackPanel();
            content.Widgets.Add(label);
            content.Widgets.Add(underline);

            var tab = new Button
            {
                Content = content,
                Background = null,
                OverBackground = null,
                PressedBackground = null,
                Border = null,
                BorderThickness = new Thickness(0),
            };

            return tab;
        }

        private void SelectSection(FriendsSection selectedSection)
        {
            foreach (KeyValuePair<FriendsSection, Widget> sectionPage in _pagesBySection)
            {
                bool isSelected = sectionPage.Key == selectedSection;
                sectionPage.Value.Visible = isSelected;
                _tabLabelsBySection[sectionPage.Key].TextColor = isSelected ? Theme.Ink : Theme.MutedInk;
                _tabUnderlinesBySection[sectionPage.Key].Background = isSelected ? Theme.MintLineBrush : null;
            }
        }

        private static VerticalStackPanel BuildFriendsPage()
        {
            LocalizedButton removeButton = SmallButton(DestructiveButton(TextKeys.Friends.RemoveFriendButton));
            HorizontalStackPanel friend = BuildPersonRow(PreviewFriendName, Theme.ButterTintBrush, null);
            friend.Widgets.Add(removeButton);

            VerticalStackPanel list = BuildList();
            list.Widgets.Add(friend);

            return list;
        }

        private static VerticalStackPanel BuildReceivedPage()
        {
            HorizontalStackPanel actions = Row();
            actions.Spacing = ActionSpacing;
            actions.VerticalAlignment = VerticalAlignment.Center;
            actions.Widgets.Add(SmallButton(PrimaryButton(TextKeys.Friends.AcceptButton)));
            actions.Widgets.Add(SmallSecondaryButton(TextKeys.Friends.RejectButton));

            HorizontalStackPanel request = BuildPersonRow(PreviewRequesterName, Theme.SkyTintBrush, TextKeys.Friends.FriendRequestMessage);
            request.Widgets.Add(actions);

            var title = new LocalizedLabel(TextKeys.Friends.RequestsTitle)
            {
                Font = Fonts.Body,
                TextColor = Theme.Ink,
                Margin = new Thickness(0, RequestsTitleTopSpacing, 0, 0),
            };

            VerticalStackPanel list = BuildList();
            list.Widgets.Add(title);
            list.Widgets.Add(request);

            return list;
        }

        private static VerticalStackPanel BuildSentPage()
        {
            HorizontalStackPanel sentRequest = BuildPersonRow(PreviewInviteeName, Theme.LavenderTintBrush, TextKeys.Friends.AwaitingResponseMessage);
            sentRequest.Widgets.Add(NeutralChip(TextKeys.Friends.PendingStatus));

            VerticalStackPanel list = BuildList();
            list.Widgets.Add(sentRequest);

            return list;
        }

        private static VerticalStackPanel BuildSearchPage()
        {
            var searchField = new LabeledTextBox(TextKeys.Friends.SearchPlaceholder, false);
            LocalizedButton searchButton = SecondaryButton(TextKeys.Friends.SearchButton);
            searchButton.VerticalAlignment = VerticalAlignment.Bottom;

            HorizontalStackPanel searchRow = Row();
            searchRow.Widgets.Add(searchField);
            searchRow.Widgets.Add(searchButton);
            StackPanel.SetProportionType(searchField, ProportionType.Fill);

            var alreadyFriend = new LocalizedLabel(TextKeys.Friends.AlreadyFriendsStatus)
            {
                TextColor = Theme.MintInk,
            };
            HorizontalStackPanel friendResult = BuildPersonRow(PreviewFriendName, Theme.ButterTintBrush, null);
            friendResult.Widgets.Add(Chip(alreadyFriend, Theme.MintTintBrush, Theme.MintBrush));

            HorizontalStackPanel sentResult = BuildPersonRow(PreviewInviteeName, Theme.LavenderTintBrush, null);
            sentResult.Widgets.Add(NeutralChip(TextKeys.Friends.RequestSentStatus));

            HorizontalStackPanel receivedResult = BuildPersonRow(PreviewRequesterName, Theme.SkyTintBrush, null);
            receivedResult.Widgets.Add(NeutralChip(TextKeys.Friends.RequestReceivedStatus));

            LocalizedLabel selfExcluded = Hint(TextKeys.Friends.SelfExcludedHint);

            VerticalStackPanel list = BuildList();
            list.Widgets.Add(searchRow);
            list.Widgets.Add(friendResult);
            list.Widgets.Add(sentResult);
            list.Widgets.Add(receivedResult);
            list.Widgets.Add(selfExcluded);

            return list;
        }

        private static VerticalStackPanel BuildList()
        {
            return new VerticalStackPanel
            {
                Spacing = Metrics.ItemListSpacing,
                Width = Sizes.FriendsListWidth,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
        }

        private static HorizontalStackPanel BuildPersonRow(string playerName, IBrush avatarBrush, string? messageKey)
        {
            var identity = new VerticalStackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
            };
            identity.Widgets.Add(PlayerNameLabel(playerName, Theme.Ink));

            if (messageKey is not null)
            {
                identity.Widgets.Add(new LocalizedLabel(messageKey)
                {
                    Font = Fonts.Small,
                    TextColor = Theme.MutedInk,
                });
            }

            HorizontalStackPanel row = ListItem();
            row.Widgets.Add(SmallAvatar(avatarBrush));
            row.Widgets.Add(identity);
            StackPanel.SetProportionType(identity, ProportionType.Fill);

            return row;
        }

        private static Panel NeutralChip(string textKey)
        {
            var label = new LocalizedLabel(textKey)
            {
                TextColor = Theme.SoftInk,
            };

            return Chip(label, Theme.SurfaceAltBrush, Theme.LineBrush);
        }

        private static LocalizedButton SmallButton(LocalizedButton button)
        {
            button.LabelFont = Fonts.Small;
            button.Padding = Metrics.SmallButtonPadding;
            button.VerticalAlignment = VerticalAlignment.Center;

            return button;
        }

        private void FriendsTabOnClick(object sender, MyraEventArgs e)
        {
            SelectSection(FriendsSection.Friends);
        }

        private void ReceivedTabOnClick(object sender, MyraEventArgs e)
        {
            SelectSection(FriendsSection.Received);
        }

        private void SentTabOnClick(object sender, MyraEventArgs e)
        {
            SelectSection(FriendsSection.Sent);
        }

        private void SearchTabOnClick(object sender, MyraEventArgs e)
        {
            SelectSection(FriendsSection.Search);
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MainMenu;
        }

        private enum FriendsSection
        {
            Friends,
            Received,
            Sent,
            Search,
        }
    }
}
