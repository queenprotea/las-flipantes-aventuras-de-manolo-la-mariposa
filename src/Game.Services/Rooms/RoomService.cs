using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Persistence;

using log4net;

using Npgsql;

namespace Game.Services
{
    public sealed class RoomService : IRoomService
    {
        private const int MostCodeDraws = 10;
        private const string RoomCreatedMessage = "Room {0} created by player {1}.";
        private const string NoFreeCodeMessage = "No free room code was found after {0} draws.";
        private const string CreationFailedMessage = "The database could not be used while player {0} created a room.";
        private const string JoinFailedMessage = "The database could not be used while player {0} joined room {1}.";
        private const string MemberJoinedMessage = "Player {0} joined room {1}.";
        private const string MemberLeftMessage = "Player {0} left room {1}.";
        private const string MemberKickedMessage = "Player {0} was removed from room {1} by its host.";
        private const string MemberTimedOutMessage = "Player {0} lost their seat in room {1} after being inactive.";
        private const string RoomClosedMessage = "Room {0} closed because nobody was left.";
        private const string CloseFailedMessage = "Room {0} could not be closed in the database; the next start up closes it.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(RoomService));

        private readonly RoomRegistry _roomRegistry;
        private readonly SessionRegistry _sessionRegistry;
        private readonly RoomRecordRepository _roomRecordRepository;
        private readonly SanctionRepository _sanctionRepository;
        private readonly TimeProvider _timeProvider;

        public RoomService(RoomRegistry roomRegistry, SessionRegistry sessionRegistry, RoomRecordRepository roomRecordRepository, SanctionRepository sanctionRepository, TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(roomRegistry);
            ArgumentNullException.ThrowIfNull(sessionRegistry);
            ArgumentNullException.ThrowIfNull(roomRecordRepository);
            ArgumentNullException.ThrowIfNull(sanctionRepository);
            ArgumentNullException.ThrowIfNull(timeProvider);

            _roomRegistry = roomRegistry;
            _sessionRegistry = sessionRegistry;
            _roomRecordRepository = roomRecordRepository;
            _sanctionRepository = sanctionRepository;
            _timeProvider = timeProvider;
            _sessionRegistry.SessionLost += SessionRegistryOnSessionLost;
        }

        public async Task<CreateRoomResult> CreateRoomAsync(RoomVisibility visibility)
        {
            PlayerSession? caller = _sessionRegistry.FindCaller();
            CreateRoomResult creation;
            if (caller is null)
            {
                creation = new CreateRoomResult { Status = CreateRoomStatus.NotIdentified };
            }
            else if (!Enum.IsDefined(visibility))
            {
                creation = new CreateRoomResult { Status = CreateRoomStatus.InvalidVisibility };
            }
            else
            {
                creation = await CreateForAsync(caller.Owner, visibility);
            }

            return creation;
        }

        public RoomListResponse GetRooms()
        {
            List<RoomSummary> publicRooms = _roomRegistry.ListRooms()
                .Where(IsListed)
                .Select(ToSummary)
                .ToList();
            var roomList = new RoomListResponse { Rooms = publicRooms };

            return roomList;
        }

        public RoomListResponse FindRoom(string roomCode)
        {
            string? normalizedCode = RoomCodeAlphabet.Normalize(roomCode);
            LiveRoom? foundRoom = normalizedCode is null ? null : _roomRegistry.FindByCode(normalizedCode);
            var roomList = new RoomListResponse();
            if (foundRoom is not null)
            {
                roomList.Rooms.Add(foundRoom.BuildSummary());
            }

            return roomList;
        }

        public async Task<JoinRoomResult> JoinRoomAsync(int roomId)
        {
            PlayerSession? caller = _sessionRegistry.FindCaller();
            JoinRoomResult joining;
            if (caller is null)
            {
                joining = new JoinRoomResult { Status = JoinRoomStatus.NotIdentified };
            }
            else
            {
                joining = await JoinForAsync(caller.Owner, roomId);
            }

            return joining;
        }

