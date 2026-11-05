using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Match;
using Game.Persistence;

using log4net;

using Npgsql;

namespace Game.Services
{
    public sealed class MatchService : IMatchService
    {
        private const string TurnSecondsParameterCode = "turn_seconds";
        private const string MatchStartedMessage = "Match {0} started in room {1} with {2} players.";
        private const string MatchEndedMessage = "Player {0} ended the match in room {1}.";
        private const string StartFailedMessage = "The database could not be used while a match was starting in room {0}.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(MatchService));

        private readonly RoomRegistry _roomRegistry;
        private readonly RoomService _roomService;
        private readonly SessionRegistry _sessionRegistry;
        private readonly SystemParameterReader _systemParameterReader;
        private readonly TimeProvider _timeProvider;
        private readonly MatchCoordinator _coordinator;

        public MatchService(RoomRegistry roomRegistry, RoomService roomService, SessionRegistry sessionRegistry, SystemParameterReader systemParameterReader, TimeProvider timeProvider, StartingBoard startingBoard)
        {
            ArgumentNullException.ThrowIfNull(roomRegistry);
            ArgumentNullException.ThrowIfNull(roomService);
            ArgumentNullException.ThrowIfNull(sessionRegistry);
            ArgumentNullException.ThrowIfNull(systemParameterReader);
            ArgumentNullException.ThrowIfNull(timeProvider);
            ArgumentNullException.ThrowIfNull(startingBoard);

            _roomRegistry = roomRegistry;
            _roomService = roomService;
            _sessionRegistry = sessionRegistry;
            _systemParameterReader = systemParameterReader;
            _timeProvider = timeProvider;
            _coordinator = new MatchCoordinator(timeProvider, new MatchNoticeForwarder(sessionRegistry), startingBoard.FlowerCells);
        }

        /// <inheritdoc/>
        public async Task<StartMatchResult> StartMatchAsync()
        {
            PlayerSession? caller = _sessionRegistry.FindCaller();
            LiveRoom? room = caller is null ? null : _roomRegistry.FindRoomOf(caller.Owner.OwnerId);
            StartMatchResult startResult;
            if (caller is null)
            {
                startResult = new StartMatchResult { Status = StartMatchStatus.NotIdentified };
            }
            else if (room is null)
            {
                startResult = new StartMatchResult { Status = StartMatchStatus.NotInRoom };
            }
            else
            {
                startResult = await StartMatchInRoomAsync(room, caller.Owner.OwnerId);
            }

            return startResult;
        }

        /// <inheritdoc/>
        public PlacementResult PlaceCaterpillar(int row, int column)
        {
            PlacementResult placementResult = Place(row, column, _coordinator.PlaceCaterpillar);

            return placementResult;
        }

        /// <inheritdoc/>
        public PlacementResult PlaceButterfly(int row, int column)
        {
            PlacementResult placementResult = Place(row, column, _coordinator.PlaceButterfly);

            return placementResult;
        }

        /// <inheritdoc/>
        public EndMatchResult EndMatch()
        {
            PlayerSession? caller = _sessionRegistry.FindCaller();
            EndMatchResult endResult = caller is null ? EndMatchResult.NotIdentified : EndMatchOf(caller.Owner.OwnerId);

            return endResult;
        }

        private static StartMatchResult ToBannedResult(MatchBan ban)
        {
            var bannedResult = new StartMatchResult { Status = StartMatchStatus.PlayerBanned, BannedDisplayName = ban.DisplayName, IsBanPermanent = ban.IsPermanent, BanTimeLeft = ban.TimeLeft };

            return bannedResult;
        }

        private EndMatchResult EndMatchOf(int ownerId)
        {
            EndMatchResult endResult = _coordinator.EndMatch(ownerId);
            LiveRoom? room = _roomRegistry.FindRoomOf(ownerId);
            if ((endResult == EndMatchResult.Ended) && (room is not null) && room.EndMatch())
            {
                int? noExcludedOwnerId = null;
                _roomService.NotifyMembers(room, RoomChangeReason.MatchEnded, noExcludedOwnerId);
                _log.InfoFormat(MatchEndedMessage, ownerId, room.RoomId);
            }

            return endResult;
        }

