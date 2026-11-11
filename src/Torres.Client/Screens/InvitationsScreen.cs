using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Avatars;
using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class InvitationsScreen : Screen
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly VerticalStackPanel _invitationsPlaceholder = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly AvatarViewSet _avatarViews;

        private Task<PendingInvitationsResponse>? _invitationsRequest;
        private Task<RespondInvitationResult>? _answerRequest;
        private int _answeredRoomId;
        private int? _shownRevision;

        internal InvitationsScreen(GameSessionConnection gameSessionConnection, PlayerSession playerSession, AvatarCache avatarCache)
            : base(TextKeys.Invitations.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(playerSession);
            ArgumentNullException.ThrowIfNull(avatarCache);

            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
            _avatarViews = new AvatarViewSet(avatarCache, Sizes.SmallAvatarSize);
        }

        internal override void Open()
        {
            ForgetRequests();
            _noticeBox.Hide();
            HideConfirmation();
            if (!_playerSession.HasAccount || !_gameSessionConnection.IsSessionOpen)
            {
                RequestedScreen = ScreenId.Login;
            }
            else if (_playerSession.CurrentRoom is not null)
            {
                RequestedScreen = ScreenId.Room;
            }
            else
            {
                RenderInvitations();
                _invitationsRequest = _gameSessionConnection.GetPendingInvitationsAsync();
            }
        }

        internal override void Update(GameTime gameTime)
        {
            FollowInvitationsRequest();
            FollowAnswer();
            if (_shownRevision != _playerSession.Invitations.Revision)
            {
                RenderInvitations();
            }

            _avatarViews.Refresh();
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = CreatePage();
            page.Spacing = Metrics.FieldSpacing;
            page.Widgets.Add(_noticeBox);
            page.Widgets.Add(ConfirmationPlaceholder);
            page.Widgets.Add(_invitationsPlaceholder);
            StackPanel.SetProportionType(_invitationsPlaceholder, ProportionType.Fill);
            RenderInvitations();

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(CreateBackBar(backButton));

            return page;
        }

        private void FollowInvitationsRequest()
        {
            if ((_invitationsRequest is null) || !_invitationsRequest.IsCompleted)
            {
                return;
            }

            Task<PendingInvitationsResponse> completedRequest = _invitationsRequest;
            _invitationsRequest = null;
            if (completedRequest.IsCompletedSuccessfully)
            {
                _playerSession.Invitations.Load(completedRequest.Result);
            }
            else
            {
                ShowServerError();
            }
        }

        private void FollowAnswer()
        {
            if ((_answerRequest is null) || !_answerRequest.IsCompleted)
            {
                return;
            }

            Task<RespondInvitationResult> completedAnswer = _answerRequest;
            _answerRequest = null;
            if (completedAnswer.IsCompletedSuccessfully)
            {
                ShowAnswerOutcome(completedAnswer.Result);
            }
            else
            {
                ShowServerError();
            }
        }

        private void ShowAnswerOutcome(RespondInvitationResult answer)
        {
            switch (answer.Status)
            {
                case RespondInvitationStatus.Accepted:
                    EnterRoom(answer.Room);
                    break;

                case RespondInvitationStatus.Rejected:
                    _playerSession.Invitations.Remove(_answeredRoomId);
                    break;

                case RespondInvitationStatus.InvitationNoLongerExists:
                    _playerSession.Invitations.Remove(_answeredRoomId);
                    _noticeBox.ShowMessage(TextKeys.Alerts.InvitationRoomGoneTitle, TextKeys.Alerts.InvitationRoomGoneBody, null);
                    break;

                case RespondInvitationStatus.RoomFull:
                    AskRoomFullChoice();
                    break;

                case RespondInvitationStatus.RoomInMatch:
                    _noticeBox.ShowTitle(TextKeys.Rooms.RoomInMatchError);
                    break;

                case RespondInvitationStatus.Banned:
                    _noticeBox.ShowBan(answer.BanEndsAtUtc);
                    break;

                case RespondInvitationStatus.AlreadyInRoom:
                    _noticeBox.ShowTitle(TextKeys.Rooms.AlreadyInRoomError);
                    break;

                case RespondInvitationStatus.DatabaseUnavailable:
                    ShowServerError();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(answer), answer.Status, null);
            }
        }

        private void EnterRoom(Room? joinedRoom)
        {
            if (joinedRoom is null)
            {
                ShowServerError();
                return;
            }

            _playerSession.Invitations.Remove(_answeredRoomId);
            _playerSession.EnterRoom(joinedRoom);
            RequestedScreen = ScreenId.Room;
        }

        private void AskRoomFullChoice()
        {
            ShowConfirmation(TextKeys.Alerts.RoomFullTitle, TextKeys.Alerts.RoomFullBody);

            LocalizedButton viewRoomsButton = CreatePrimaryButton(TextKeys.Alerts.ViewRoomsButton);
            viewRoomsButton.Click += ViewRoomsButtonOnClick;
            LocalizedButton cancelButton = CreateSecondaryButton(TextKeys.Common.CancelButton);
            cancelButton.Click += CancelButtonOnClick;
            AddConfirmationButtons(viewRoomsButton, cancelButton);
        }

        private void AcceptInvitation(int roomId)
        {
            bool canAnswer = _gameSessionConnection.IsSessionOpen && (_answerRequest is null);
            if (canAnswer)
            {
                PrepareAnswer(roomId);
                _answerRequest = _gameSessionConnection.AcceptInvitationAsync(roomId);
            }
        }

        private void RejectInvitation(int roomId)
        {
            bool canAnswer = _gameSessionConnection.IsSessionOpen && (_answerRequest is null);
            if (canAnswer)
            {
                PrepareAnswer(roomId);
                _answerRequest = _gameSessionConnection.RejectInvitationAsync(roomId);
            }
        }

        private void PrepareAnswer(int roomId)
        {
            _noticeBox.Hide();
            HideConfirmation();
            _answeredRoomId = roomId;
        }

        private void ShowServerError()
        {
            ForgetRequests();
            _noticeBox.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, null);
        }

        private void ForgetRequests()
        {
            _invitationsRequest = null;
            _answerRequest = null;
        }

        private void RenderInvitations()
        {
            _shownRevision = _playerSession.Invitations.Revision;
            _avatarViews.Clear();
            _invitationsPlaceholder.Widgets.Clear();
            IReadOnlyList<RoomInvitation> invitations = _playerSession.Invitations.Invitations;
            if (invitations.Count == 0)
            {
                _invitationsPlaceholder.Widgets.Add(CreateEmptyState(TextKeys.Invitations.EmptyStateTitle, TextKeys.Invitations.EmptyStateHint));
                return;
            }

            foreach (RoomInvitation invitation in invitations)
            {
                _invitationsPlaceholder.Widgets.Add(BuildInvitationCard(invitation));
            }
        }

        private Panel BuildInvitationCard(RoomInvitation invitation)
        {
            HorizontalStackPanel itemRow = CreateButtonRow();
            itemRow.HorizontalAlignment = HorizontalAlignment.Stretch;
            itemRow.Widgets.Add(_avatarViews.Create(invitation.Sender.AvatarReference).Root);

            var nameColumn = new VerticalStackPanel { VerticalAlignment = VerticalAlignment.Center };
            nameColumn.Widgets.Add(CreateUsernameLabel(invitation.Sender.Username));
            nameColumn.Widgets.Add(CreateHint(TextKeys.Invitations.InvitedByMessage));
            nameColumn.Widgets.Add(BuildRoomCodeRow(invitation.RoomCode));
            itemRow.Widgets.Add(nameColumn);

            var spacer = new Panel { HorizontalAlignment = HorizontalAlignment.Stretch };
            StackPanel.SetProportionType(spacer, ProportionType.Part);
            itemRow.Widgets.Add(spacer);

            LocalizedButton joinButton = CreatePrimaryButton(TextKeys.Invitations.JoinButton);
            joinButton.Click += (sender, e) => AcceptInvitation(invitation.RoomId);
            itemRow.Widgets.Add(joinButton);

            LocalizedButton rejectButton = CreateSecondaryButton(TextKeys.Invitations.RejectToolTip);
            rejectButton.Click += (sender, e) => RejectInvitation(invitation.RoomId);
            itemRow.Widgets.Add(rejectButton);

            Panel invitationCard = CreateItemCard(itemRow);

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
            ForgetRequests();
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