        public async Task<LeaveRoomResult> LeaveRoomAsync()
        {
            PlayerSession? caller = _sessionRegistry.FindCaller();
            bool hasLeft = (caller is not null) && await LeaveForAsync(caller.Owner.PlayerId);
            LeaveRoomResult leaving = hasLeft ? LeaveRoomResult.Left : LeaveRoomResult.NotInRoom;

            return leaving;
        }

        public RoomLookupResponse GetCurrentRoom()
        {
            PlayerSession? caller = _sessionRegistry.FindCaller();
            Room? currentRoom = caller is null ? null : FindViewFor(caller.Owner.PlayerId);
            var lookup = new RoomLookupResponse
            {
                Status = currentRoom is null ? RoomLookupStatus.NotInRoom : RoomLookupStatus.InRoom,
                Room = currentRoom,
            };

            return lookup;
        }

        public KickMemberResult KickMember(int targetMemberId)
        {
            PlayerSession? caller = _sessionRegistry.FindCaller();
            LiveRoom? room = caller is null ? null : _roomRegistry.FindRoomOf(caller.Owner.PlayerId);
            KickMemberResult kickResult = KickMemberResult.NotInRoom;
            if ((caller is not null) && (room is not null))
            {
                RoomKick kick = room.Kick(caller.Owner.PlayerId, targetMemberId);
                FollowKick(room, kick);
                kickResult = kick.Result;
            }

            return kickResult;
        }

        internal Room? Reattach(int playerId)
        {
            LiveRoom? room = _roomRegistry.FindRoomOf(playerId);
            if ((room is not null) && room.MarkActive(playerId))
            {
                NotifyMembers(room, RoomChangeReason.MemberReturned, playerId);
            }

            Room? currentRoom = room?.BuildViewFor(playerId);

            return currentRoom;
        }

        internal async Task<bool> LeaveForAsync(int playerId)
        {
            LiveRoom? room = _roomRegistry.FindRoomOf(playerId);
            if (room is null)
            {
                return false;
            }

            RoomDeparture departure = room.Remove(playerId);
            _roomRegistry.ForgetMembership(playerId);
            _log.InfoFormat(MemberLeftMessage, playerId, room.RoomId);
            await FollowDepartureAsync(room, departure, RoomChangeReason.MemberLeft);

            return departure.WasMember;
        }

        internal async Task RemoveExpiredMembersAsync(TimeSpan inactivityLimit)
        {
            DateTime limitUtc = NowUtc() - inactivityLimit;
            foreach (LiveRoom room in _roomRegistry.ListRooms())
            {
                await ExpireMembersOfAsync(room, limitUtc);
            }
        }

        private static bool IsListed(LiveRoom room)
        {
            bool isListed = room.IsPublic;

            return isListed;
        }

        private static RoomSummary ToSummary(LiveRoom room)
        {
            RoomSummary summary = room.BuildSummary();

            return summary;
        }

        private static string ToVisibilityCode(RoomVisibility visibility)
        {
            string visibilityCode = visibility switch
            {
                RoomVisibility.Public => RoomVisibilityCodes.Public,
                RoomVisibility.Private => RoomVisibilityCodes.Private,
                _ => throw new ArgumentOutOfRangeException(nameof(visibility)),
            };

            return visibilityCode;
        }

        private async Task<CreateRoomResult> CreateForAsync(SessionOwner owner, RoomVisibility visibility)
        {
            CreateRoomResult creation;
            try
            {
                Sanction? ban = await _sanctionRepository.FindInForceAsync(owner.PlayerId, NowUtc());
                if (ban is null)
                {
                    creation = await OpenRoomAsync(owner, visibility);
                }
                else
                {
                    creation = new CreateRoomResult { Status = CreateRoomStatus.Banned, BanEndsAtUtc = ban.EffectiveUntil };
                }
            }
            catch (NpgsqlException exception)
            {
                _log.Error(string.Format(CultureInfo.InvariantCulture, CreationFailedMessage, owner.PlayerId), exception);
                creation = new CreateRoomResult { Status = CreateRoomStatus.DatabaseUnavailable };
            }

            return creation;
        }

