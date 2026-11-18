using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using Game.Contracts;
using Game.Domain;
using Game.Persistence;

using log4net;

namespace Game.Match
{
    public sealed class MatchCoordinator
    {
        private const string SeatedEntrantMessage = "A player cannot take two seats at once.";
        private const string StateMissingMessage = "Match {0} was left in progress without a saved state; it is recorded as interrupted.";
        private const string StateUnreadableMessage = "The saved state of match {0} cannot be read; the match is recorded as interrupted.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(MatchCoordinator));

        private readonly Lock _coordinatorLock = new Lock();
        private readonly Dictionary<int, LiveMatch> _matchesByEntrantId = new Dictionary<int, LiveMatch>();
        private readonly Dictionary<int, AwaitingMatch> _awaitingMatchesByEntrantId = new Dictionary<int, AwaitingMatch>();
        private readonly HashSet<int> _reservedEntrantIds = new HashSet<int>();
        private readonly TimeProvider _timeProvider;
        private readonly IMatchListener _matchListener;
        private readonly MatchRepositories _matchRepositories;
        private readonly MatchRecordWriter _matchRecordWriter;
        private readonly StartingLayout _startingLayout;
        private readonly Random _chanceSource;

        public MatchCoordinator(TimeProvider timeProvider, IMatchListener matchListener, MatchRepositories matchRepositories, IReadOnlyList<MatchCell> startingFlowerCells, Random chanceSource)
        {
            ArgumentNullException.ThrowIfNull(timeProvider);
            ArgumentNullException.ThrowIfNull(matchListener);
            ArgumentNullException.ThrowIfNull(matchRepositories);
            ArgumentNullException.ThrowIfNull(matchRepositories.MatchRecordRepository, nameof(matchRepositories));
            ArgumentNullException.ThrowIfNull(matchRepositories.MatchParticipantRepository, nameof(matchRepositories));
            ArgumentNullException.ThrowIfNull(matchRepositories.MatchStateRepository, nameof(matchRepositories));
            ArgumentNullException.ThrowIfNull(startingFlowerCells);
            ArgumentNullException.ThrowIfNull(chanceSource);

            _timeProvider = timeProvider;
            _matchListener = matchListener;
            _matchRepositories = matchRepositories;
            _matchRecordWriter = new MatchRecordWriter(matchRepositories, timeProvider);
            _startingLayout = new StartingLayout(startingFlowerCells.Select(ToStartingFlower).ToList());
            _chanceSource = chanceSource;
        }

        public static int MinimumEntrantCount => MatchTable.MinimumSeatCount;

        public static int MaximumEntrantCount => MatchTable.MaximumSeatCount;

        public static int RoundCount => MatchRules.RoundCount;

        public static bool BoardContains(MatchCell examinedCell)
        {
            ArgumentNullException.ThrowIfNull(examinedCell);

            bool isOnBoard = Board.Contains(MatchCellConversion.ToBoardCell(examinedCell));

            return isOnBoard;
        }

        public Task<int> StartMatchAsync(MatchOpening matchOpening, MatchTimeLimits timeLimits, MatchRuleSet ruleSet)
        {
            ArgumentNullException.ThrowIfNull(matchOpening);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(matchOpening.RoomId, nameof(matchOpening));
            ArgumentNullException.ThrowIfNull(matchOpening.RoomCode, nameof(matchOpening));
            ArgumentNullException.ThrowIfNull(matchOpening.Entrants, nameof(matchOpening));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeLimits.StepTime, TimeSpan.Zero, nameof(timeLimits));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeLimits.ReconnectionTime, TimeSpan.Zero, nameof(timeLimits));
            ArgumentNullException.ThrowIfNull(ruleSet);

            Task<int> matchStart = StartRecordedMatchAsync(matchOpening, timeLimits, ruleSet);

