using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using CoreWCF;

using Game.Contracts;
using Game.Persistence;

using log4net;

using Npgsql;

namespace Game.Services
{
    public sealed class InvitationService : IInvitationService
    {
        private const string InvitationsFeatureName = "the room invitations";
        private const string InGameChannel = "in_game";
        private const string InvitationsUnavailableMessage = "The room invitations are not available right now.";
        private const string PendingReadFailedMessage = "The database could not be read while listing the invitations of player {0}.";
        private const string InviteesReadFailedMessage = "The database could not be read while player {0} listed the invitees of room {1}.";
        private const string InvitationWriteFailedMessage = "The database could not be written while player {0} invited player {1} to room {2}.";
        private const string AnswerFailedMessage = "The database could not be used while player {0} answered the invitation to room {1}.";
        private const string NoticeFailedMessage = "The sender of the invitation of player {0} to room {1} could not be read; the invitation waits in the inbox without a live notice.";
        private const string AnsweredInvitationKeptMessage = "The invitation of player {0} to room {1} could not be deleted after it was accepted; it stays pending until the room closes.";
        private const string InvalidInviteeMessage = "Player {0} tried to invite player {1}, who cannot be invited.";
        private const string InvitationSentMessage = "Player {0} invited player {1} to room {2}.";
        private const string InvitationAcceptedMessage = "Player {0} accepted the invitation to room {1}.";
        private const string InvitationRejectedMessage = "Player {0} rejected the invitation to room {1}.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(InvitationService));

        private readonly SessionRegistry _sessionRegistry;
        private readonly RoomRegistry _roomRegistry;
        private readonly RoomService _roomService;
        private readonly RoomInvitationRepository _roomInvitationRepository;
        private readonly PlayerRepository _playerRepository;

        public InvitationService(SessionRegistry sessionRegistry, RoomRegistry roomRegistry, RoomService roomService, RoomInvitationRepository roomInvitationRepository, PlayerRepository playerRepository)
        {
            ArgumentNullException.ThrowIfNull(sessionRegistry);
            ArgumentNullException.ThrowIfNull(roomRegistry);
            ArgumentNullException.ThrowIfNull(roomService);
            ArgumentNullException.ThrowIfNull(roomInvitationRepository);
            ArgumentNullException.ThrowIfNull(playerRepository);

            _sessionRegistry = sessionRegistry;
            _roomRegistry = roomRegistry;
            _roomService = roomService;
            _roomInvitationRepository = roomInvitationRepository;
            _playerRepository = playerRepository;
        }

        /// <inheritdoc/>
        public async Task<PendingInvitationsResponse> GetPendingInvitationsAsync()
        {
            int callerPlayerId = AccountOwnerGuard.RequireAccountOwner(_sessionRegistry, _log, InvitationsFeatureName).OwnerId;
            PendingInvitationsResponse pendingInvitations;
            try
            {
                pendingInvitations = await ReadPendingInvitationsAsync(callerPlayerId);
            }
            catch (NpgsqlException exception)
            {
                string failureMessage = string.Format(CultureInfo.InvariantCulture, PendingReadFailedMessage, callerPlayerId);
                _log.Error(failureMessage, exception);
                throw new FaultException(InvitationsUnavailableMessage);
            }

            return pendingInvitations;
        }

        /// <inheritdoc/>
        public async Task<RoomInviteesResponse> GetRoomInviteesAsync()
        {
            int callerPlayerId = AccountOwnerGuard.RequireAccountOwner(_sessionRegistry, _log, InvitationsFeatureName).OwnerId;
            LiveRoom? room = _roomRegistry.FindRoomOf(callerPlayerId);
            var invitees = new RoomInviteesResponse();
            if (room is null)
            {
                return invitees;
            }

            try
            {
                invitees.InvitedPlayerIds = await _roomInvitationRepository.ListInviteeIdsAsync(room.RoomId);
            }
            catch (NpgsqlException exception)
            {
                string failureMessage = string.Format(CultureInfo.InvariantCulture, InviteesReadFailedMessage, callerPlayerId, room.RoomId);
                _log.Error(failureMessage, exception);
                throw new FaultException(InvitationsUnavailableMessage);
            }

            return invitees;
        }

        /// <inheritdoc/>
        public async Task<SendInvitationResult> SendInvitationAsync(int targetPlayerId)
        {
            SessionOwner inviter = AccountOwnerGuard.RequireAccountOwner(_sessionRegistry, _log, InvitationsFeatureName);
            LiveRoom? room = _roomRegistry.FindRoomOf(inviter.OwnerId);
            SendInvitationResult sendResult;
            if (room is null)
            {
                sendResult = SendInvitationResult.NotInRoom;
            }
            else if ((targetPlayerId <= 0) || (targetPlayerId == inviter.OwnerId))
            {
                _log.WarnFormat(CultureInfo.InvariantCulture, InvalidInviteeMessage, inviter.OwnerId, targetPlayerId);
                sendResult = SendInvitationResult.PlayerNotFound;
            }
            else if (room.FindMemberIdOf(targetPlayerId) is not null)
            {
                sendResult = SendInvitationResult.AlreadyInThisRoom;
            }
            else
            {
                var newInvitation = new RoomInvitationRecord
                {
                    RoomId = room.RoomId,
                    InviterPlayerId = inviter.OwnerId,
                    InvitedPlayerId = targetPlayerId,
                    Channel = InGameChannel,
                };
                sendResult = await StoreInvitationAsync(newInvitation, room.Code);
            }

            return sendResult;
        }

