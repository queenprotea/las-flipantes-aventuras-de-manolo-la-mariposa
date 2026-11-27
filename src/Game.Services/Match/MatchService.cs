using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Match;
using Game.Persistence;

using log4net;

using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace Game.Services
{
    public sealed class MatchService : IMatchService
    {
        private const string TurnSecondsParameterCode = "turn_seconds";
        private const string ReconnectionSecondsParameterCode = "reconnection_seconds";
        private const string ResumeMinutesParameterCode = "match_resume_minutes";
        private const string AbsentStepsParameterCode = "absent_after_expired_steps";
        private const double ReconnectionTokenMargin = 1.25;
        private const string MatchesRestoredMessage = "{0} players wait for the matches left in progress by the previous run.";
        private const string SeatReturnedMessage = "Player {0} came back to the match interrupted by the server restart.";
        private const string SeatKeptMessage = "Player {0} lost the connection during a match; the seat waits for the reconnection window.";
        private const string SeatTakenBackMessage = "Player {0} came back to the match before the reconnection window ran out.";
        private const string MatchStartedMessage = "Match {0} started in room {1} with {2} players.";
        private const string MatchForfeitedMessage = "Player {0} forfeited the match.";
        private const string StartFailedMessage = "The database could not be used while a match was starting in room {0}.";
        private const string RulesNotLoadedMessage = "The match rules were not loaded; LoadRulesAsync must run before a match starts.";

        private static readonly ILog _matchLog = LogManager.GetLogger(typeof(MatchService));
        private static readonly IReadOnlyList<EntrantStanding> _unplayedStandings = Array.Empty<EntrantStanding>();

        private readonly RoomRegistry _roomRegistry;
        private readonly RoomService _roomService;
        private readonly SessionRegistry _sessionRegistry;
        private readonly SystemParameterReader _systemParameterReader;
        private readonly MatchRuleReader _matchRuleReader;
        private readonly TimeProvider _timeProvider;
        private readonly GuestIdIssuer _guestIdIssuer;
        private readonly MatchCoordinator _matchCoordinator;
        private readonly MatchReconnectionTokenIssuer _reconnectionTokenIssuer;

        private MatchTimeLimits? _timeLimits;
        private TimeSpan _resumeTime;
        private IReadOnlyDictionary<int, MatchRuleSet>? _ruleSetsByPlayerCount;

        public MatchService(RoomRegistry roomRegistry, RoomService roomService, SessionRegistry sessionRegistry, SystemParameterReader systemParameterReader, MatchRuleReader matchRuleReader, IMatchRecordRepository matchRecordRepository, IMatchParticipantRepository matchParticipantRepository, IMatchStateRepository matchStateRepository, TimeProvider timeProvider, StartingBoard startingBoard, GuestIdIssuer guestIdIssuer, SessionTokenIssuer sessionTokenIssuer, Random chanceSource)
        {
            ArgumentNullException.ThrowIfNull(roomRegistry);
            ArgumentNullException.ThrowIfNull(roomService);
            ArgumentNullException.ThrowIfNull(sessionRegistry);
            ArgumentNullException.ThrowIfNull(systemParameterReader);
            ArgumentNullException.ThrowIfNull(matchRuleReader);
            ArgumentNullException.ThrowIfNull(matchRecordRepository);
            ArgumentNullException.ThrowIfNull(matchParticipantRepository);
            ArgumentNullException.ThrowIfNull(matchStateRepository);
            ArgumentNullException.ThrowIfNull(timeProvider);
            ArgumentNullException.ThrowIfNull(startingBoard);
            ArgumentNullException.ThrowIfNull(guestIdIssuer);
            ArgumentNullException.ThrowIfNull(sessionTokenIssuer);
            ArgumentNullException.ThrowIfNull(chanceSource);

            _roomRegistry = roomRegistry;
            _roomService = roomService;
            _sessionRegistry = sessionRegistry;
            _systemParameterReader = systemParameterReader;
            _matchRuleReader = matchRuleReader;
            _timeProvider = timeProvider;
            _guestIdIssuer = guestIdIssuer;
            var matchRoomReturn = new MatchRoomReturn(roomRegistry, roomService);
            var matchRepositories = new MatchRepositories(matchRecordRepository, matchParticipantRepository, matchStateRepository);
            _reconnectionTokenIssuer = new MatchReconnectionTokenIssuer(sessionTokenIssuer, sessionRegistry);
            _matchCoordinator = new MatchCoordinator(timeProvider, new MatchNoticeForwarder(sessionRegistry, matchRoomReturn, _reconnectionTokenIssuer), matchRepositories, startingBoard.FlowerCells, chanceSource);
            _sessionRegistry.SessionLost += SessionRegistryOnSessionLost;
            _sessionRegistry.SessionClosed += SessionRegistryOnSessionClosed;
            _sessionRegistry.SessionOpened += SessionRegistryOnSessionOpened;
        }

        public async Task LoadRulesAsync()
        {
            TimeSpan stepTime = await ReadSecondsAsync(TurnSecondsParameterCode);
            TimeSpan reconnectionTime = await ReadSecondsAsync(ReconnectionSecondsParameterCode);
            int absentAfterExpiredSteps = await _systemParameterReader.ReadPositiveAsync(AbsentStepsParameterCode);
            int resumeMinutes = await _systemParameterReader.ReadPositiveAsync(ResumeMinutesParameterCode);
            _timeLimits = new MatchTimeLimits(stepTime, reconnectionTime, absentAfterExpiredSteps);
            _resumeTime = TimeSpan.FromMinutes(resumeMinutes);
            _reconnectionTokenIssuer.UseLifetime((stepTime + reconnectionTime + _resumeTime) * ReconnectionTokenMargin);
            _ruleSetsByPlayerCount = await _matchRuleReader.ReadByPlayerCountAsync();
        }

        public async Task ResumeInterruptedMatchesAsync()
        {
            MatchTimeLimits timeLimits = _timeLimits ?? throw new InvalidOperationException(RulesNotLoadedMessage);
            IReadOnlyDictionary<int, MatchRuleSet> ruleSetsByPlayerCount = _ruleSetsByPlayerCount ?? throw new InvalidOperationException(RulesNotLoadedMessage);
            var resumption = new MatchResumption(timeLimits, ruleSetsByPlayerCount, _resumeTime);
            IReadOnlyList<int> waitingEntrantIds = await _matchCoordinator.RestoreInterruptedMatchesAsync(resumption);
            await _roomService.RestoreRoomsAsync(_matchCoordinator.ListInterruptedRooms());
            foreach (int waitingGuestId in waitingEntrantIds.Where(entrantId => entrantId < 0))
            {
                _guestIdIssuer.SkipPast(waitingGuestId);
            }

            _matchLog.InfoFormat(MatchesRestoredMessage, waitingEntrantIds.Count);
        }

        internal MatchSeating? FindSeating(int ownerId)
        {
            MatchSeating? seating = _matchCoordinator.FindSeating(ownerId);

            return seating;
        }

        internal bool HoldsSeatIn(int ownerId, int matchId)
        {
            bool holdsSeat = _matchCoordinator.HoldsSeatIn(ownerId, matchId);

            return holdsSeat;
        }

        public Task FinishRecordingAsync()
        {
            Task pendingRecording = _matchCoordinator.FinishRecordingAsync();

            return pendingRecording;
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
        public MatchActionResult PlacePiece(MatchPiece piece, MatchCell destinationCell)
        {
            int? callerOwnerId = FindCallerOwnerId();
            MatchActionResult actionResult = CheckCallArguments(callerOwnerId, destinationCell);
            if ((actionResult == MatchActionResult.Applied) && !Enum.IsDefined(piece))
            {
                actionResult = MatchActionResult.WrongPhase;
            }

            if ((actionResult == MatchActionResult.Applied) && (callerOwnerId is int ownerId))
            {
                actionResult = _matchCoordinator.PlacePiece(ownerId, piece, destinationCell);
            }

            return actionResult;
        }

        /// <inheritdoc/>
        public MatchActionResult MoveCaterpillar(MatchMove move)
        {
            int? callerOwnerId = FindCallerOwnerId();
            MatchActionResult actionResult = CheckCallArguments(callerOwnerId, move?.Origin, move?.Destination);
            if ((actionResult == MatchActionResult.Applied) && (callerOwnerId is int ownerId) && (move is not null))
            {
                actionResult = _matchCoordinator.MoveCaterpillar(ownerId, move);
            }

            return actionResult;
        }

        /// <inheritdoc/>
        public MatchActionResult AddFlowerLevel(MatchCell buildCell)
        {
            int? callerOwnerId = FindCallerOwnerId();
            MatchActionResult actionResult = CheckCallArguments(callerOwnerId, buildCell);
            if ((actionResult == MatchActionResult.Applied) && (callerOwnerId is int ownerId))
            {
                actionResult = _matchCoordinator.AddFlowerLevel(ownerId, buildCell);
            }

            return actionResult;
        }

        /// <inheritdoc/>
        public MatchActionResult DrawCard()
        {
            int? callerOwnerId = FindCallerOwnerId();
            MatchActionResult actionResult = CheckCallArguments(callerOwnerId);
            if ((actionResult == MatchActionResult.Applied) && (callerOwnerId is int ownerId))
            {
                actionResult = _matchCoordinator.DrawCard(ownerId);
            }

            return actionResult;
        }

        /// <inheritdoc/>
        public MatchActionResult PlayCard(MatchCardPlay cardPlay)
        {
            int? callerOwnerId = FindCallerOwnerId();
            MatchActionResult actionResult = CheckCardPlayArguments(callerOwnerId, cardPlay);
            if ((actionResult == MatchActionResult.Applied) && (callerOwnerId is int ownerId))
            {
                actionResult = _matchCoordinator.PlayCard(ownerId, cardPlay);
            }

            return actionResult;
        }

        /// <inheritdoc/>
        public MatchActionResult EndTurn()
        {
            int? callerOwnerId = FindCallerOwnerId();
            MatchActionResult actionResult = CheckCallArguments(callerOwnerId);
            if ((actionResult == MatchActionResult.Applied) && (callerOwnerId is int ownerId))
            {
                actionResult = _matchCoordinator.EndTurn(ownerId);
            }

            return actionResult;
        }

        /// <inheritdoc/>
        public ForfeitResult Forfeit()
        {
            PlayerSession? callerSession = _sessionRegistry.FindCaller();
            ForfeitResult forfeitResult = callerSession is null ? ForfeitResult.NotIdentified : ForfeitFor(callerSession.Owner.OwnerId);

            return forfeitResult;
        }

        /// <inheritdoc/>
        public ReturnToMatchResult ReturnToMatch()
        {
            PlayerSession? callerSession = _sessionRegistry.FindCaller();
            ReturnToMatchResult returnResult = callerSession is null ? ReturnToMatchResult.NotIdentified : ReturnToMatchFor(callerSession.Owner.OwnerId);

            return returnResult;
        }

        private ReturnToMatchResult ReturnToMatchFor(int ownerId)
        {
            ReturnToMatchResult returnResult = _matchCoordinator.ReturnToMatch(ownerId);
            if (returnResult == ReturnToMatchResult.Returned)
            {
                _matchLog.InfoFormat(SeatReturnedMessage, ownerId);
            }

            return returnResult;
        }

        private static StartMatchResult ToBannedResult(MatchBan ban)
        {
            var bannedResult = new StartMatchResult { Status = StartMatchStatus.PlayerBanned, BannedDisplayName = ban.DisplayName, IsBanPermanent = ban.IsPermanent, BanTimeLeft = ban.BanTimeLeft };

            return bannedResult;
        }

        private ForfeitResult ForfeitFor(int ownerId)
        {
            ForfeitResult forfeitResult = _matchCoordinator.Forfeit(ownerId);
            if (forfeitResult == ForfeitResult.Forfeited)
            {
                _matchLog.InfoFormat(MatchForfeitedMessage, ownerId);
            }

            return forfeitResult;
        }

        private static MatchActionResult CheckCallArguments(int? callerOwnerId, params MatchCell?[] examinedCells)
        {
            MatchActionResult actionResult = MatchActionResult.Applied;
            if (callerOwnerId is null)
            {
                actionResult = MatchActionResult.NotIdentified;
            }
            else if (!examinedCells.All(IsOnBoard))
            {
                actionResult = MatchActionResult.OutsideBoard;
            }

            return actionResult;
        }

        private static MatchActionResult CheckCardPlayArguments(int? callerOwnerId, MatchCardPlay? cardPlay)
        {
            MatchActionResult actionResult;
            if ((cardPlay is null) || (cardPlay.AffectedCells is null) || !Enum.IsDefined(cardPlay.Card))
            {
                actionResult = callerOwnerId is null ? MatchActionResult.NotIdentified : MatchActionResult.WrongCardTarget;
            }
            else
            {
                actionResult = CheckCallArguments(callerOwnerId, cardPlay.AffectedCells.ToArray());
            }

            return actionResult;
        }

        private static bool IsOnBoard(MatchCell? examinedCell)
        {
            bool isOnBoard = (examinedCell is not null) && MatchCoordinator.BoardContains(examinedCell);

            return isOnBoard;
        }

        private int? FindCallerOwnerId()
        {
            PlayerSession? callerSession = _sessionRegistry.FindCaller();
            int? callerOwnerId = callerSession?.Owner.OwnerId;

            return callerOwnerId;
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
                startResult = LogStartFailure(room, exception);
            }
            catch (DbUpdateException exception)
            {
                startResult = LogStartFailure(room, exception);
            }

            return startResult;
        }

        private static StartMatchResult LogStartFailure(LiveRoom room, Exception exception)
        {
            string failureMessage = string.Format(CultureInfo.InvariantCulture, StartFailedMessage, room.RoomId);
            _matchLog.Error(failureMessage, exception);
            var failedResult = new StartMatchResult { Status = StartMatchStatus.DatabaseUnavailable };

            return failedResult;
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
            IReadOnlyDictionary<int, MatchRuleSet> ruleSetsByPlayerCount = _ruleSetsByPlayerCount ?? throw new InvalidOperationException(RulesNotLoadedMessage);
            MatchStartCheck startCheck = room.StartMatch(callerOwnerId);
            if (startCheck.Status == StartMatchStatus.Started)
            {
                int seatedCount = startCheck.Entrants.Count(entrant => entrant.IsActive);
                int matchId = await StartLiveMatchAsync(room, startCheck.Entrants, ruleSetsByPlayerCount[seatedCount]);
                int? noExcludedOwnerId = null;
                _roomService.NotifyMembers(room, RoomChangeReason.MatchStarted, noExcludedOwnerId);
                _matchLog.InfoFormat(MatchStartedMessage, matchId, room.RoomId, seatedCount);
            }

            var startResult = new StartMatchResult { Status = startCheck.Status };

            return startResult;
        }

        private async Task<int> StartLiveMatchAsync(LiveRoom room, IReadOnlyList<MatchEntrant> entrants, MatchRuleSet ruleSet)
        {
            int matchId;
            try
            {
                MatchTimeLimits timeLimits = _timeLimits ?? throw new InvalidOperationException(RulesNotLoadedMessage);
                matchId = await _matchCoordinator.StartMatchAsync(new MatchOpening(room.RoomId, room.Code, entrants), timeLimits, ruleSet);
            }
            catch (NpgsqlException)
            {
                room.EndMatch(_unplayedStandings);
                throw;
            }
            catch (DbUpdateException)
            {
                room.EndMatch(_unplayedStandings);
                throw;
            }

            return matchId;
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

        private async Task<TimeSpan> ReadSecondsAsync(string parameterCode)
        {
            int parameterSeconds = await _systemParameterReader.ReadPositiveAsync(parameterCode);
            TimeSpan parameterTime = TimeSpan.FromSeconds(parameterSeconds);

            return parameterTime;
        }

        private void SessionRegistryOnSessionLost(object? sender, SessionOwnerEventArgs e)
        {
            KeepSeatOf(e.OwnerId);
        }

        private void SessionRegistryOnSessionClosed(object? sender, SessionOwnerEventArgs e)
        {
            KeepSeatOf(e.OwnerId);
        }

        private void SessionRegistryOnSessionOpened(object? sender, SessionOwnerEventArgs e)
        {
            TakeSeatBack(e.OwnerId);
        }

        private void KeepSeatOf(int ownerId)
        {
            if (_matchCoordinator.OpenReconnectionWindow(ownerId))
            {
                _matchLog.InfoFormat(SeatKeptMessage, ownerId);
            }

            if (_sessionRegistry.HasOpenSession(ownerId))
            {
                TakeSeatBack(ownerId);
            }
        }

        private void TakeSeatBack(int ownerId)
        {
            if (_matchCoordinator.CloseReconnectionWindow(ownerId))
            {
                _matchLog.InfoFormat(SeatTakenBackMessage, ownerId);
            }
        }
    }
}