            return matchStart;
        }

        public async Task<IReadOnlyList<int>> RestoreInterruptedMatchesAsync(MatchResumption resumption)
        {
            ArgumentNullException.ThrowIfNull(resumption);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(resumption.TimeLimits.StepTime, TimeSpan.Zero, nameof(resumption));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(resumption.TimeLimits.ReconnectionTime, TimeSpan.Zero, nameof(resumption));
            ArgumentNullException.ThrowIfNull(resumption.RuleSetsBySeatCount, nameof(resumption));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(resumption.ResumeTime, TimeSpan.Zero, nameof(resumption));

            List<int> inProgressMatchIds = await _matchRepositories.MatchRecordRepository.ListInProgressIdsAsync();
            List<MatchState> savedStates = await _matchRepositories.MatchStateRepository.ListAsync();
            var waitingEntrantIds = new List<int>();
            foreach (int inProgressMatchId in inProgressMatchIds)
            {
                MatchState? savedState = savedStates.Find(storedState => storedState.MatchId == inProgressMatchId);
                AwaitingMatch? awaitingMatch = savedState is null ? null : RestoreAwaitingMatch(savedState, resumption);
                if (savedState is null)
                {
                    _log.WarnFormat(StateMissingMessage, inProgressMatchId);
                }

                if (awaitingMatch is null)
                {
                    _matchRecordWriter.RecordFinish(inProgressMatchId, MatchEndReasonCodes.Interrupted, Array.Empty<MatchParticipant>());
                }
                else
                {
                    waitingEntrantIds.AddRange(OpenAwaitingMatch(awaitingMatch, resumption.ResumeTime));
                }
            }

            return waitingEntrantIds;
        }

        public MatchSeating? FindSeating(int entrantId)
        {
            AwaitingMatch? awaitingMatch = FindAwaitingMatchOfEntrant(entrantId);
            LiveMatch? liveMatch = FindMatchOfEntrant(entrantId);
            MatchSeating? seating = null;
            if (awaitingMatch is not null)
            {
                bool holdsSeat = awaitingMatch.HoldsSeatOf(entrantId);
                seating = holdsSeat ? new MatchSeating { RoomCode = awaitingMatch.SeatedMatch.RoomCode, IsAwaitingPlayers = true } : null;
            }
            else if (liveMatch is not null)
            {
                seating = new MatchSeating { RoomCode = liveMatch.RoomCode, IsAwaitingPlayers = false };
            }

            return seating;
        }

        public ReturnToMatchResult ReturnToMatch(int entrantId)
        {
            AwaitingMatch? awaitingMatch = FindAwaitingMatchOfEntrant(entrantId);
            bool isBackInMatch = (awaitingMatch is not null) && awaitingMatch.ReturnSeat(entrantId);
            if (!isBackInMatch)
            {
                isBackInMatch = CloseReconnectionWindow(entrantId);
            }

            ReturnToMatchResult returnResult = isBackInMatch ? ReturnToMatchResult.Returned : ReturnToMatchResult.NoMatchToResume;

            return returnResult;
        }

        public bool OpenReconnectionWindow(int entrantId)
        {
            FindAwaitingMatchOfEntrant(entrantId)?.ReleaseSeat(entrantId);
            LiveMatch? liveMatch = FindMatchOfEntrant(entrantId);
            bool hasOpenedWindow = (liveMatch is not null) && liveMatch.OpenReconnectionWindow(entrantId);

            return hasOpenedWindow;
        }

        public bool CloseReconnectionWindow(int entrantId)
        {
            LiveMatch? liveMatch = FindMatchOfEntrant(entrantId);
            bool isBackInMatch = (liveMatch is not null) && liveMatch.CloseReconnectionWindow(entrantId);

            return isBackInMatch;
        }

        public MatchActionResult PlacePiece(int entrantId, MatchPiece piece, MatchCell destinationCell)
        {
            ThrowIfOffBoard(destinationCell);

            LiveMatch? liveMatch = FindMatchOfEntrant(entrantId);
            MatchActionResult actionResult = liveMatch is null ? MatchActionResult.NotInMatch : liveMatch.PlacePiece(entrantId, piece, MatchCellConversion.ToBoardCell(destinationCell));

            return actionResult;
        }

        public MatchActionResult MoveCaterpillar(int entrantId, MatchMove move)
        {
            ArgumentNullException.ThrowIfNull(move);
            ThrowIfOffBoard(move.Origin);
            ThrowIfOffBoard(move.Destination);

            LiveMatch? liveMatch = FindMatchOfEntrant(entrantId);
            MatchActionResult actionResult = liveMatch is null ? MatchActionResult.NotInMatch : liveMatch.MoveCaterpillar(entrantId, MatchCellConversion.ToCaterpillarMove(move));

            return actionResult;
        }

        public MatchActionResult AddFlowerLevel(int entrantId, MatchCell buildCell)
        {
            ThrowIfOffBoard(buildCell);

            LiveMatch? liveMatch = FindMatchOfEntrant(entrantId);
            MatchActionResult actionResult = liveMatch is null ? MatchActionResult.NotInMatch : liveMatch.AddFlowerLevel(entrantId, MatchCellConversion.ToBoardCell(buildCell));

            return actionResult;
        }

        public MatchActionResult DrawCard(int entrantId)
        {
            LiveMatch? liveMatch = FindMatchOfEntrant(entrantId);
            MatchActionResult actionResult = liveMatch is null ? MatchActionResult.NotInMatch : liveMatch.DrawCard(entrantId);

            return actionResult;
        }

        public MatchActionResult PlayCard(int entrantId, MatchCardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay);
            ArgumentNullException.ThrowIfNull(cardPlay.AffectedCells);
            foreach (MatchCell playedCell in cardPlay.AffectedCells)
            {
                ThrowIfOffBoard(playedCell);
            }

            LiveMatch? liveMatch = FindMatchOfEntrant(entrantId);
            MatchActionResult actionResult = liveMatch is null ? MatchActionResult.NotInMatch : liveMatch.PlayCard(entrantId, MatchCardConversion.ToCardPlay(cardPlay));

            return actionResult;
        }

        public MatchActionResult EndTurn(int entrantId)
        {
            LiveMatch? liveMatch = FindMatchOfEntrant(entrantId);
            MatchActionResult actionResult = liveMatch is null ? MatchActionResult.NotInMatch : liveMatch.EndTurn(entrantId);

            return actionResult;
        }

        public ForfeitResult Forfeit(int entrantId)
        {
            AwaitingMatch? awaitingMatch = FindAwaitingMatchOfEntrant(entrantId);
            LiveMatch? liveMatch = FindMatchOfEntrant(entrantId);
            bool hasForfeited = (awaitingMatch is not null) ? awaitingMatch.Forfeit(entrantId) : (liveMatch is not null) && liveMatch.Forfeit(entrantId);
            ForfeitResult forfeitResult = hasForfeited ? ForfeitResult.Forfeited : ForfeitResult.NotInMatch;

            return forfeitResult;
        }

        public Task FinishRecordingAsync()
        {
            Task pendingRecording = _matchRecordWriter.FinishWritingAsync();

            return pendingRecording;
        }

        private static void ThrowIfOffBoard(MatchCell examinedCell)
        {
            ArgumentNullException.ThrowIfNull(examinedCell);
            ArgumentOutOfRangeException.ThrowIfNegative(examinedCell.Row, nameof(examinedCell));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(examinedCell.Row, Board.CellsPerSide, nameof(examinedCell));
            ArgumentOutOfRangeException.ThrowIfNegative(examinedCell.Column, nameof(examinedCell));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(examinedCell.Column, Board.CellsPerSide, nameof(examinedCell));
        }

        private static List<SeatedEntrant> SeatActiveEntrants(IReadOnlyList<MatchEntrant> entrants)
        {
            List<SeatedEntrant> seatedEntrants = entrants.Where(entrant => entrant.IsActive).OrderBy(entrant => entrant.MemberId).Select(ToSeatedEntrant).ToList();

            return seatedEntrants;
        }

        private static SeatedEntrant ToSeatedEntrant(MatchEntrant entrant, int arrivalIndex)
        {
            var seatedEntrant = new SeatedEntrant(MatchTable.FirstSeatNumber + arrivalIndex, entrant);

            return seatedEntrant;
        }

        private static Flower ToStartingFlower(MatchCell flowerCell)
        {
            var flower = new Flower(MatchCellConversion.ToBoardCell(flowerCell), StartingLayout.StartingFlowerLevel);

            return flower;
        }

        private async Task<int> StartRecordedMatchAsync(MatchOpening matchOpening, MatchTimeLimits timeLimits, MatchRuleSet ruleSet)
        {
            MatchRules matchRules = MatchRuleConversion.ToMatchRules(ruleSet);
            List<SeatedEntrant> seatedEntrants = SeatActiveEntrants(matchOpening.Entrants);
            List<int> entrantIds = seatedEntrants.Select(seatedEntrant => seatedEntrant.Entrant.EntrantId).ToList();
            var matchTable = new MatchTable(_startingLayout, seatedEntrants.Count, matchRules, _chanceSource);
            ReserveSeats(entrantIds);
            int matchId;
            try
            {
                matchId = await _matchRepositories.MatchRecordRepository.CreateAsync(matchOpening.RoomId, seatedEntrants.Count);
                await _matchRepositories.MatchParticipantRepository.AddAsync(matchId, MatchRecordConversion.ToSeatedPlayers(seatedEntrants));
                var liveMatch = new LiveMatch(new SeatedMatch(matchId, matchOpening.RoomCode, seatedEntrants), matchTable, _matchRecordWriter, _timeProvider, _matchListener, timeLimits);
                OpenMatch(liveMatch, entrantIds);
            }
            finally
            {
                ReleaseSeats(entrantIds);
            }

            return matchId;
        }

        private void ReserveSeats(List<int> entrantIds)
        {
            lock (_coordinatorLock)
            {
                bool isAnyEntrantSeated = (entrantIds.Distinct().Count() != entrantIds.Count) || entrantIds.Any(_matchesByEntrantId.ContainsKey) || entrantIds.Any(_awaitingMatchesByEntrantId.ContainsKey) || entrantIds.Any(_reservedEntrantIds.Contains);
                if (isAnyEntrantSeated)
                {
                    throw new InvalidOperationException(SeatedEntrantMessage);
                }

                _reservedEntrantIds.UnionWith(entrantIds);
            }
        }

        private void ReleaseSeats(List<int> entrantIds)
        {
            lock (_coordinatorLock)
            {
                _reservedEntrantIds.ExceptWith(entrantIds);
            }
        }

        private void OpenMatch(LiveMatch liveMatch, List<int> entrantIds)
        {
            lock (_coordinatorLock)
            {
                foreach (int entrantId in entrantIds)
                {
                    _matchesByEntrantId.Add(entrantId, liveMatch);
                }

                liveMatch.Finished += LiveMatchOnFinished;
                liveMatch.StartPlacement();
            }
        }

        private AwaitingMatch? RestoreAwaitingMatch(MatchState savedState, MatchResumption resumption)
        {
            AwaitingMatch? awaitingMatch = null;
            try
            {
                MatchCheckpoint checkpoint = MatchCheckpointDocument.Read(savedState.StateDocument);
                awaitingMatch = CreateAwaitingMatch(savedState.MatchId, checkpoint, resumption);
            }
            catch (SerializationException exception)
            {
                _log.Warn(string.Format(CultureInfo.InvariantCulture, StateUnreadableMessage, savedState.MatchId), exception);
            }
            catch (XmlException exception)
            {
                _log.Warn(string.Format(CultureInfo.InvariantCulture, StateUnreadableMessage, savedState.MatchId), exception);
            }
            catch (ArgumentException exception)
            {
                _log.Warn(string.Format(CultureInfo.InvariantCulture, StateUnreadableMessage, savedState.MatchId), exception);
            }

            return awaitingMatch;
        }

        private AwaitingMatch CreateAwaitingMatch(int matchId, MatchCheckpoint checkpoint, MatchResumption resumption)
        {
            List<SeatedEntrant> seatedEntrants = MatchCheckpointConversion.ToSeatedEntrants(checkpoint);
            if (!resumption.RuleSetsBySeatCount.TryGetValue(seatedEntrants.Count, out MatchRuleSet? ruleSet))
            {
                throw new ArgumentException(StateUnreadableMessage, nameof(checkpoint));
            }

            MatchTable matchTable = MatchTable.RestoreFromSnapshot(MatchCheckpointConversion.ToSnapshot(checkpoint), MatchRuleConversion.ToMatchRules(ruleSet), _chanceSource);
            foreach (int retiredSeatNumber in MatchCheckpointConversion.FindRetiredSeatNumbers(checkpoint).Where(seatNumber => !matchTable.IsSeatRetired(seatNumber)))
            {
                matchTable.RetireSeat(retiredSeatNumber);
            }

            if (matchTable.Phase == TablePhase.Finished)
            {
                throw new ArgumentException(StateUnreadableMessage, nameof(checkpoint));
            }

            var seatedMatch = new SeatedMatch(matchId, checkpoint.RoomCode ?? string.Empty, seatedEntrants);
            var awaitingMatch = new AwaitingMatch(seatedMatch, matchTable, _matchRecordWriter, _timeProvider, _matchListener, resumption.TimeLimits);

            return awaitingMatch;
        }

        private List<int> OpenAwaitingMatch(AwaitingMatch awaitingMatch, TimeSpan resumeTime)
        {
            List<int> entrantIds = awaitingMatch.SeatedMatch.SeatedEntrants.Select(seatedEntrant => seatedEntrant.Entrant.EntrantId).ToList();
            lock (_coordinatorLock)
            {
                foreach (int entrantId in entrantIds)
                {
                    _awaitingMatchesByEntrantId.Add(entrantId, awaitingMatch);
                }

                awaitingMatch.Resumed += AwaitingMatchOnResumed;
                awaitingMatch.Finished += AwaitingMatchOnFinished;
                awaitingMatch.StartWaiting(resumeTime);
            }

            return entrantIds;
        }

        private void AwaitingMatchOnResumed(object? sender, EventArgs e)
        {
            if (sender is not AwaitingMatch resumedMatch)
            {
                return;
            }

            UnsubscribeAwaitingMatch(resumedMatch);
            var liveMatch = new LiveMatch(resumedMatch.SeatedMatch, resumedMatch.Table, _matchRecordWriter, _timeProvider, _matchListener, resumedMatch.TimeLimits);
            lock (_coordinatorLock)
            {
                List<int> entrantIds = RemoveAwaitingEntrantsOf(resumedMatch);
                foreach (int entrantId in entrantIds)
                {
                    _matchesByEntrantId.Add(entrantId, liveMatch);
                }

                liveMatch.Finished += LiveMatchOnFinished;
                liveMatch.StartResumed(resumedMatch.ReadAbsentSeatNumbers());
            }
        }

        private void AwaitingMatchOnFinished(object? sender, EventArgs e)
        {
            if (sender is not AwaitingMatch finishedMatch)
            {
                return;
            }

            UnsubscribeAwaitingMatch(finishedMatch);
            List<int> releasedEntrantIds;
            lock (_coordinatorLock)
            {
                releasedEntrantIds = RemoveAwaitingEntrantsOf(finishedMatch);
            }

            _matchRecordWriter.RecordFinish(finishedMatch.SeatedMatch.MatchId, finishedMatch.ReadEndReason(), finishedMatch.ReadPlayerResults());
            IReadOnlyList<EntrantStanding> unplayedStandings = Array.Empty<EntrantStanding>();
            _matchListener.ReceiveMatchFinished(releasedEntrantIds, unplayedStandings);
        }

        private void UnsubscribeAwaitingMatch(AwaitingMatch awaitingMatch)
        {
            awaitingMatch.Resumed -= AwaitingMatchOnResumed;
            awaitingMatch.Finished -= AwaitingMatchOnFinished;
        }

        private List<int> RemoveAwaitingEntrantsOf(AwaitingMatch awaitingMatch)
        {
            List<int> awaitingEntrantIds = _awaitingMatchesByEntrantId.Where(matchByEntrant => matchByEntrant.Value == awaitingMatch).Select(matchByEntrant => matchByEntrant.Key).ToList();
            foreach (int awaitingEntrantId in awaitingEntrantIds)
            {
                _awaitingMatchesByEntrantId.Remove(awaitingEntrantId);
            }

            return awaitingEntrantIds;
        }

        private AwaitingMatch? FindAwaitingMatchOfEntrant(int entrantId)
        {
            AwaitingMatch? awaitingMatch;
            lock (_coordinatorLock)
            {
                awaitingMatch = _awaitingMatchesByEntrantId.GetValueOrDefault(entrantId);
            }

            return awaitingMatch;
        }

        private void LiveMatchOnFinished(object? sender, EventArgs e)
        {
            if (sender is LiveMatch finishedMatch)
            {
                finishedMatch.Finished -= LiveMatchOnFinished;
                List<int> releasedEntrantIds;
                lock (_coordinatorLock)
                {
                    releasedEntrantIds = RemoveEntrantsOf(finishedMatch);
                }

                if (releasedEntrantIds.Count > 0)
                {
                    _matchRecordWriter.RecordFinish(finishedMatch.MatchId, finishedMatch.ReadEndReason(), finishedMatch.ReadPlayerResults());
                }

                IReadOnlyList<EntrantStanding> entrantStandings = finishedMatch.ReadStandings();
                _matchListener.ReceiveMatchFinished(releasedEntrantIds, entrantStandings);
            }
        }

        private List<int> RemoveEntrantsOf(LiveMatch liveMatch)
        {
            List<int> seatedEntrantIds = _matchesByEntrantId.Where(matchByEntrant => matchByEntrant.Value == liveMatch).Select(matchByEntrant => matchByEntrant.Key).ToList();
            foreach (int seatedEntrantId in seatedEntrantIds)
            {
                _matchesByEntrantId.Remove(seatedEntrantId);
            }

            return seatedEntrantIds;
        }

        private LiveMatch? FindMatchOfEntrant(int entrantId)
        {
            LiveMatch? liveMatch;
            lock (_coordinatorLock)
            {
                liveMatch = _matchesByEntrantId.GetValueOrDefault(entrantId);
            }

            return liveMatch;
        }
    }
}
