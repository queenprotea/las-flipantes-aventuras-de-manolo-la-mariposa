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
    internal sealed class InvitationsScreen : Screen
    {
        private readonly InvitationsFlow _invitationsFlow;
        private readonly VerticalStackPanel _invitationsPlaceholder = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly AvatarViewSet _avatarViews;

        private int? _shownRevision;

        internal InvitationsScreen(InvitationsFlow invitationsFlow, AvatarCache avatarCache)
            : base(TextKeys.Invitations.HeaderLabel, true)
        {
            _invitationsFlow = invitationsFlow;
            _avatarViews = new AvatarViewSet(avatarCache, Sizes.SmallAvatarSize);
        }

        internal override void Open()
        {
            _noticeBox.Hide();
            HideConfirmation();
            RequestedScreen = _invitationsFlow.Open();
            if (RequestedScreen is null)
            {
                RenderInvitations();
            }
        }

        internal override void Update(GameTime gameTime)
        {
            InvitationsOutcome? outcome = _invitationsFlow.TakeOutcome();
            if (outcome is not null)
            {
                ShowOutcome(outcome);
            }

            if (_shownRevision != _invitationsFlow.Revision)
            {
                RenderInvitations();
            }

            _avatarViews.Refresh();
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = PanelFactory.CreatePage();
            page.Spacing = Metrics.FieldSpacing;
            page.Widgets.Add(_noticeBox);
            page.Widgets.Add(ConfirmationPlaceholder);
            page.Widgets.Add(_invitationsPlaceholder);
            StackPanel.SetProportionType(_invitationsPlaceholder, ProportionType.Fill);
            RenderInvitations();

            LocalizedButton backButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(PanelFactory.CreateBackBar(backButton));

            return page;
        }

        private void ShowOutcome(InvitationsOutcome outcome)
        {
            if (outcome.Notice is Notice notice)
            {
                DisplayNotice(_noticeBox, notice);
            }

            if (outcome.AsksRoomFullChoice)
            {
                AskRoomFullChoice();
            }

            if (outcome.NextScreen is ScreenId nextScreen)
            {
                RequestedScreen = nextScreen;
            }
        }

        private void AskRoomFullChoice()
        {
            ShowConfirmation(TextKeys.Alerts.RoomFullTitle, TextKeys.Alerts.RoomFullBody);

            LocalizedButton viewRoomsButton = ButtonFactory.CreatePrimaryButton(TextKeys.Alerts.ViewRoomsButton);
            viewRoomsButton.Click += ViewRoomsButtonOnClick;
            LocalizedButton cancelButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.CancelButton);
            cancelButton.Click += CancelButtonOnClick;
            AddConfirmationButtons(viewRoomsButton, cancelButton);
        }

        private void AcceptInvitation(int roomId)
        {
            if (_invitationsFlow.Accept(roomId))
            {
                PrepareAnswer();
            }
        }

        private void RejectInvitation(int roomId)
        {
            if (_invitationsFlow.Reject(roomId))
            {
                PrepareAnswer();
            }
        }

        private void PrepareAnswer()
        {
            _noticeBox.Hide();
            HideConfirmation();
        }

        private void RenderInvitations()
        {
            _shownRevision = _invitationsFlow.Revision;
            _avatarViews.Clear();
            _invitationsPlaceholder.Widgets.Clear();
            IReadOnlyList<RoomInvitation> invitations = _invitationsFlow.Invitations;
            if (invitations.Count == 0)
            {
                _invitationsPlaceholder.Widgets.Add(PanelFactory.CreateEmptyState(TextKeys.Invitations.EmptyStateTitle, TextKeys.Invitations.EmptyStateHint));
                return;
            }

            foreach (RoomInvitation invitation in invitations)
            {
                _invitationsPlaceholder.Widgets.Add(BuildInvitationCard(invitation));
            }
        }

        private Panel BuildInvitationCard(RoomInvitation invitation)
        {
            HorizontalStackPanel itemRow = PanelFactory.CreateButtonRow();
            itemRow.HorizontalAlignment = HorizontalAlignment.Stretch;
            itemRow.Widgets.Add(_avatarViews.Create(invitation.Sender.AvatarReference).Root);

            var nameColumn = new VerticalStackPanel { VerticalAlignment = VerticalAlignment.Center };
            nameColumn.Widgets.Add(LabelFactory.CreateUsernameLabel(invitation.Sender.Username));
            nameColumn.Widgets.Add(LabelFactory.CreateHint(TextKeys.Invitations.InvitedByMessage));
            nameColumn.Widgets.Add(BuildRoomCodeRow(invitation.RoomCode));
            itemRow.Widgets.Add(nameColumn);

            var spacer = new Panel { HorizontalAlignment = HorizontalAlignment.Stretch };
            StackPanel.SetProportionType(spacer, ProportionType.Part);
            itemRow.Widgets.Add(spacer);

            LocalizedButton joinButton = ButtonFactory.CreatePrimaryButton(TextKeys.Invitations.JoinButton);
            joinButton.Click += (sender, e) => AcceptInvitation(invitation.RoomId);
            itemRow.Widgets.Add(joinButton);

            LocalizedButton rejectButton = ButtonFactory.CreateSecondaryButton(TextKeys.Invitations.RejectToolTip);
            rejectButton.Click += (sender, e) => RejectInvitation(invitation.RoomId);
            itemRow.Widgets.Add(rejectButton);

            Panel invitationCard = PanelFactory.CreateItemCard(itemRow);

            return invitationCard;
        }

        private static HorizontalStackPanel BuildRoomCodeRow(string roomCode)
        {
            var roomLabel = new LocalizedLabel(TextKeys.Invitations.RoomLabel)
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
            };
            var roomCodeLabel = new Label
            {
                Text = roomCode,
                Font = Fonts.Small,
                TextColor = Theme.MintInk,
            };

            var roomCodeRow = new HorizontalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            roomCodeRow.Widgets.Add(roomLabel);
            roomCodeRow.Widgets.Add(roomCodeLabel);

            return roomCodeRow;
        }

        private void ViewRoomsButtonOnClick(object sender, MyraEventArgs e)
        {
            HideConfirmation();
            RequestedScreen = ScreenId.Rooms;
        }

        private void CancelButtonOnClick(object sender, MyraEventArgs e)
        {
            HideConfirmation();
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            HideConfirmation();
            _invitationsFlow.ForgetRequests();
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
