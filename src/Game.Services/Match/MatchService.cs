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

        private static readonly ILog _matchLog = LogManager.GetLogger(typeof(MatchService));

        private readonly RoomRegistry _roomRegistry;
        private readonly RoomService _roomService;
        private readonly SessionRegistry _sessionRegistry;
        private readonly SystemParameterReader _systemParameterReader;
        private readonly MatchRuleReader _matchRuleReader;
        private readonly TimeProvider _timeProvider;
        private readonly MatchCoordinator _matchCoordinator;
        private readonly MatchRoomReturn _matchRoomReturn;

        public MatchService(RoomRegistry roomRegistry, RoomService roomService, SessionRegistry sessionRegistry, SystemParameterReader systemParameterReader, MatchRuleReader matchRuleReader, TimeProvider timeProvider, StartingBoard startingBoard, Random tieBreakRandom)
        {
            ArgumentNullException.ThrowIfNull(roomRegistry);
            ArgumentNullException.ThrowIfNull(roomService);
            ArgumentNullException.ThrowIfNull(sessionRegistry);
            ArgumentNullException.ThrowIfNull(systemParameterReader);
            ArgumentNullException.ThrowIfNull(matchRuleReader);
            ArgumentNullException.ThrowIfNull(timeProvider);
            ArgumentNullException.ThrowIfNull(startingBoard);
            ArgumentNullException.ThrowIfNull(tieBreakRandom);

            _roomRegistry = roomRegistry;
            _roomService = roomService;
            _sessionRegistry = sessionRegistry;
            _systemParameterReader = systemParameterReader;
            _matchRuleReader = matchRuleReader;
            _timeProvider = timeProvider;
            _matchRoomReturn = new MatchRoomReturn(roomRegistry, roomService);
            _matchCoordinator = new MatchCoordinator(timeProvider, new MatchNoticeForwarder(sessionRegistry, _matchRoomReturn), startingBoard.FlowerCells, tieBreakRandom);
        }

        /// <inheritdoc/>
        public async Task<StartMatchResult> StartMatchAsync()
        {
            PlayerSession? callerSession = _sessionRegistry.FindCaller();
            LiveRoom? callerRoom = callerSession is null ? null : _roomRegistry.FindRoomOf(callerSession.Owner.OwnerId);
            StartMatchResult startResult;
            if (callerSession is null)
            {
                startResult = new StartMatchResult { Status = StartMatchStatus.NotIdentified };
            }
            else if (callerRoom is null)
            {
                startResult = new StartMatchResult { Status = StartMatchStatus.NotInRoom };
            }
            else
            {
                startResult = await StartMatchInRoomAsync(callerRoom, callerSession.Owner.OwnerId);
            }

            return startResult;
        }

        /// <inheritdoc/>
        public PlacementResult PlaceCaterpillar(int row, int column)
        {
            PlacementResult placementResult = PlaceForCaller(row, column, _matchCoordinator.PlaceCaterpillar);

            return placementResult;
        }

        /// <inheritdoc/>
        public PlacementResult PlaceButterfly(int row, int column)
        {
            PlacementResult placementResult = PlaceForCaller(row, column, _matchCoordinator.PlaceButterfly);

            return placementResult;
        }

        /// <inheritdoc/>
        public EndMatchResult EndMatch()
        {
            PlayerSession? callerSession = _sessionRegistry.FindCaller();
            EndMatchResult endResult = callerSession is null ? EndMatchResult.NotIdentified : EndMatchOf(callerSession.Owner.OwnerId);

            return endResult;
        }

        private static StartMatchResult ToBannedResult(MatchBan ban)
        {
            var bannedResult = new StartMatchResult { Status = StartMatchStatus.PlayerBanned, BannedDisplayName = ban.DisplayName, IsBanPermanent = ban.IsPermanent, BanTimeLeft = ban.BanTimeLeft };

            return bannedResult;
        }

        private EndMatchResult EndMatchOf(int ownerId)
        {
            EndMatchResult endResult = _matchCoordinator.EndMatch(ownerId);
            LiveRoom? returnedRoom = endResult == EndMatchResult.Ended ? _matchRoomReturn.ReturnRoomOf(ownerId, RoomChangeReason.MatchEnded) : null;
            if (returnedRoom is not null)
            {
                _matchLog.InfoFormat(MatchEndedMessage, ownerId, returnedRoom.RoomId);
            }

            return endResult;
        }

        private PlacementResult PlaceForCaller(int row, int column, Func<int, MatchCell, PlacementResult> placeInMatch)
        {
            PlayerSession? callerSession = _sessionRegistry.FindCaller();
            var destinationCell = new MatchCell { Row = row, Column = column };
            PlacementResult placementResult;
            if (callerSession is null)
            {
                placementResult = PlacementResult.NotIdentified;
            }
            else if (!MatchCoordinator.BoardContains(destinationCell))
            {
                placementResult = PlacementResult.OutsideBoard;
            }
            else
            {
                placementResult = placeInMatch(callerSession.Owner.OwnerId, destinationCell);
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
                _matchLog.Error(failureMessage, exception);
                startResult = new StartMatchResult { Status = StartMatchStatus.DatabaseUnavailable };
            }

            return startResult;
        }

        private async Task<StartMatchResult> CheckAndStartMatchAsync(LiveRoom room, int callerOwnerId)
        {
            MatchStartCheck startCheck = room.CheckMatchStart(callerOwnerId);
            MatchBan? firstBan = startCheck.Status == StartMatchStatus.Started ? await FindFirstBanAsync(startCheck.Entrants) : null;
            StartMatchResult startResult;
            if (startCheck.Status != StartMatchStatus.Started)
            {
                startResult = new StartMatchResult { Status = startCheck.Status };
            }
            else if (firstBan is not null)
            {
                startResult = ToBannedResult(firstBan);
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
            IReadOnlyDictionary<int, MatchRuleSet> ruleSetsByPlayerCount = await _matchRuleReader.ReadByPlayerCountAsync();
            MatchStartCheck startCheck = room.StartMatch(callerOwnerId);
            if (startCheck.Status == StartMatchStatus.Started)
            {
                int? noExcludedOwnerId = null;
                _roomService.NotifyMembers(room, RoomChangeReason.MatchStarted, noExcludedOwnerId);
                int seatedCount = startCheck.Entrants.Count(entrant => entrant.IsActive);
                int matchId = await _matchCoordinator.StartMatchAsync(startCheck.Entrants, placementTime, ruleSetsByPlayerCount[seatedCount]);
                _matchLog.InfoFormat(MatchStartedMessage, matchId, room.RoomId, seatedCount);
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
                Sanction? sanction = await _roomService.FindBanInForceAsync(activeEntrant.EntrantId, activeEntrant.IsGuest);
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

            var matchBan = new MatchBan(bannedEntrant.DisplayName, sanction.EffectiveUntil is null, banTimeLeft);

            return matchBan;
        }

        private async Task<TimeSpan> ReadPlacementTimeAsync()
        {
            int turnSeconds = await _systemParameterReader.ReadPositiveAsync(TurnSecondsParameterCode);
            TimeSpan placementTime = TimeSpan.FromSeconds(turnSeconds);

            return placementTime;
        }
    }
}