        private async Task<CreateRoomResult> OpenRoomAsync(SessionOwner owner, RoomVisibility visibility)
        {
            CreateRoomResult creation;
            if (!_roomRegistry.Reserve(owner.PlayerId))
            {
                creation = new CreateRoomResult { Status = CreateRoomStatus.AlreadyInRoom };
            }
            else
            {
                try
                {
                    creation = await StoreAndSeatHostAsync(owner, visibility);
                }
                finally
                {
                    _roomRegistry.Release(owner.PlayerId);
                }
            }

            return creation;
        }

        private async Task<CreateRoomResult> StoreAndSeatHostAsync(SessionOwner owner, RoomVisibility visibility)
        {
            string roomCode = string.Empty;
            int? roomId = null;
            int drawCount = 0;
            while ((roomId is null) && (drawCount < MostCodeDraws))
            {
                drawCount++;
                roomCode = RoomCodeAlphabet.Draw();
                roomId = await StoreIfCodeIsFreeAsync(roomCode, visibility);
            }

            CreateRoomResult creation;
            if (roomId is null)
            {
                _log.WarnFormat(NoFreeCodeMessage, MostCodeDraws);
                creation = new CreateRoomResult { Status = CreateRoomStatus.DatabaseUnavailable };
            }
            else
            {
                creation = SeatHost(owner, new LiveRoom(roomId.Value, roomCode, visibility));
            }

            return creation;
        }

        private async Task<int?> StoreIfCodeIsFreeAsync(string roomCode, RoomVisibility visibility)
        {
            if (_roomRegistry.FindByCode(roomCode) is not null)
            {
                return null;
            }

            int? roomId = await _roomRecordRepository.CreateAsync(roomCode, ToVisibilityCode(visibility));

            return roomId;
        }

        private CreateRoomResult SeatHost(SessionOwner owner, LiveRoom room)
        {
            room.Admit(owner);
            _roomRegistry.AddRoom(room, owner.PlayerId);
            _log.InfoFormat(RoomCreatedMessage, room.RoomId, owner.PlayerId);
            var creation = new CreateRoomResult
            {
                Status = CreateRoomStatus.Created,
                Room = room.BuildViewFor(owner.PlayerId),
            };

            return creation;
        }

        private async Task<JoinRoomResult> JoinForAsync(SessionOwner owner, int roomId)
        {
            JoinRoomResult joining;
            try
            {
                Sanction? ban = await _sanctionRepository.FindInForceAsync(owner.PlayerId, NowUtc());
                if (ban is null)
                {
                    joining = Seat(owner, roomId);
                }
                else
                {
                    joining = new JoinRoomResult { Status = JoinRoomStatus.Banned, BanEndsAtUtc = ban.EffectiveUntil };
                }
            }
            catch (NpgsqlException exception)
            {
                _log.Error(string.Format(CultureInfo.InvariantCulture, JoinFailedMessage, owner.PlayerId, roomId), exception);
                joining = new JoinRoomResult { Status = JoinRoomStatus.DatabaseUnavailable };
            }

            return joining;
        }

        private JoinRoomResult Seat(SessionOwner owner, int roomId)
        {
            if (!_roomRegistry.Reserve(owner.PlayerId))
            {
                var alreadyInRoom = new JoinRoomResult { Status = JoinRoomStatus.AlreadyInRoom };
                return alreadyInRoom;
            }

            LiveRoom? room = _roomRegistry.FindById(roomId);
            JoinRoomStatus admission = room is null ? JoinRoomStatus.RoomNotFound : room.Admit(owner);
            var joining = new JoinRoomResult { Status = admission };
            if ((admission == JoinRoomStatus.Joined) && (room is not null))
            {
                _roomRegistry.RecordMembership(owner.PlayerId, room);
                _log.InfoFormat(MemberJoinedMessage, owner.PlayerId, room.RoomId);
                NotifyMembers(room, RoomChangeReason.MemberJoined, owner.PlayerId);
                joining.Room = room.BuildViewFor(owner.PlayerId);
            }
            else
            {
                _roomRegistry.Release(owner.PlayerId);
            }

            return joining;
        }

