using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Game.Contracts;
using Game.Domain;
using Game.Persistence;

using log4net;

namespace Game.Match
{
    internal sealed class LiveMatch
    {
        private const string SeatRetiredMessage = "Seat {0} of match {1} was retired: its reconnection window ran out.";
        private const string SeatForfeitedMessage = "Seat {0} of match {1} forfeited.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(LiveMatch));

        private readonly Lock _matchLock = new Lock();
        private readonly Dictionary<int, CancellationTokenSource> _reconnectionCountdownsBySeat = new Dictionary<int, CancellationTokenSource>();
        private readonly Dictionary<int, int> _expiredStepCountsBySeat = new Dictionary<int, int>();
        private readonly MatchTable _matchTable;
        private readonly IReadOnlyList<SeatedEntrant> _seatedEntrants;
        private readonly MatchViewFactory _viewFactory;
        private readonly TimeProvider _timeProvider;
        private readonly IMatchListener _matchListener;
        private readonly MatchRecordWriter _recordWriter;
        private readonly TimeSpan _stepDuration;
        private readonly TimeSpan _reconnectionDuration;
        private readonly int _absentAfterExpiredSteps;

        private CancellationTokenSource? _stepCountdown;
        private DateTime? _stepEndsAtUtc;
        private MatchSnapshot? _stepStartSnapshot;
        private bool _hasReportedFinish;

        internal LiveMatch(SeatedMatch seatedMatch, MatchTable matchTable, MatchRecordWriter recordWriter, TimeProvider timeProvider, IMatchListener matchListener, MatchTimeLimits timeLimits)
        {
            MatchId = seatedMatch.MatchId;
            RoomCode = seatedMatch.RoomCode;
            _matchTable = matchTable;
            _seatedEntrants = seatedMatch.SeatedEntrants;
            _viewFactory = new MatchViewFactory(seatedMatch, _reconnectionCountdownsBySeat.Keys, timeLimits.ReconnectionTime);
            _recordWriter = recordWriter;
            _timeProvider = timeProvider;
            _matchListener = matchListener;
            _stepDuration = timeLimits.StepTime;
            _reconnectionDuration = timeLimits.ReconnectionTime;
            _absentAfterExpiredSteps = timeLimits.AbsentAfterExpiredSteps;
        }

        internal event EventHandler? Finished;

        internal int MatchId { get; }

        internal string RoomCode { get; }

        internal bool HoldsSeatOf(int entrantId)
        {
            bool holdsSeat;
            lock (_matchLock)
            {
                holdsSeat = IsSeatInPlay(GetSeatNumber(entrantId));
            }

            return holdsSeat;
        }

        internal void StartPlacement()
        {
            lock (_matchLock)
            {
                RestartStepCountdown();
                _matchListener.ReceiveMatchStarted(BuildRecipientViews());
            }
        }

        internal void StartResumed(IReadOnlyCollection<int> absentSeatNumbers)
        {
            lock (_matchLock)
            {
                RestartStepCountdown();
                foreach (int absentSeatNumber in absentSeatNumbers.Where(IsSeatInPlay))
                {
                    StartReconnectionCountdown(absentSeatNumber, FindReconnectionTime(absentSeatNumber));
                }

                _matchListener.ReceiveMatchStateChanged(BuildRecipientViews());
            }
        }

        internal bool Forfeit(int entrantId)
        {
            bool hasForfeited;
            bool hasJustFinished;
            lock (_matchLock)
            {
                int seatNumber = GetSeatNumber(entrantId);
                hasForfeited = IsSeatInPlay(seatNumber);
                if (hasForfeited)
                {
                    StopReconnectionCountdown(seatNumber);
                    RetireSeatOnTable(seatNumber);
                    _log.InfoFormat(SeatForfeitedMessage, seatNumber, MatchId);
                    _matchListener.ReceiveMatchStateChanged(BuildRecipientViews());
                }

                hasJustFinished = TakeFinishToReport();
            }

            ReportFinish(hasJustFinished);

            return hasForfeited;
        }

