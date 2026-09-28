using System.Collections.Generic;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class ReceivedRequestsScreen : Screen
    {
        private readonly List<RequestEntry> _received = SampleReceived();

        internal ReceivedRequestsScreen()
            : base(TextKeys.Friends.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = Page();
            page.Spacing = Metrics.FieldSpacing;

            page.Widgets.Add(Title(TextKeys.Invitations.HeaderLabel));
            page.Widgets.Add(BuildContent());

            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += OnBackClick;
            page.Widgets.Add(BackBar(backButton));

            return page;
        }

        private Widget BuildContent()
        {
            if (_received.Count == 0)
            {
                return EmptyState(TextKeys.Invitations.EmptyStateTitle, TextKeys.Invitations.EmptyStateHint);
            }

            var list = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (RequestEntry request in _received)
            {
                HorizontalStackPanel itemRow = Row();

                var nameColumn = new VerticalStackPanel();
                nameColumn.Widgets.Add(NameLabel(request.Username));
                nameColumn.Widgets.Add(Hint(TextKeys.Invitations.InvitedByMessage));
                itemRow.Widgets.Add(nameColumn);

                LocalizedButton acceptButton = PrimaryButton(TextKeys.Invitations.JoinButton);
                // TODO: connect to AcceptFriendRequest.
                itemRow.Widgets.Add(acceptButton);

                LocalizedButton rejectButton = SecondaryButton(TextKeys.Invitations.RejectToolTip);
                // TODO: connect to RejectFriendRequest.
                itemRow.Widgets.Add(rejectButton);

                list.Widgets.Add(BuildItemCard(itemRow));
            }

            return list;
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

        private void OnBackClick(object sender, MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.MainMenu;
        }

        private static List<RequestEntry> SampleReceived() => new()
        {
            new RequestEntry("marco_r"),
            new RequestEntry("pau_v"),
        };

        private sealed class RequestEntry
        {
            internal RequestEntry(string username) => Username = username;
            internal string Username { get; }
        }
    }
}