        private Room? FindViewFor(int playerId)
        {
            Room? currentRoom = _roomRegistry.FindRoomOf(playerId)?.BuildViewFor(playerId);

            return currentRoom;
        }

        private void FollowKick(LiveRoom room, RoomKick kick)
        {
            if ((kick.KickedPlayerId is not int kickedPlayerId) || (kick.KickedView is null))
            {
                return;
            }

            _roomRegistry.ForgetMembership(kickedPlayerId);
            _log.InfoFormat(MemberKickedMessage, kickedPlayerId, room.RoomId);
            _sessionRegistry.Deliver(kickedPlayerId, new RoomChangeNotice(RoomChangeReason.YouWereKicked, kick.KickedView));
            NotifyMembers(room, RoomChangeReason.MemberKicked, null);
        }

        private async Task ExpireMembersOfAsync(LiveRoom room, DateTime limitUtc)
        {
            RoomExpiry expiry = room.RemoveExpired(limitUtc);
            foreach (int removedPlayerId in expiry.RemovedPlayerIds)
            {
                _roomRegistry.ForgetMembership(removedPlayerId);
                _log.InfoFormat(MemberTimedOutMessage, removedPlayerId, room.RoomId);
            }

            if (expiry.RemovedPlayerIds.Count > 0)
            {
                var departure = new RoomDeparture(true, expiry.HasHostChanged, expiry.IsRoomEmpty);
                await FollowDepartureAsync(room, departure, RoomChangeReason.MemberTimedOut);
            }
        }

        private async Task FollowDepartureAsync(LiveRoom room, RoomDeparture departure, RoomChangeReason reason)
        {
            if (departure.IsRoomEmpty)
            {
                _roomRegistry.RemoveRoom(room);
                await CloseRecordAsync(room);
            }
            else if (departure.WasMember)
            {
                RoomChangeReason noticeReason = departure.HasHostChanged ? RoomChangeReason.HostChanged : reason;
                NotifyMembers(room, noticeReason, null);
            }
        }

        private async Task CloseRecordAsync(LiveRoom room)
        {
            try
            {
                await _roomRecordRepository.CloseAsync(room.RoomId, NowUtc());
                _log.InfoFormat(RoomClosedMessage, room.RoomId);
            }
            catch (NpgsqlException exception)
            {
                _log.Error(string.Format(CultureInfo.InvariantCulture, CloseFailedMessage, room.RoomId), exception);
            }
        }

        private void NotifyMembers(LiveRoom room, RoomChangeReason reason, int? excludedPlayerId)
        {
            foreach (RoomRecipientView recipient in room.BuildViewsForMembers())
            {
                if (recipient.PlayerId == excludedPlayerId)
                {
                    continue;
                }

                _sessionRegistry.Deliver(recipient.PlayerId, new RoomChangeNotice(reason, recipient.View));
            }
        }

        private void SessionRegistryOnSessionLost(object? sender, SessionLostEventArgs e)
        {
            LiveRoom? room = _roomRegistry.FindRoomOf(e.PlayerId);
            RoomActivityChange activityChange = room?.MarkInactive(e.PlayerId, NowUtc()) ?? new RoomActivityChange(false, false);
            if ((room is not null) && activityChange.HasChanged)
            {
                RoomChangeReason noticeReason = activityChange.HasHostChanged ? RoomChangeReason.HostChanged : RoomChangeReason.MemberWentInactive;
                NotifyMembers(room, noticeReason, e.PlayerId);
            }
        }

        private DateTime NowUtc()
        {
            DateTime nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

            return nowUtc;
        }
    }
}