        /// <inheritdoc/>
        public async Task<RespondInvitationResult> RespondInvitationAsync(int roomId, bool accept)
        {
            SessionOwner invitee = AccountOwnerGuard.RequireAccountOwner(_sessionRegistry, _log, InvitationsFeatureName);
            var answer = new RespondInvitationResult { Status = RespondInvitationStatus.InvitationNoLongerExists };
            if (roomId <= 0)
            {
                return answer;
            }

            try
            {
                answer = accept ? await AcceptAsync(invitee, roomId) : await RejectAsync(invitee.OwnerId, roomId);
            }
            catch (NpgsqlException exception)
            {
                string failureMessage = string.Format(CultureInfo.InvariantCulture, AnswerFailedMessage, invitee.OwnerId, roomId);
                _log.Error(failureMessage, exception);
                answer = new RespondInvitationResult { Status = RespondInvitationStatus.DatabaseUnavailable };
            }

            return answer;
        }

        private async Task<PendingInvitationsResponse> ReadPendingInvitationsAsync(int callerPlayerId)
        {
            List<RoomInvitationRecord> storedInvitations = await _roomInvitationRepository.ListForInviteeAsync(callerPlayerId);
            Dictionary<int, string> roomCodesByRoomId = FindOpenRoomCodes(storedInvitations);
            List<RoomInvitationRecord> openInvitations = storedInvitations
                .Where(storedInvitation => roomCodesByRoomId.ContainsKey(storedInvitation.RoomId))
                .ToList();
            List<int> inviterIds = openInvitations
                .Select(openInvitation => openInvitation.InviterPlayerId)
                .Distinct()
                .ToList();
            List<Player> inviters = await _playerRepository.ListByIdsAsync(inviterIds);
            Dictionary<int, Player> invitersByPlayerId = inviters.ToDictionary(inviter => inviter.PlayerId);

            var pendingInvitations = new PendingInvitationsResponse
            {
                Invitations = openInvitations
                    .Where(openInvitation => invitersByPlayerId.ContainsKey(openInvitation.InviterPlayerId))
                    .Select(openInvitation => ToRoomInvitation(openInvitation, roomCodesByRoomId[openInvitation.RoomId], invitersByPlayerId[openInvitation.InviterPlayerId]))
                    .ToList(),
            };

            return pendingInvitations;
        }

        private Dictionary<int, string> FindOpenRoomCodes(List<RoomInvitationRecord> storedInvitations)
        {
            var roomCodesByRoomId = new Dictionary<int, string>();
            foreach (RoomInvitationRecord storedInvitation in storedInvitations)
            {
                LiveRoom? room = _roomRegistry.FindById(storedInvitation.RoomId);
                if (room is not null)
                {
                    roomCodesByRoomId[room.RoomId] = room.Code;
                }
            }

            return roomCodesByRoomId;
        }

        private async Task<SendInvitationResult> StoreInvitationAsync(RoomInvitationRecord newInvitation, string roomCode)
        {
            InvitationCreationOutcome creationOutcome;
            try
            {
                creationOutcome = await _roomInvitationRepository.CreateAsync(newInvitation);
            }
            catch (NpgsqlException exception)
            {
                string failureMessage = string.Format(CultureInfo.InvariantCulture, InvitationWriteFailedMessage, newInvitation.InviterPlayerId, newInvitation.InvitedPlayerId, newInvitation.RoomId);
                _log.Error(failureMessage, exception);
                return SendInvitationResult.DatabaseUnavailable;
            }

            SendInvitationResult sendResult = ToSendResult(creationOutcome);
            if (sendResult == SendInvitationResult.Sent)
            {
                _log.InfoFormat(CultureInfo.InvariantCulture, InvitationSentMessage, newInvitation.InviterPlayerId, newInvitation.InvitedPlayerId, newInvitation.RoomId);
                await NotifyInviteeAsync(newInvitation, roomCode);
            }

            return sendResult;
        }