        internal bool OpenReconnectionWindow(int entrantId)
        {
            bool hasOpenedWindow;
            bool hasJustFinished;
            lock (_matchLock)
            {
                int seatNumber = GetSeatNumber(entrantId);
                bool isSeatWaiting = IsSeatInPlay(seatNumber) && !_reconnectionCountdownsBySeat.ContainsKey(seatNumber);
                TimeSpan reconnectionTime = isSeatWaiting ? FindReconnectionTime(seatNumber) : TimeSpan.Zero;
                hasOpenedWindow = reconnectionTime > TimeSpan.Zero;
                if (hasOpenedWindow)
                {
                    StartReconnectionCountdown(seatNumber, reconnectionTime);
                }
                else if (isSeatWaiting)
                {
                    RetireDisconnectedSeat(seatNumber);
                }

                if (isSeatWaiting)
                {
                    _matchListener.ReceiveMatchStateChanged(BuildRecipientViews());
                }

                hasJustFinished = TakeFinishToReport();
            }

            ReportFinish(hasJustFinished);

            return hasOpenedWindow;
        }

        internal bool CloseReconnectionWindow(int entrantId)
        {
            bool isBackInMatch;
            lock (_matchLock)
            {
                int seatNumber = GetSeatNumber(entrantId);
                isBackInMatch = IsSeatInPlay(seatNumber);
                StopReconnectionCountdown(seatNumber);
                _expiredStepCountsBySeat.Remove(seatNumber);
                if (isBackInMatch)
                {
                    _matchListener.ReceiveMatchStateChanged(BuildRecipientViews());
                }
                else if (_matchTable.Phase != TablePhase.Finished)
                {
                    _matchListener.ReceiveMatchStateChanged(BuildRetiredRecipientView(entrantId, seatNumber));
                }
            }

            return isBackInMatch;
        }

        internal IReadOnlyList<MatchParticipant> ReadPlayerResults()
        {
            List<MatchParticipant> playerResults;
            lock (_matchLock)
            {
                playerResults = MatchRecordConversion.ToPlayerResults(_matchTable, _seatedEntrants);
            }

            return playerResults;
        }

        internal string ReadEndReason()
        {
            string endReason;
            lock (_matchLock)
            {
                endReason = MatchRecordConversion.ToEndReason(_matchTable.FinishCause);
            }

            return endReason;
        }

        internal IReadOnlyList<EntrantStanding> ReadStandings()
        {
            var entrantStandings = new List<EntrantStanding>();
            lock (_matchLock)
            {
                if (_matchTable.Phase == TablePhase.Finished)
                {
                    int? winnerSeatNumber = _matchTable.FindRanking().FirstOrDefault()?.SeatNumber;
                    entrantStandings.AddRange(_seatedEntrants.Select(seatedEntrant => ToEntrantStanding(seatedEntrant, winnerSeatNumber)));
                }
            }

            return entrantStandings;
        }

        internal MatchActionResult PlacePiece(int entrantId, MatchPiece piece, BoardCell destinationCell)
        {
            MatchActionResult actionResult;
            bool hasJustFinished;
            lock (_matchLock)
            {
                TableStep stepBefore = ReadTableStep();
                actionResult = JudgeOpenStep(entrantId);
                if (actionResult == MatchActionResult.Applied)
                {
                    actionResult = PlacePieceOnTable(GetSeatNumber(entrantId), piece, destinationCell);
                }

                hasJustFinished = CompleteAction(entrantId, actionResult, stepBefore);
            }

            ReportFinish(hasJustFinished);

            return actionResult;
        }

        internal MatchActionResult MoveCaterpillar(int entrantId, CaterpillarMove move)
        {
            MatchActionResult actionResult;
            bool hasJustFinished;
            lock (_matchLock)
            {
                TableStep stepBefore = ReadTableStep();
                actionResult = JudgeOpenStep(entrantId);
                if (actionResult == MatchActionResult.Applied)
                {
                    actionResult = MatchActionResultConversion.ToActionResult(_matchTable.MoveCaterpillar(GetSeatNumber(entrantId), move));
                }

                hasJustFinished = CompleteAction(entrantId, actionResult, stepBefore);
            }

            ReportFinish(hasJustFinished);

            return actionResult;
        }

        internal MatchActionResult AddFlowerLevel(int entrantId, BoardCell buildCell)
        {
            MatchActionResult actionResult;
            bool hasJustFinished;
            lock (_matchLock)
            {
                TableStep stepBefore = ReadTableStep();
                actionResult = JudgeOpenStep(entrantId);
                if (actionResult == MatchActionResult.Applied)
                {
                    actionResult = MatchActionResultConversion.ToActionResult(_matchTable.AddFlowerLevel(GetSeatNumber(entrantId), buildCell));
                }

                hasJustFinished = CompleteAction(entrantId, actionResult, stepBefore);
            }

            ReportFinish(hasJustFinished);

            return actionResult;
        }

