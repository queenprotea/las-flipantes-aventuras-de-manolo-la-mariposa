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

        private static readonly ILog _log = LogManager.GetLogger(typeof(LiveMatch));

        private readonly Lock _matchLock = new Lock();
        private readonly Dictionary<int, CancellationTokenSource> _reconnectionCountdownsBySeat = new Dictionary<int, CancellationTokenSource>();
        private readonly MatchTable _matchTable;
        private readonly IReadOnlyList<SeatedEntrant> _seatedEntrants;
        private readonly MatchViewFactory _viewFactory;
        private readonly TimeProvider _timeProvider;
        private readonly IMatchListener _matchListener;
        private readonly TimeSpan _stepDuration;
        private readonly TimeSpan _reconnectionDuration;

        private CancellationTokenSource? _stepCountdown;
        private DateTime? _stepEndsAtUtc;
        private bool _hasEnded;
        private bool _hasReportedFinish;

        internal LiveMatch(int matchId, MatchTable matchTable, IReadOnlyList<SeatedEntrant> seatedEntrants, TimeProvider timeProvider, IMatchListener matchListener, MatchTimeLimits timeLimits)
        {
            MatchId = matchId;
            _matchTable = matchTable;
            _seatedEntrants = seatedEntrants;
            _viewFactory = new MatchViewFactory(seatedEntrants, _reconnectionCountdownsBySeat.Keys, timeLimits.ReconnectionTime);
            _timeProvider = timeProvider;
            _matchListener = matchListener;
            _stepDuration = timeLimits.StepTime;
            _reconnectionDuration = timeLimits.ReconnectionTime;
        }

        internal event EventHandler? Finished;

        internal int MatchId { get; }

        internal void StartPlacement()
        {
            lock (_matchLock)
            {
                RestartStepCountdown();
                _matchListener.ReceiveMatchStarted(BuildRecipientViews());
            }
        }

        internal void EndMatch()
        {
            lock (_matchLock)
            {
                _hasEnded = true;
                StopStepCountdown();
                StopReconnectionCountdowns();
            }
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
                if (isBackInMatch)
                {
                    _matchListener.ReceiveMatchStateChanged(BuildRecipientViews());
                }
                else if (!_hasEnded)
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
                playerResults = _matchTable.Phase == TablePhase.Finished ? MatchRecordConversion.ToPlayerResults(_matchTable, _seatedEntrants) : MatchRecordConversion.ToAbandonedResults(_matchTable, _seatedEntrants);
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

                hasJustFinished = CompleteAction(actionResult, stepBefore);
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

                hasJustFinished = CompleteAction(actionResult, stepBefore);
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

                hasJustFinished = CompleteAction(actionResult, stepBefore);
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

                hasJustFinished = CompleteAction(actionResult, stepBefore);
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

                hasJustFinished = CompleteAction(actionResult, stepBefore);
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

                hasJustFinished = CompleteAction(actionResult, stepBefore);
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
            if (_hasEnded || _matchTable.IsSeatRetired(GetSeatNumber(entrantId)))
            {
                actionResult = MatchActionResult.NotInMatch;
            }
            else if (HasStepExpired())
            {
                actionResult = MatchActionResult.TimeExpired;
            }

            return actionResult;
        }

        private bool CompleteAction(MatchActionResult actionResult, TableStep stepBefore)
        {
            if (actionResult == MatchActionResult.Applied)
            {
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
            lock (_matchLock)
            {
                bool isCurrentCountdown = (_stepCountdown is not null) && _stepCountdown.IsCancellationRequested;
                if (isCurrentCountdown)
                {
                    ExpireTableStep();
                    RestartStepCountdown();
                    _matchListener.ReceiveMatchStateChanged(BuildRecipientViews());
                }

                hasJustFinished = TakeFinishToReport();
            }

            ReportFinish(hasJustFinished);
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
            bool isInPlay = !_hasEnded && (_matchTable.Phase != TablePhase.Finished) && !_matchTable.IsSeatRetired(seatNumber);

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
            TableStep stepBefore = ReadTableStep();
            _matchTable.RetireSeat(seatNumber);
            if (ReadTableStep() != stepBefore)
            {
                RestartStepCountdown();
            }

            _log.InfoFormat(SeatRetiredMessage, seatNumber, MatchId);
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

        private int GetSeatNumber(int entrantId)
        {
            int seatNumber = _seatedEntrants.First(seatedEntrant => seatedEntrant.Entrant.EntrantId == entrantId).SeatNumber;

            return seatNumber;
        }

        private List<MatchRecipientView> BuildRetiredRecipientView(int entrantId, int seatNumber)
        {
            MatchView retiredView = _viewFactory.CreateForSeat(_matchTable, ReadStepTimeLeft(), seatNumber);
            var retiredRecipientViews = new List<MatchRecipientView> { new MatchRecipientView(entrantId, retiredView) };

            return retiredRecipientViews;
        }

        private List<MatchRecipientView> BuildRecipientViews()
        {
            TimeSpan? stepTimeLeft = ReadStepTimeLeft();
            var recipientViews = new List<MatchRecipientView>();
            foreach (SeatedEntrant recipient in _seatedEntrants.Where(seatedEntrant => !_matchTable.IsSeatRetired(seatedEntrant.SeatNumber)))
            {
                MatchView matchView = _viewFactory.CreateForSeat(_matchTable, stepTimeLeft, recipient.SeatNumber);
                recipientViews.Add(new MatchRecipientView(recipient.Entrant.EntrantId, matchView));
            }

            return recipientViews;
        }
    }
}
