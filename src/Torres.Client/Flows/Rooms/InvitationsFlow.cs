using System;
using System.Collections.Generic;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class InvitationsFlow
    {
        private readonly IGameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<PendingInvitationsResponse> _invitationsRequest = new PendingRequest<PendingInvitationsResponse>();
        private readonly PendingRequest<RespondInvitationResult> _answerRequest = new PendingRequest<RespondInvitationResult>();

        private int _answeredRoomId;

        internal InvitationsFlow(IGameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal IReadOnlyList<RoomInvitation> Invitations => _playerSession.Invitations.Invitations;

        internal int Revision => _playerSession.Invitations.Revision;

        internal static ScreenId FindScreenToGoBack()
        {
            ScreenId nextScreen = ScreenId.MainMenu;

            return nextScreen;
        }

        internal ScreenId? Open()
        {
            ForgetRequests();
            ScreenId? redirect = null;
            if (!_playerSession.HasAccount || !_gameSessionConnection.IsSessionOpen)
            {
                redirect = ScreenId.Login;
            }
            else if (_playerSession.CurrentRoom is not null)
            {
                redirect = ScreenId.Room;
            }
            else
            {
                _invitationsRequest.Start(_gameSessionConnection.GetPendingInvitationsAsync());
            }

            return redirect;
        }

        internal bool Accept(int roomId)
        {
            bool canAnswer = _gameSessionConnection.IsSessionOpen && !_answerRequest.IsWaiting;
            if (canAnswer)
            {
                _answeredRoomId = roomId;
                _answerRequest.Start(_gameSessionConnection.AcceptInvitationAsync(roomId));
            }

            return canAnswer;
        }

        internal bool Reject(int roomId)
        {
            bool canAnswer = _gameSessionConnection.IsSessionOpen && !_answerRequest.IsWaiting;
            if (canAnswer)
            {
                _answeredRoomId = roomId;
                _answerRequest.Start(_gameSessionConnection.RejectInvitationAsync(roomId));
            }

            return canAnswer;
        }

        internal InvitationsOutcome? TakeOutcome()
        {
            InvitationsOutcome? invitationsOutcome = FollowInvitationsRequest();
            InvitationsOutcome? answerOutcome = FollowAnswerRequest();
            InvitationsOutcome? outcome = answerOutcome ?? invitationsOutcome;

            return outcome;
        }

        internal void ForgetRequests()
        {
            _invitationsRequest.Forget();
            _answerRequest.Forget();
        }

        private static InvitationsOutcome CreateNotice(Notice notice)
        {
            var noticeOutcome = new InvitationsOutcome(notice, null, false);

            return noticeOutcome;
        }

        private InvitationsOutcome? FollowInvitationsRequest()
        {
            InvitationsOutcome? outcome = null;
            FinishedRequest<PendingInvitationsResponse>? finishedInvitations = _invitationsRequest.TakeFinished();
            if ((finishedInvitations is not null) && finishedInvitations.IsSuccessful && (finishedInvitations.Reply is PendingInvitationsResponse pendingInvitations))
            {
                _playerSession.Invitations.Load(pendingInvitations);
            }
            else if (finishedInvitations is not null)
            {
                outcome = CreateServerError();
            }

            return outcome;
        }

        private InvitationsOutcome? FollowAnswerRequest()
        {
            InvitationsOutcome? outcome = null;
            FinishedRequest<RespondInvitationResult>? finishedAnswer = _answerRequest.TakeFinished();
            if ((finishedAnswer is not null) && finishedAnswer.IsSuccessful && (finishedAnswer.Reply is RespondInvitationResult answer))
            {
                outcome = FollowAnswer(answer);
            }
            else if (finishedAnswer is not null)
            {
                outcome = CreateServerError();
            }

            return outcome;
        }

        private InvitationsOutcome CreateServerError()
        {
            ForgetRequests();
            InvitationsOutcome serverError = CreateNotice(Notice.CreateServerError());

            return serverError;
        }

        private InvitationsOutcome? FollowAnswer(RespondInvitationResult answer)
        {
            InvitationsOutcome? outcome = null;
            switch (answer.Status)
            {
                case RespondInvitationStatus.Accepted:
                    outcome = EnterRoom(answer.Room);
                    break;

                case RespondInvitationStatus.Rejected:
                    _playerSession.Invitations.Remove(_answeredRoomId);
                    break;

                case RespondInvitationStatus.InvitationNoLongerExists:
                    _playerSession.Invitations.Remove(_answeredRoomId);
                    outcome = CreateNotice(Notice.CreateMessage(TextKeys.Alerts.InvitationRoomGoneTitle, TextKeys.Alerts.InvitationRoomGoneBody, null));
                    break;

                case RespondInvitationStatus.RoomFull:
                    outcome = new InvitationsOutcome(null, null, true);
                    break;

                case RespondInvitationStatus.RoomInMatch:
                    outcome = CreateNotice(Notice.CreateTitle(TextKeys.Rooms.RoomInMatchError));
                    break;

                case RespondInvitationStatus.Banned:
                    outcome = CreateNotice(Notice.CreateBan(answer.BanEndsAtUtc));
                    break;

                case RespondInvitationStatus.AlreadyInRoom:
                    outcome = CreateNotice(Notice.CreateTitle(TextKeys.Rooms.AlreadyInRoomError));
                    break;

                case RespondInvitationStatus.DatabaseUnavailable:
                    outcome = CreateServerError();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(answer), answer.Status, null);
            }

            return outcome;
        }

        private InvitationsOutcome EnterRoom(Room? joinedRoom)
        {
            if (joinedRoom is null)
            {
                InvitationsOutcome serverError = CreateServerError();

                return serverError;
            }

            _playerSession.Invitations.Remove(_answeredRoomId);
            _playerSession.EnterRoom(joinedRoom);
            var enteredRoom = new InvitationsOutcome(null, ScreenId.Room, false);

            return enteredRoom;
        }
    }
}