        internal MatchActionResult DrawCard(int entrantId)
        {
            MatchActionResult actionResult;
            bool hasJustFinished;
            lock (_matchLock)
            {
                TableStep stepBefore = ReadTableStep();
                actionResult = JudgeOpenStep(entrantId);
                if (actionResult == MatchActionResult.Applied)
                {
                    actionResult = MatchActionResultConversion.ToActionResult(_matchTable.CardTable.DrawCard(GetSeatNumber(entrantId)));
                }

                hasJustFinished = CompleteAction(entrantId, actionResult, stepBefore);
            }

            ReportFinish(hasJustFinished);

            return actionResult;
        }

        internal MatchActionResult PlayCard(int entrantId, CardPlay cardPlay)
        {
            MatchActionResult actionResult;
            bool hasJustFinished;
            lock (_matchLock)
            {
                TableStep stepBefore = ReadTableStep();
                actionResult = JudgeOpenStep(entrantId);
                if (actionResult == MatchActionResult.Applied)
                {
                    actionResult = MatchActionResultConversion.ToActionResult(_matchTable.CardTable.PlayCard(GetSeatNumber(entrantId), cardPlay));
                }

                hasJustFinished = CompleteAction(entrantId, actionResult, stepBefore);
            }

            ReportFinish(hasJustFinished);

            return actionResult;
        }

        internal MatchActionResult EndTurn(int entrantId)
        {
            MatchActionResult actionResult;
            bool hasJustFinished;
            lock (_matchLock)
            {
                TableStep stepBefore = ReadTableStep();
                actionResult = JudgeOpenStep(entrantId);
                if (actionResult == MatchActionResult.Applied)
                {
                    actionResult = MatchActionResultConversion.ToActionResult(_matchTable.EndTurn(GetSeatNumber(entrantId)));
                }

                hasJustFinished = CompleteAction(entrantId, actionResult, stepBefore);
            }

            ReportFinish(hasJustFinished);

            return actionResult;
        }

        private MatchActionResult PlacePieceOnTable(int seatNumber, MatchPiece piece, BoardCell destinationCell)
        {
            MatchActionResult actionResult = piece switch
            {
                MatchPiece.Butterfly => MatchActionResultConversion.ToActionResult(_matchTable.PlaceButterfly(seatNumber, destinationCell)),
                MatchPiece.Caterpillar => PlaceCaterpillarOnTable(seatNumber, destinationCell),
                _ => throw new ArgumentOutOfRangeException(nameof(piece)),
            };

            return actionResult;
        }

        private MatchActionResult PlaceCaterpillarOnTable(int seatNumber, BoardCell destinationCell)
        {
            MatchActionResult actionResult;
            if (_matchTable.Phase == TablePhase.PlayerTurn)
            {
                actionResult = MatchActionResultConversion.ToActionResult(_matchTable.PlaceCaterpillar(seatNumber, destinationCell));
            }
            else
            {
                actionResult = MatchActionResultConversion.ToActionResult(_matchTable.PlaceInitialCaterpillar(seatNumber, destinationCell));
            }

            return actionResult;
        }

        private MatchActionResult JudgeOpenStep(int entrantId)
        {
            MatchActionResult actionResult = MatchActionResult.Applied;
            if (_matchTable.IsSeatRetired(GetSeatNumber(entrantId)))
            {
                actionResult = MatchActionResult.NotInMatch;
            }
            else if (HasStepExpired())
            {
                actionResult = MatchActionResult.TimeExpired;
            }

            return actionResult;
        }

        private bool CompleteAction(int entrantId, MatchActionResult actionResult, TableStep stepBefore)
        {
            if (actionResult == MatchActionResult.Applied)
            {
                _expiredStepCountsBySeat.Remove(GetSeatNumber(entrantId));
                if (ReadTableStep() != stepBefore)
                {
                    RestartStepCountdown();
                }

                _matchListener.ReceiveMatchStateChanged(BuildRecipientViews());
            }

            bool hasJustFinished = TakeFinishToReport();

            return hasJustFinished;
        }

        private TableStep ReadTableStep()
        {
            var tableStep = new TableStep(_matchTable.Phase, _matchTable.RoundNumber, _matchTable.TurnRoundNumber, _matchTable.ActiveSeatNumber);

            return tableStep;
        }

