using System;
using System.Collections.Generic;
using System.Linq;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class InvitePlayersScreen : Screen
    {
        private readonly List<InviteCandidate> _friends = CreateSampleFriends();
        private readonly VerticalStackPanel _candidatesPlaceholder = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
        private readonly LabeledTextBox _searchField = new LabeledTextBox(TextKeys.Friends.SearchPlaceholder, false);

        private string _searchTerm = string.Empty;

        internal InvitePlayersScreen()
            : base(TextKeys.InvitePlayers.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = CreatePage();
            page.Spacing = Metrics.FieldSpacing;
            page.Widgets.Add(BuildSearchRow());
            page.Widgets.Add(CreateTitle(TextKeys.InvitePlayers.MyFriendsTab));
            page.Widgets.Add(_candidatesPlaceholder);
            StackPanel.SetProportionType(_candidatesPlaceholder, ProportionType.Fill);
            RenderCandidates();

            LocalizedButton inviteByEmailButton = CreateSecondaryButton(TextKeys.InvitePlayers.InviteByEmailButton);
            page.Widgets.Add(inviteByEmailButton);

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackToRoomButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(CreateBackBar(backButton));

            return page;
        }

        private HorizontalStackPanel BuildSearchRow()
        {
            HorizontalStackPanel searchRow = CreateButtonRow();
            _searchField.Box.TextChanged += SearchBoxOnTextChanged;
            searchRow.Widgets.Add(_searchField);
            StackPanel.SetProportionType(_searchField, ProportionType.Fill);

            return searchRow;
        }

        private void RenderCandidates()
        {
            _candidatesPlaceholder.Widgets.Clear();
            List<InviteCandidate> matchingCandidates = FindMatchingCandidates();
            if (matchingCandidates.Count == 0)
            {
                _candidatesPlaceholder.Widgets.Add(BuildNoCandidatesState());
                return;
            }

            foreach (InviteCandidate candidate in matchingCandidates)
            {
                HorizontalStackPanel itemRow = CreateButtonRow();
                Label usernameLabel = CreateUsernameLabel(candidate.Username);
                itemRow.Widgets.Add(usernameLabel);
                StackPanel.SetProportionType(usernameLabel, ProportionType.Fill);

                Widget invitationControl = candidate.IsInvited ? CreateStatusBadge(TextKeys.InvitePlayers.InvitedBadge) : BuildInviteButton();
                itemRow.Widgets.Add(invitationControl);

                _candidatesPlaceholder.Widgets.Add(CreateItemCard(itemRow));
            }
        }

        private List<InviteCandidate> FindMatchingCandidates()
        {
            List<InviteCandidate> matchingCandidates = string.IsNullOrWhiteSpace(_searchTerm)
                ? _friends
                : _friends.Where(candidate => candidate.Username.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

            return matchingCandidates;
        }

        private VerticalStackPanel BuildNoCandidatesState()
        {
            VerticalStackPanel noCandidatesState = _friends.Count == 0
                ? CreateEmptyState(TextKeys.Friends.EmptyStateTitle, TextKeys.Friends.EmptyStateHint)
                : CreateEmptyState(TextKeys.Friends.NoResultsTitle, TextKeys.Friends.NoResultsHint);

            return noCandidatesState;
        }

        private static LocalizedButton BuildInviteButton()
        {
            LocalizedButton inviteButton = CreatePrimaryButton(TextKeys.InvitePlayers.InviteButton);

            return inviteButton;
        }

        private void SearchBoxOnTextChanged(object sender, ValueChangedEventArgs<string> e)
        {
            _searchTerm = _searchField.Value;
            RenderCandidates();
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Room;
        }

        private static List<InviteCandidate> CreateSampleFriends()
        {
            var sampleFriends = new List<InviteCandidate>
            {
                new InviteCandidate(PreviewSamples.LuisPlayer, isInvited: false),
                new InviteCandidate(PreviewSamples.SofiaPlayer, isInvited: true),
                new InviteCandidate(PreviewSamples.DiegoPlayer, isInvited: false),
            };

            return sampleFriends;
        }

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
