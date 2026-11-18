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
    internal sealed class AwaitingMatch
    {
        private const string SeatForfeitedMessage = "Seat {0} of match {1} forfeited while the match waited for its players.";
        private const string NobodyReturnedMessage = "Nobody came back to match {0} before its resume time ran out; the match is interrupted.";
        private const string OneReturnedMessage = "Only one player came back to match {0} before its resume time ran out; the match is abandoned.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(AwaitingMatch));

        private readonly Lock _matchLock = new Lock();
        private readonly HashSet<int> _absentSeatNumbers;
        private readonly MatchTable _matchTable;
        private readonly SeatedMatch _seatedMatch;
        private readonly MatchSnapshot _stepStartSnapshot;
        private readonly MatchViewFactory _viewFactory;
        private readonly MatchRecordWriter _recordWriter;
        private readonly TimeProvider _timeProvider;
        private readonly IMatchListener _matchListener;

        private CancellationTokenSource? _resumeCountdown;
        private DateTime _resumeEndsAtUtc;
        private bool _isSettled;
        private bool _isInterrupted;

        internal AwaitingMatch(SeatedMatch seatedMatch, MatchTable matchTable, MatchRecordWriter recordWriter, TimeProvider timeProvider, IMatchListener matchListener, MatchTimeLimits timeLimits)
        {
            _seatedMatch = seatedMatch;
            _matchTable = matchTable;
            _stepStartSnapshot = matchTable.TakeSnapshot();
            _absentSeatNumbers = seatedMatch.SeatedEntrants.Select(seatedEntrant => seatedEntrant.SeatNumber).Where(seatNumber => !matchTable.IsSeatRetired(seatNumber)).ToHashSet();
            _viewFactory = new MatchViewFactory(seatedMatch, _absentSeatNumbers, timeLimits.ReconnectionTime);
            _recordWriter = recordWriter;
            _timeProvider = timeProvider;
            _matchListener = matchListener;
            TimeLimits = timeLimits;
        }

        internal event EventHandler? Resumed;

        internal event EventHandler? Finished;

        internal SeatedMatch SeatedMatch => _seatedMatch;

        internal MatchTable Table => _matchTable;

        internal MatchTimeLimits TimeLimits { get; }

        internal void StartWaiting(TimeSpan resumeTime)
        {
            lock (_matchLock)
            {
                _resumeEndsAtUtc = _timeProvider.GetUtcNow().UtcDateTime + resumeTime;
                _resumeCountdown = new CancellationTokenSource(resumeTime, _timeProvider);
                _resumeCountdown.Token.Register(ExpireResumeTime);
            }
        }

        internal IReadOnlyCollection<int> ReadAbsentSeatNumbers()
        {
            List<int> absentSeatNumbers;
            lock (_matchLock)
            {
                absentSeatNumbers = _absentSeatNumbers.ToList();
            }

            return absentSeatNumbers;
        }

        internal bool HoldsSeatOf(int entrantId)
        {
            bool holdsSeat;
            lock (_matchLock)
            {
                holdsSeat = IsSeatWaiting(GetSeatNumber(entrantId));
            }

            return holdsSeat;
        }

        internal bool ReturnSeat(int entrantId)
        {
            bool isReturned;
            bool canResume = false;
            lock (_matchLock)
            {
                int seatNumber = GetSeatNumber(entrantId);
                isReturned = IsSeatWaiting(seatNumber);
                if (isReturned)
                {
                    _absentSeatNumbers.Remove(seatNumber);
                    canResume = CountReturnedSeats() >= MatchTable.MinimumSeatCount;
                }

                if (canResume)
                {
                    Settle();
                }
                else if (isReturned)
                {
                    _matchListener.ReceiveMatchStateChanged(BuildReturnedViews());
                }
            }

            if (canResume)
            {
                Resumed?.Invoke(this, EventArgs.Empty);
            }

            return isReturned;
        }

        internal void ReleaseSeat(int entrantId)
        {
            lock (_matchLock)
            {
                int seatNumber = GetSeatNumber(entrantId);
                if (IsSeatWaiting(seatNumber) && _absentSeatNumbers.Add(seatNumber))
                {
                    _matchListener.ReceiveMatchStateChanged(BuildReturnedViews());
                }
            }
        }

        internal bool Forfeit(int entrantId)
        {
            bool hasForfeited;
            bool hasFinished = false;
            lock (_matchLock)
            {
                int seatNumber = GetSeatNumber(entrantId);
                hasForfeited = IsSeatWaiting(seatNumber);
                if (hasForfeited)
                {
                    _matchTable.RetireSeat(seatNumber);
                    _absentSeatNumbers.Remove(seatNumber);
                    _log.InfoFormat(SeatForfeitedMessage, seatNumber, _seatedMatch.MatchId);
                    hasFinished = _matchTable.Phase == TablePhase.Finished;
                    SaveCheckpoint();
                }

                if (hasFinished)
                {
                    Settle();
                }

                if (hasForfeited)
                {
                    _matchListener.ReceiveMatchStateChanged(BuildReturnedViews());
                }
            }

            if (hasFinished)
            {
                Finished?.Invoke(this, EventArgs.Empty);
            }

            return hasForfeited;
        }

        internal string ReadEndReason()
        {
            string endReason;
            lock (_matchLock)
            {
                endReason = _isInterrupted ? MatchEndReasonCodes.Interrupted : MatchRecordConversion.ToEndReason(_matchTable.FinishCause);
            }

            return endReason;
        }

        internal IReadOnlyList<MatchParticipant> ReadPlayerResults()
        {
            IReadOnlyList<MatchParticipant> playerResults = Array.Empty<MatchParticipant>();
            lock (_matchLock)
            {
                if (!_isInterrupted)
                {
                    playerResults = MatchRecordConversion.ToPlayerResults(_matchTable, _seatedMatch.SeatedEntrants);
                }
            }

            return playerResults;
        }

        private void ExpireResumeTime()
        {
            bool hasExpired;
            lock (_matchLock)
            {
                hasExpired = !_isSettled;
                if (hasExpired)
                {
                    FinishWithoutResuming();
                }
            }

            if (hasExpired)
            {
                Finished?.Invoke(this, EventArgs.Empty);
            }
        }

        private void FinishWithoutResuming()
        {
            Settle();
            if (CountReturnedSeats() == 0)
            {
                _isInterrupted = true;
                _log.InfoFormat(NobodyReturnedMessage, _seatedMatch.MatchId);
                return;
            }

            foreach (int absentSeatNumber in _absentSeatNumbers.ToList())
            {
                _matchTable.RetireSeat(absentSeatNumber);
            }

            _absentSeatNumbers.Clear();
            _log.InfoFormat(OneReturnedMessage, _seatedMatch.MatchId);
            _matchListener.ReceiveMatchStateChanged(BuildReturnedViews());
        }

        private void Settle()
        {
            _isSettled = true;
            _resumeCountdown?.Dispose();
            _resumeCountdown = null;
        }

        private bool IsSeatWaiting(int seatNumber)
        {
            bool isWaiting = !_isSettled && !_matchTable.IsSeatRetired(seatNumber);

            return isWaiting;
        }

        private int CountReturnedSeats()
        {
            int returnedCount = _seatedMatch.SeatedEntrants.Count(seatedEntrant => !_matchTable.IsSeatRetired(seatedEntrant.SeatNumber) && !_absentSeatNumbers.Contains(seatedEntrant.SeatNumber));

            return returnedCount;
        }

        private int GetSeatNumber(int entrantId)
        {
            int seatNumber = _seatedMatch.SeatedEntrants.First(seatedEntrant => seatedEntrant.Entrant.EntrantId == entrantId).SeatNumber;

            return seatNumber;
        }

        private void SaveCheckpoint()
        {
            if (_matchTable.Phase == TablePhase.Finished)
            {
                return;
            }

            List<CheckpointSeat> checkpointSeats = _seatedMatch.SeatedEntrants.Select(seatedEntrant => MatchCheckpointConversion.ToCheckpointSeat(seatedEntrant, _matchTable.IsSeatRetired(seatedEntrant.SeatNumber))).ToList();
            _recordWriter.RecordCheckpoint(_seatedMatch.MatchId, MatchCheckpointConversion.ToCheckpoint(_stepStartSnapshot, _seatedMatch.RoomCode, checkpointSeats));
        }

        private List<MatchRecipientView> BuildReturnedViews()
        {
            TimeSpan resumeTimeLeft = _resumeEndsAtUtc - _timeProvider.GetUtcNow().UtcDateTime;
            var recipientViews = new List<MatchRecipientView>();
            IEnumerable<SeatedEntrant> returnedEntrants = _seatedMatch.SeatedEntrants.Where(seatedEntrant => !_matchTable.IsSeatRetired(seatedEntrant.SeatNumber) && !_absentSeatNumbers.Contains(seatedEntrant.SeatNumber));
            foreach (SeatedEntrant recipient in returnedEntrants)
            {
                recipientViews.Add(new MatchRecipientView(recipient.Entrant.EntrantId, BuildViewFor(recipient.SeatNumber, resumeTimeLeft)));
            }

            return recipientViews;
        }

        private MatchView BuildViewFor(int viewerSeatNumber, TimeSpan resumeTimeLeft)
        {
            TimeSpan? noStepTimeLeft = null;
            MatchView matchView = _viewFactory.CreateForSeat(_matchTable, noStepTimeLeft, viewerSeatNumber);
            if (_matchTable.Phase != TablePhase.Finished)
            {
                matchView.Phase = MatchPhase.AwaitingPlayers;
                matchView.StepTimeLeft = resumeTimeLeft;
                matchView.PlacementTargets.Clear();
                matchView.MoveTargets.Clear();
                matchView.BuildTargets.Clear();
                matchView.PlayableCards.Clear();
                matchView.LevelRemovalTargets.Clear();
                matchView.CanDrawCard = false;
            }

            return matchView;
        }
    }
}
