using System;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class RoomFlow
    {
        private const int LoneMemberCount = 1;

        private readonly IGameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<LeaveRoomResult> _leaveRequest = new PendingRequest<LeaveRoomResult>();
        private readonly PendingRequest<KickMemberResult> _kickRequest = new PendingRequest<KickMemberResult>();
        private readonly PendingRequest<StartMatchResult> _startMatchRequest = new PendingRequest<StartMatchResult>();

        internal RoomFlow(IGameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal Room? CurrentRoom => _playerSession.CurrentRoom;

        internal bool HasAccount => _playerSession.HasAccount;

        internal string? ViewerAvatarReference => _playerSession.AvatarReference;

        internal bool NeedsLeaveConfirmation => _playerSession.CurrentRoom?.Members.Count == LoneMemberCount;

        internal ScreenId? FindRedirect()
        {
            ScreenId? redirect = _playerSession.CurrentRoom is null ? ScreenId.MainMenu : null;

            return redirect;
        }

        internal void Leave()
        {
            if (!_leaveRequest.IsWaiting)
            {
                _leaveRequest.Start(_gameSessionConnection.LeaveRoomAsync());
            }
        }

        internal void Kick(int targetMemberId)
        {
            if (!_kickRequest.IsWaiting)
            {
                _kickRequest.Start(_gameSessionConnection.KickMemberAsync(targetMemberId));
            }
        }

        internal void StartMatch()
        {
            if (!_startMatchRequest.IsWaiting)
            {
                _startMatchRequest.Start(_gameSessionConnection.StartMatchAsync());
            }
        }

        internal RoomOutcome? TakeOutcome()
        {
            RoomOutcome? outcome = FollowLeaveRequest();
            outcome = FollowKickRequest() ?? outcome;
            outcome = FollowStartMatchRequest() ?? outcome;

            return outcome;
        }

        private static RoomOutcome CreateNotice(Notice notice)
        {
            var noticeOutcome = new RoomOutcome(notice, null, null);

            return noticeOutcome;
        }

        private RoomOutcome? FollowLeaveRequest()
        {
            FinishedRequest<LeaveRoomResult>? finishedLeave = _leaveRequest.TakeFinished();
            RoomOutcome? outcome = null;
            if ((finishedLeave is not null) && finishedLeave.IsSuccessful && (finishedLeave.Reply == LeaveRoomResult.RoomInMatch))
            {
                outcome = CreateNotice(Notice.CreateTitle(TextKeys.Room.LeaveDuringMatchNotice));
            }
            else if ((finishedLeave is not null) && finishedLeave.IsSuccessful)
            {
                _playerSession.LeaveRoom();
                outcome = new RoomOutcome(null, ScreenId.MainMenu, null);
            }
            else if (finishedLeave is not null)
            {
                outcome = CreateNotice(Notice.CreateServerError());
            }

            return outcome;
        }

        private RoomOutcome? FollowKickRequest()
        {
            FinishedRequest<KickMemberResult>? finishedKick = _kickRequest.TakeFinished();
            RoomOutcome? outcome = null;
            if ((finishedKick is not null) && !finishedKick.IsSuccessful)
            {
                outcome = CreateNotice(Notice.CreateServerError());
            }
            else if ((finishedKick is not null) && (finishedKick.Reply == KickMemberResult.NotHost))
            {
                outcome = CreateNotice(Notice.CreateTitle(TextKeys.Room.HostOnlyHint));
            }

            return outcome;
        }

        private RoomOutcome? FollowStartMatchRequest()
        {
            FinishedRequest<StartMatchResult>? finishedStartMatch = _startMatchRequest.TakeFinished();
            RoomOutcome? outcome = null;
            if ((finishedStartMatch is not null) && finishedStartMatch.IsSuccessful && (finishedStartMatch.Reply is StartMatchResult startResult))
            {
                outcome = ToOutcome(startResult);
            }
            else if (finishedStartMatch is not null)
            {
                outcome = CreateNotice(Notice.CreateServerError());
            }

            return outcome;
        }

        private static RoomOutcome? ToOutcome(StartMatchResult startResult)
        {
            RoomOutcome? outcome = startResult.Status switch
            {
                StartMatchStatus.Started or StartMatchStatus.RoomInMatch => null,
                StartMatchStatus.PlayerBanned => new RoomOutcome(null, null, startResult),
                StartMatchStatus.NotEnoughPlayers => CreateNotice(Notice.CreateTitle(TextKeys.Room.NotEnoughPlayersNotice)),
                StartMatchStatus.NotHost => CreateNotice(Notice.CreateTitle(TextKeys.Room.HostOnlyHint)),
                StartMatchStatus.NotInRoom or StartMatchStatus.NotIdentified or StartMatchStatus.DatabaseUnavailable => CreateNotice(Notice.CreateServerError()),
                _ => throw new ArgumentOutOfRangeException(nameof(startResult)),
            };

            return outcome;
        }
    }
}
