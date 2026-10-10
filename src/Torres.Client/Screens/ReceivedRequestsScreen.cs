using System.Collections.Generic;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class ReceivedRequestsScreen : Screen
    {
        private readonly List<RequestEntry> _receivedInvitations = CreateSampleInvitations();

        internal ReceivedRequestsScreen()
            : base(TextKeys.Friends.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = CreatePage();
            page.Spacing = Metrics.FieldSpacing;
            page.Widgets.Add(CreateTitle(TextKeys.Invitations.HeaderLabel));
            page.Widgets.Add(BuildInvitationsContent());

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(CreateBackBar(backButton));

            return page;
        }

        private Widget BuildInvitationsContent()
        {
            if (_receivedInvitations.Count == 0)
            {
                VerticalStackPanel noInvitationsState = CreateEmptyState(TextKeys.Invitations.EmptyStateTitle, TextKeys.Invitations.EmptyStateHint);
                return noInvitationsState;
            }

            var invitationsList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (RequestEntry invitation in _receivedInvitations)
            {
                HorizontalStackPanel itemRow = CreateButtonRow();

                var nameColumn = new VerticalStackPanel();
                nameColumn.Widgets.Add(CreateUsernameLabel(invitation.Username));
                nameColumn.Widgets.Add(CreateHint(TextKeys.Invitations.InvitedByMessage));
                itemRow.Widgets.Add(nameColumn);

                LocalizedButton acceptButton = CreatePrimaryButton(TextKeys.Invitations.JoinButton);
                itemRow.Widgets.Add(acceptButton);

                LocalizedButton rejectButton = CreateSecondaryButton(TextKeys.Invitations.RejectToolTip);
                itemRow.Widgets.Add(rejectButton);

                invitationsList.Widgets.Add(CreateItemCard(itemRow));
            }

            return invitationsList;
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MainMenu;
        }

        private static List<RequestEntry> CreateSampleInvitations()
        {
            var sampleInvitations = new List<RequestEntry>
            {
                new RequestEntry(PreviewSamples.MarcoPlayer),
                new RequestEntry(PreviewSamples.PauPlayer),
            };

            return sampleInvitations;
        }

        private sealed class RequestEntry
        {
            internal RequestEntry(string username)
            {
                Username = username;
            }

            internal string Username { get; }
        }
    }
}