        private PlacementResult Place(int row, int column, Func<int, MatchCell, PlacementResult> placeInMatch)
        {
            PlayerSession? caller = _sessionRegistry.FindCaller();
            var cell = new MatchCell { Row = row, Column = column };
            PlacementResult placementResult;
            if (caller is null)
            {
                placementResult = PlacementResult.NotIdentified;
            }
            else if (!MatchCoordinator.BoardContains(cell))
            {
                placementResult = PlacementResult.OutsideBoard;
            }
            else
            {
                placementResult = placeInMatch(caller.Owner.OwnerId, cell);
            }

            return placementResult;
        }

        private async Task<StartMatchResult> StartMatchInRoomAsync(LiveRoom room, int callerOwnerId)
        {
            StartMatchResult startResult;
            try
            {
                startResult = await CheckAndStartMatchAsync(room, callerOwnerId);
            }
            catch (NpgsqlException exception)
            {
                string failureMessage = string.Format(CultureInfo.InvariantCulture, StartFailedMessage, room.RoomId);
                _log.Error(failureMessage, exception);
                startResult = new StartMatchResult { Status = StartMatchStatus.DatabaseUnavailable };
            }

            return startResult;
        }

        private async Task<StartMatchResult> CheckAndStartMatchAsync(LiveRoom room, int callerOwnerId)
        {
            MatchStartCheck startCheck = room.CheckMatchStart(callerOwnerId);
            MatchBan? ban = startCheck.Status == StartMatchStatus.Started ? await FindFirstBanAsync(startCheck.Entrants) : null;
            StartMatchResult startResult;
            if (startCheck.Status != StartMatchStatus.Started)
            {
                startResult = new StartMatchResult { Status = startCheck.Status };
            }
            else if (ban is not null)
            {
                startResult = ToBannedResult(ban);
            }
            else
            {
                startResult = await StartCheckedMatchAsync(room, callerOwnerId);
            }

            return startResult;
        }

        private async Task<StartMatchResult> StartCheckedMatchAsync(LiveRoom room, int callerOwnerId)
        {
            TimeSpan placementTime = await ReadPlacementTimeAsync();
            MatchStartCheck startCheck = room.StartMatch(callerOwnerId);
            if (startCheck.Status == StartMatchStatus.Started)
            {
                int? noExcludedOwnerId = null;
                _roomService.NotifyMembers(room, RoomChangeReason.MatchStarted, noExcludedOwnerId);
                int matchId = await _coordinator.StartMatchAsync(startCheck.Entrants, placementTime);
                int seatedCount = startCheck.Entrants.Count(entrant => entrant.IsActive);
                _log.InfoFormat(MatchStartedMessage, matchId, room.RoomId, seatedCount);
            }

            var startResult = new StartMatchResult { Status = startCheck.Status };

            return startResult;
        }

        private async Task<MatchBan?> FindFirstBanAsync(IReadOnlyList<MatchEntrant> entrants)
        {
            List<MatchEntrant> activeEntrants = entrants.Where(entrant => entrant.IsActive).ToList();
            MatchBan? firstBan = null;
            foreach (MatchEntrant activeEntrant in activeEntrants)
            {
                Sanction? sanction = await _roomService.FindBanAsync(activeEntrant.EntrantId, activeEntrant.IsGuest);
                if (sanction is not null)
                {
                    firstBan = ToMatchBan(activeEntrant, sanction);
                    break;
                }
            }

            return firstBan;
        }

        private MatchBan ToMatchBan(MatchEntrant bannedEntrant, Sanction sanction)
        {
            DateTime nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
            TimeSpan banTimeLeft = TimeSpan.Zero;
            if (sanction.EffectiveUntil is DateTime effectiveUntilUtc)
            {
                banTimeLeft = effectiveUntilUtc - nowUtc;
            }

            var ban = new MatchBan(bannedEntrant.DisplayName, sanction.EffectiveUntil is null, banTimeLeft);

            return ban;
        }

        private async Task<TimeSpan> ReadPlacementTimeAsync()
        {
            int turnSeconds = await _systemParameterReader.ReadPositiveAsync(TurnSecondsParameterCode);
            TimeSpan placementTime = TimeSpan.FromSeconds(turnSeconds);

            return placementTime;
        }
    }
}