        private async Task NotifyInviteeAsync(RoomInvitationRecord storedInvitation, string roomCode)
        {
            Player? inviter;
            try
            {
                inviter = await _playerRepository.FindByIdAsync(storedInvitation.InviterPlayerId);
            }
            catch (NpgsqlException exception)
            {
                string failureMessage = string.Format(CultureInfo.InvariantCulture, NoticeFailedMessage, storedInvitation.InvitedPlayerId, storedInvitation.RoomId);
                _log.Warn(failureMessage, exception);
                return;
            }

            if (inviter is not null)
            {
                RoomInvitation invitation = ToRoomInvitation(storedInvitation, roomCode, inviter);
                _sessionRegistry.Deliver(storedInvitation.InvitedPlayerId, new RoomInvitationNotice(invitation));
            }
        }

        private async Task<RespondInvitationResult> AcceptAsync(SessionOwner invitee, int roomId)
        {
            var answer = new RespondInvitationResult { Status = RespondInvitationStatus.InvitationNoLongerExists };
            bool isPending = await _roomInvitationRepository.ExistsAsync(roomId, invitee.OwnerId);
            if (!isPending)
            {
                return answer;
            }

            if (_roomRegistry.FindById(roomId) is null)
            {
                await _roomInvitationRepository.DeleteAsync(roomId, invitee.OwnerId);
            }
            else
            {
                answer = await EnterInvitedRoomAsync(invitee, roomId);
            }

            return answer;
        }

        private async Task<RespondInvitationResult> EnterInvitedRoomAsync(SessionOwner invitee, int roomId)
        {
            JoinRoomResult joining = await _roomService.JoinForAsync(invitee, roomId);
            var answer = new RespondInvitationResult
            {
                Status = ToAnswerStatus(joining.Status),
                Room = joining.Room,
                BanEndsAtUtc = joining.BanEndsAtUtc,
            };
            if (joining.Status == JoinRoomStatus.Joined)
            {
                _log.InfoFormat(CultureInfo.InvariantCulture, InvitationAcceptedMessage, invitee.OwnerId, roomId);
                await ForgetAcceptedInvitationAsync(invitee.OwnerId, roomId);
            }

            return answer;
        }

        private async Task ForgetAcceptedInvitationAsync(int inviteeId, int roomId)
        {
            try
            {
                await _roomInvitationRepository.DeleteAsync(roomId, inviteeId);
            }
            catch (NpgsqlException exception)
            {
                string failureMessage = string.Format(CultureInfo.InvariantCulture, AnsweredInvitationKeptMessage, inviteeId, roomId);
                _log.Warn(failureMessage, exception);
            }
        }

        private async Task<RespondInvitationResult> RejectAsync(int inviteeId, int roomId)
        {
            bool wasDeleted = await _roomInvitationRepository.DeleteAsync(roomId, inviteeId);
            var answer = new RespondInvitationResult { Status = RespondInvitationStatus.InvitationNoLongerExists };
            if (wasDeleted)
            {
                _log.InfoFormat(CultureInfo.InvariantCulture, InvitationRejectedMessage, inviteeId, roomId);
                answer.Status = RespondInvitationStatus.Rejected;
            }

            return answer;
        }

        private static SendInvitationResult ToSendResult(InvitationCreationOutcome creationOutcome)
        {
            SendInvitationResult sendResult = creationOutcome switch
            {
                InvitationCreationOutcome.Created => SendInvitationResult.Sent,
                InvitationCreationOutcome.AlreadyPending => SendInvitationResult.InvitationAlreadyPending,
                InvitationCreationOutcome.PlayerMissing => SendInvitationResult.PlayerNotFound,
                _ => throw new ArgumentOutOfRangeException(nameof(creationOutcome)),
            };

            return sendResult;
        }

        private static RespondInvitationStatus ToAnswerStatus(JoinRoomStatus joinStatus)
        {
            RespondInvitationStatus answerStatus = joinStatus switch
            {
                JoinRoomStatus.Joined => RespondInvitationStatus.Accepted,
                JoinRoomStatus.AlreadyInRoom => RespondInvitationStatus.AlreadyInRoom,
                JoinRoomStatus.RoomNotFound => RespondInvitationStatus.InvitationNoLongerExists,
                JoinRoomStatus.RoomFull => RespondInvitationStatus.RoomFull,
                JoinRoomStatus.RoomInMatch => RespondInvitationStatus.RoomInMatch,
                JoinRoomStatus.Banned => RespondInvitationStatus.Banned,
                JoinRoomStatus.DatabaseUnavailable => RespondInvitationStatus.DatabaseUnavailable,
                _ => throw new ArgumentOutOfRangeException(nameof(joinStatus)),
            };

            return answerStatus;
        }

        private static RoomInvitation ToRoomInvitation(RoomInvitationRecord storedInvitation, string roomCode, Player inviter)
        {
            var invitation = new RoomInvitation
            {
                RoomId = storedInvitation.RoomId,
                RoomCode = roomCode,
                Sender = new PlayerSummary
                {
                    PlayerId = inviter.PlayerId,
                    Username = inviter.Username,
                    AvatarReference = inviter.AvatarReference,
                },
                SentAtUtc = storedInvitation.CreatedAt,
            };

            return invitation;
        }
    }
}
