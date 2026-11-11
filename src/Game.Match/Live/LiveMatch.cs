using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Game.Contracts;
using Game.Domain;

namespace Game.Match
{
    internal sealed class LiveMatch
    {
        private readonly Lock _matchLock = new Lock();
        private readonly MatchTable _matchTable;
        private readonly IReadOnlyList<SeatedEntrant> _seatedEntrants;
        private readonly MatchViewFactory _viewFactory;
        private readonly TimeProvider _timeProvider;
        private readonly IMatchListener _matchListener;
        private readonly TimeSpan _stepTime;

        private CancellationTokenSource? _stepCountdown;
        private DateTime? _stepEndsAtUtc;
        private bool _hasEnded;
        private bool _hasReportedFinish;

        internal LiveMatch(int matchId, MatchTable matchTable, IReadOnlyList<SeatedEntrant> seatedEntrants, TimeProvider timeProvider, IMatchListener matchListener, TimeSpan stepTime)
        {
            MatchId = matchId;
            _matchTable = matchTable;
            _seatedEntrants = seatedEntrants;
            _viewFactory = new MatchViewFactory(seatedEntrants);
            _timeProvider = timeProvider;
            _matchListener = matchListener;
            _stepTime = stepTime;
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
            }
        }

        internal PlacementResult PlaceCaterpillar(int entrantId, BoardCell destinationCell)
        {
            PlacementResult placementResult;
            lock (_matchLock)
            {
                placementResult = PlaceOnTable(entrantId, destinationCell, _matchTable.PlaceInitialCaterpillar);
            }

            return placementResult;
        }

        internal PlacementResult PlaceButterfly(int entrantId, BoardCell destinationCell)
        {
            PlacementResult placementResult;
            lock (_matchLock)
            {
                placementResult = PlaceOnTable(entrantId, destinationCell, _matchTable.PlaceButterfly);
            }

            return placementResult;
        }

        private static PlacementResult ToPlacementResult(PlacementOutcome placementOutcome)
        {
            PlacementResult placementResult = placementOutcome switch
            {
                PlacementOutcome.Placed => PlacementResult.Placed,
                PlacementOutcome.WrongPhase => PlacementResult.WrongPhase,
                PlacementOutcome.NotYourPlacement => PlacementResult.NotYourPlacement,
                PlacementOutcome.NotAFlower => PlacementResult.NotAFlower,
                PlacementOutcome.FlowerOccupied => PlacementResult.FlowerOccupied,
                _ => throw new ArgumentOutOfRangeException(nameof(placementOutcome)),
            };

            return placementResult;
        }

        private PlacementResult PlaceOnTable(int entrantId, BoardCell destinationCell, Func<int, BoardCell, PlacementOutcome> placeOnTable)
        {
            PlacementResult placementResult = PlacementResult.TimeExpired;
            if (_hasEnded)
            {
                placementResult = PlacementResult.NotInMatch;
            }
            else if (!HasStepExpired())
            {
                PlacementOutcome placementOutcome = placeOnTable(GetSeatNumber(entrantId), destinationCell);
                placementResult = ToPlacementResult(placementOutcome);
            }

            if (placementResult == PlacementResult.Placed)
            {
                RestartStepCountdown();
                _matchListener.ReceiveMatchStateChanged(BuildRecipientViews());
            }

            return placementResult;
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

            _stepEndsAtUtc = _timeProvider.GetUtcNow().UtcDateTime + _stepTime;
            _stepCountdown = new CancellationTokenSource(_stepTime, _timeProvider);
            _stepCountdown.Token.Register(ExpireStep);
        }

        private bool HasStepExpired()
        {
            bool hasExpired = (_stepEndsAtUtc is DateTime endsAtUtc) && (_timeProvider.GetUtcNow().UtcDateTime >= endsAtUtc);

            return hasExpired;
        }

        private TimeSpan? ReadStepTimeLeft()
        {
            TimeSpan? placementTimeLeft = null;
            if (_stepEndsAtUtc is DateTime endsAtUtc)
            {
                placementTimeLeft = endsAtUtc - _timeProvider.GetUtcNow().UtcDateTime;
            }

            return placementTimeLeft;
        }

        private int GetSeatNumber(int entrantId)
        {
            int seatNumber = _seatedEntrants.First(seatedEntrant => seatedEntrant.Entrant.EntrantId == entrantId).SeatNumber;

            return seatNumber;
        }

        private List<MatchRecipientView> BuildRecipientViews()
        {
            TimeSpan? stepTimeLeft = ReadStepTimeLeft();
            var recipientViews = new List<MatchRecipientView>();
            foreach (SeatedEntrant recipient in _seatedEntrants)
            {
                MatchView matchView = _viewFactory.CreateForSeat(_matchTable, stepTimeLeft, recipient.SeatNumber);
                recipientViews.Add(new MatchRecipientView(recipient.Entrant.EntrantId, matchView));
            }

            return recipientViews;
        }
    }
}
