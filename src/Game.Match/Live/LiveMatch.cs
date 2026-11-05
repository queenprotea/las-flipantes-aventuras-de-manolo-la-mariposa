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
        private readonly TimeSpan _placementTime;

        private CancellationTokenSource? _placementCountdown;
        private DateTime? _placementEndsAtUtc;
        private bool _hasEnded;

        internal LiveMatch(int matchId, MatchTable matchTable, IReadOnlyList<SeatedEntrant> seatedEntrants, TimeProvider timeProvider, IMatchListener matchListener, TimeSpan placementTime)
        {
            MatchId = matchId;
            _matchTable = matchTable;
            _seatedEntrants = seatedEntrants;
            _viewFactory = new MatchViewFactory(seatedEntrants);
            _timeProvider = timeProvider;
            _matchListener = matchListener;
            _placementTime = placementTime;
        }

        internal int MatchId { get; }

        internal void StartPlacement()
        {
            lock (_matchLock)
            {
                RestartPlacementCountdown();
                _matchListener.ReceiveMatchStarted(BuildRecipientViews());
            }
        }

        internal void EndMatch()
        {
            lock (_matchLock)
            {
                _hasEnded = true;
                StopPlacementCountdown();
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
            else if (!HasPlacementExpired())
            {
                PlacementOutcome placementOutcome = placeOnTable(GetSeatNumber(entrantId), destinationCell);
                placementResult = ToPlacementResult(placementOutcome);
            }

            if (placementResult == PlacementResult.Placed)
            {
                RestartPlacementCountdown();
                _matchListener.ReceiveMatchStateChanged(BuildRecipientViews());
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

                _matchTable.ExpirePlacement();
                RestartPlacementCountdown();
                _matchListener.ReceiveMatchStateChanged(BuildRecipientViews());
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
            if (_matchTable.Phase == TablePhase.PlayerTurn)
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
                MatchView matchView = _viewFactory.CreateForSeat(_matchTable, placementTimeLeft, recipient.SeatNumber);
                recipientViews.Add(new MatchRecipientView(recipient.Entrant.EntrantId, matchView));
            }

            return recipientViews;
        }
    }
}
