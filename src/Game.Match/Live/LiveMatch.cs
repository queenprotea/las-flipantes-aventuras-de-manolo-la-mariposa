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
        private readonly MatchTable _table;
        private readonly IReadOnlyList<SeatedEntrant> _seatedEntrants;
        private readonly MatchViewFactory _viewFactory;
        private readonly TimeProvider _timeProvider;
        private readonly IMatchListener _listener;
        private readonly TimeSpan _placementTime;

        private CancellationTokenSource? _placementCountdown;
        private DateTime? _placementEndsAtUtc;
        private bool _hasEnded;

        internal LiveMatch(int matchId, MatchTable table, IReadOnlyList<SeatedEntrant> seatedEntrants, TimeProvider timeProvider, IMatchListener listener, TimeSpan placementTime)
        {
            MatchId = matchId;
            _table = table;
            _seatedEntrants = seatedEntrants;
            _viewFactory = new MatchViewFactory(seatedEntrants);
            _timeProvider = timeProvider;
            _listener = listener;
            _placementTime = placementTime;
        }

        internal int MatchId { get; }

        internal void Start()
        {
            lock (_matchLock)
            {
                RestartPlacementCountdown();
                _listener.ReceiveMatchStarted(BuildRecipientViews());
            }
        }

        internal void End()
        {
            lock (_matchLock)
            {
                _hasEnded = true;
                StopPlacementCountdown();
            }
        }

        internal PlacementResult PlaceCaterpillar(int entrantId, BoardCell cell)
        {
            PlacementResult placementResult;
            lock (_matchLock)
            {
                placementResult = Place(entrantId, cell, _table.PlaceInitialCaterpillar);
            }

            return placementResult;
        }

        internal PlacementResult PlaceButterfly(int entrantId, BoardCell cell)
        {
            PlacementResult placementResult;
            lock (_matchLock)
            {
                placementResult = Place(entrantId, cell, _table.PlaceButterfly);
            }

            return placementResult;
        }

        private static PlacementResult ToPlacementResult(PlacementOutcome outcome)
        {
            PlacementResult placementResult = outcome switch
            {
                PlacementOutcome.Placed => PlacementResult.Placed,
                PlacementOutcome.WrongPhase => PlacementResult.WrongPhase,
                PlacementOutcome.NotYourPlacement => PlacementResult.NotYourPlacement,
                PlacementOutcome.NotAFlower => PlacementResult.NotAFlower,
                PlacementOutcome.FlowerOccupied => PlacementResult.FlowerOccupied,
                _ => throw new ArgumentOutOfRangeException(nameof(outcome)),
            };

            return placementResult;
        }

        private PlacementResult Place(int entrantId, BoardCell cell, Func<int, BoardCell, PlacementOutcome> placeOnTable)
        {
            PlacementResult placementResult = PlacementResult.TimeExpired;
            if (_hasEnded)
            {
                placementResult = PlacementResult.NotInMatch;
            }
            else if (!HasPlacementExpired())
            {
                PlacementOutcome outcome = placeOnTable(GetSeatNumber(entrantId), cell);
                placementResult = ToPlacementResult(outcome);
            }

            if (placementResult == PlacementResult.Placed)
            {
                RestartPlacementCountdown();
                _listener.ReceiveMatchStateChanged(BuildRecipientViews());
            }

            return placementResult;
        }

        private void ExpirePlacement()
        {
            lock (_matchLock)
            {
                bool isCurrentCountdown = (_placementCountdown is not null) && _placementCountdown.IsCancellationRequested;

                if (!isCurrentCountdown)
                {
                    return;
                }

                _table.ExpirePlacement();
                RestartPlacementCountdown();
                _listener.ReceiveMatchStateChanged(BuildRecipientViews());
            }
        }

        private void RestartPlacementCountdown()
        {
            StopPlacementCountdown();
            StartPlacementCountdown();
        }

        private void StopPlacementCountdown()
        {
            _placementCountdown?.Dispose();
            _placementCountdown = null;
            _placementEndsAtUtc = null;
        }

        private void StartPlacementCountdown()
        {
            if (_table.Phase == TablePhase.PlayerTurn)
            {
                return;
            }

            _placementEndsAtUtc = _timeProvider.GetUtcNow().UtcDateTime + _placementTime;
            _placementCountdown = new CancellationTokenSource(_placementTime, _timeProvider);
            _placementCountdown.Token.Register(ExpirePlacement);
        }

        private bool HasPlacementExpired()
        {
            bool hasExpired = (_placementEndsAtUtc is DateTime endsAtUtc) && (_timeProvider.GetUtcNow().UtcDateTime >= endsAtUtc);

            return hasExpired;
        }

        private TimeSpan? ReadPlacementTimeLeft()
        {
            TimeSpan? placementTimeLeft = null;
            if (_placementEndsAtUtc is DateTime endsAtUtc)
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
            TimeSpan? placementTimeLeft = ReadPlacementTimeLeft();
            var recipientViews = new List<MatchRecipientView>();
            foreach (SeatedEntrant recipient in _seatedEntrants)
            {
                MatchView matchView = _viewFactory.Create(_table, placementTimeLeft, recipient.SeatNumber);
                recipientViews.Add(new MatchRecipientView(recipient.Entrant.EntrantId, matchView));
            }

            return recipientViews;
        }
    }
}