        private void ExpireStep()
        {
            bool hasJustFinished;
            int? absentEntrantId = null;
            lock (_matchLock)
            {
                bool isCurrentCountdown = (_stepCountdown is not null) && _stepCountdown.IsCancellationRequested;
                if (isCurrentCountdown)
                {
                    int expiredSeatNumber = _matchTable.ActiveSeatNumber;
                    ExpireTableStep();
                    absentEntrantId = CountExpiredStep(expiredSeatNumber);
                    RestartStepCountdown();
                    _matchListener.ReceiveMatchStateChanged(BuildRecipientViews());
                }

                hasJustFinished = TakeFinishToReport();
            }

            ReportFinish(hasJustFinished);
            ReportAbsence(absentEntrantId);
        }

        private int? CountExpiredStep(int expiredSeatNumber)
        {
            bool isCounted = (_absentAfterExpiredSteps > 0) && IsSeatInPlay(expiredSeatNumber) && !_reconnectionCountdownsBySeat.ContainsKey(expiredSeatNumber);
            int? absentEntrantId = null;
            if (isCounted)
            {
                int expiredStepCount = _expiredStepCountsBySeat.GetValueOrDefault(expiredSeatNumber) + 1;
                _expiredStepCountsBySeat[expiredSeatNumber] = expiredStepCount;
                if (expiredStepCount >= _absentAfterExpiredSteps)
                {
                    _expiredStepCountsBySeat.Remove(expiredSeatNumber);
                    absentEntrantId = _seatedEntrants.First(seatedEntrant => seatedEntrant.SeatNumber == expiredSeatNumber).Entrant.EntrantId;
                }
            }

            return absentEntrantId;
        }

        private void ReportAbsence(int? absentEntrantId)
        {
            if (absentEntrantId is int entrantId)
            {
                _matchListener.ReceiveSeatAbsent(entrantId);
            }
        }

        private void ExpireTableStep()
        {
            if (_matchTable.Phase == TablePhase.PlayerTurn)
            {
                _matchTable.ExpireTurn();
            }
            else
            {
                _matchTable.ExpirePlacement();
            }
        }

        private bool TakeFinishToReport()
        {
            bool hasJustFinished = (_matchTable.Phase == TablePhase.Finished) && !_hasReportedFinish;
            if (hasJustFinished)
            {
                _hasReportedFinish = true;
                StopReconnectionCountdowns();
            }

            return hasJustFinished;
        }

        private void ReportFinish(bool hasJustFinished)
        {
            if (hasJustFinished)
            {
                Finished?.Invoke(this, EventArgs.Empty);
            }
        }

        private void RestartStepCountdown()
        {
            StopStepCountdown();
            StartStepCountdown();
            SaveStepStart();
        }

        private void SaveStepStart()
        {
            if (_matchTable.Phase != TablePhase.Finished)
            {
                _stepStartSnapshot = _matchTable.TakeSnapshot();
                SaveCheckpoint();
            }
        }

        private void SaveCheckpoint()
        {
            if ((_stepStartSnapshot is null) || (_matchTable.Phase == TablePhase.Finished))
            {
                return;
            }

            List<CheckpointSeat> checkpointSeats = _seatedEntrants.Select(seatedEntrant => MatchCheckpointConversion.ToCheckpointSeat(seatedEntrant, _matchTable.IsSeatRetired(seatedEntrant.SeatNumber))).ToList();
            _recordWriter.RecordCheckpoint(MatchId, MatchCheckpointConversion.ToCheckpoint(_stepStartSnapshot, RoomCode, checkpointSeats));
        }

        private void StopStepCountdown()
        {
            _stepCountdown?.Dispose();
            _stepCountdown = null;
            _stepEndsAtUtc = null;
        }

        private void StartStepCountdown()
        {
            if (_matchTable.Phase == TablePhase.Finished)
            {
                return;
            }

            _stepEndsAtUtc = _timeProvider.GetUtcNow().UtcDateTime + _stepDuration;
            _stepCountdown = new CancellationTokenSource(_stepDuration, _timeProvider);
            _stepCountdown.Token.Register(ExpireStep);
        }

        private bool IsSeatInPlay(int seatNumber)
        {
            bool isInPlay = (_matchTable.Phase != TablePhase.Finished) && !_matchTable.IsSeatRetired(seatNumber);

            return isInPlay;
        }

        private TimeSpan FindReconnectionTime(int seatNumber)
        {
            TimeSpan reconnectionTime = _reconnectionDuration;
            if (seatNumber == _matchTable.ActiveSeatNumber)
            {
                reconnectionTime = ReadStepTimeLeft() ?? TimeSpan.Zero;
            }

            return reconnectionTime;
        }

        private void StartReconnectionCountdown(int seatNumber, TimeSpan reconnectionTime)
        {
            var reconnectionCountdown = new CancellationTokenSource(reconnectionTime, _timeProvider);
            _reconnectionCountdownsBySeat.Add(seatNumber, reconnectionCountdown);
            reconnectionCountdown.Token.Register(() => ExpireReconnectionWindow(seatNumber, reconnectionCountdown));
        }

        private void ExpireReconnectionWindow(int seatNumber, CancellationTokenSource expiredCountdown)
        {
            bool hasJustFinished;
            lock (_matchLock)
            {
                bool isCurrentWindow = _reconnectionCountdownsBySeat.TryGetValue(seatNumber, out CancellationTokenSource? currentCountdown) && ReferenceEquals(currentCountdown, expiredCountdown);
                if (isCurrentWindow)
                {
                    StopReconnectionCountdown(seatNumber);
                    RetireDisconnectedSeat(seatNumber);
                    _matchListener.ReceiveMatchStateChanged(BuildRecipientViews());
                }

                hasJustFinished = TakeFinishToReport();
            }

            ReportFinish(hasJustFinished);
        }

        private void RetireDisconnectedSeat(int seatNumber)
        {
            RetireSeatOnTable(seatNumber);
            _log.InfoFormat(SeatRetiredMessage, seatNumber, MatchId);
        }

        private void RetireSeatOnTable(int seatNumber)
        {
            TableStep stepBefore = ReadTableStep();
            _matchTable.RetireSeat(seatNumber);
            if (ReadTableStep() != stepBefore)
            {
                RestartStepCountdown();
            }
            else
            {
                SaveCheckpoint();
            }
        }

        private void StopReconnectionCountdown(int seatNumber)
        {
            if (_reconnectionCountdownsBySeat.Remove(seatNumber, out CancellationTokenSource? stoppedCountdown))
            {
                stoppedCountdown.Dispose();
            }
        }

        private void StopReconnectionCountdowns()
        {
            foreach (CancellationTokenSource stoppedCountdown in _reconnectionCountdownsBySeat.Values)
            {
                stoppedCountdown.Dispose();
            }

            _reconnectionCountdownsBySeat.Clear();
        }

        private bool HasStepExpired()
        {
            bool hasExpired = (_stepEndsAtUtc is DateTime endsAtUtc) && (_timeProvider.GetUtcNow().UtcDateTime >= endsAtUtc);

            return hasExpired;
        }

        private TimeSpan? ReadStepTimeLeft()
        {
            TimeSpan? stepTimeLeft = null;
            if (_stepEndsAtUtc is DateTime endsAtUtc)
            {
                stepTimeLeft = endsAtUtc - _timeProvider.GetUtcNow().UtcDateTime;
            }

            return stepTimeLeft;
        }

        private EntrantStanding ToEntrantStanding(SeatedEntrant seatedEntrant, int? winnerSeatNumber)
        {
            SeatStanding seatStanding = _matchTable.Standings[seatedEntrant.SeatNumber - MatchTable.FirstSeatNumber];
            bool isWinner = seatStanding.SeatNumber == winnerSeatNumber;
            var entrantStanding = new EntrantStanding(seatedEntrant.Entrant.MemberId, seatStanding.FinalScore, isWinner);

            return entrantStanding;
        }

        private int GetSeatNumber(int entrantId)
        {
            int seatNumber = _seatedEntrants.First(seatedEntrant => seatedEntrant.Entrant.EntrantId == entrantId).SeatNumber;

            return seatNumber;
        }

        private List<MatchRecipientView> BuildRetiredRecipientView(int entrantId, int seatNumber)
        {
            MatchView retiredView = _viewFactory.CreateForSeat(_matchTable, ReadStepTimeLeft(), seatNumber);
            var retiredRecipientViews = new List<MatchRecipientView> { new MatchRecipientView(entrantId, MatchId, retiredView) };

            return retiredRecipientViews;
        }

        private List<MatchRecipientView> BuildRecipientViews()
        {
            TimeSpan? stepTimeLeft = ReadStepTimeLeft();
            var recipientViews = new List<MatchRecipientView>();
            foreach (SeatedEntrant recipient in _seatedEntrants.Where(seatedEntrant => !_matchTable.IsSeatRetired(seatedEntrant.SeatNumber)))
            {
                MatchView matchView = _viewFactory.CreateForSeat(_matchTable, stepTimeLeft, recipient.SeatNumber);
                recipientViews.Add(new MatchRecipientView(recipient.Entrant.EntrantId, MatchId, matchView));
            }

            return recipientViews;
        }
    }
}
