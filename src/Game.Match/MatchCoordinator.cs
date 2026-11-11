using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Domain;

namespace Game.Match
{
    public sealed class MatchCoordinator
    {
        private const string SeatedEntrantMessage = "A player cannot take two seats at once.";

        private readonly Lock _coordinatorLock = new Lock();
        private readonly Dictionary<int, LiveMatch> _matchesByEntrantId = new Dictionary<int, LiveMatch>();
        private readonly TimeProvider _timeProvider;
        private readonly IMatchListener _matchListener;
        private readonly StartingLayout _startingLayout;

        private int _lastMatchId;

        public MatchCoordinator(TimeProvider timeProvider, IMatchListener matchListener, IReadOnlyList<MatchCell> startingFlowerCells)
        {
            ArgumentNullException.ThrowIfNull(timeProvider);
            ArgumentNullException.ThrowIfNull(matchListener);
            ArgumentNullException.ThrowIfNull(startingFlowerCells);

            _timeProvider = timeProvider;
            _matchListener = matchListener;
            _startingLayout = new StartingLayout(startingFlowerCells.Select(ToStartingFlower).ToList());
        }

        public static int MinimumEntrantCount => MatchTable.MinimumSeatCount;

        public static int MaximumEntrantCount => MatchTable.MaximumSeatCount;

        public static bool BoardContains(MatchCell examinedCell)
        {
            ArgumentNullException.ThrowIfNull(examinedCell);

            bool isOnBoard = Board.Contains(MatchCellConversion.ToBoardCell(examinedCell));

            return isOnBoard;
        }

        public Task<int> StartMatchAsync(IReadOnlyList<MatchEntrant> entrants, TimeSpan placementTime)
        {
            ArgumentNullException.ThrowIfNull(entrants);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(placementTime, TimeSpan.Zero);

            List<SeatedEntrant> seatedEntrants = SeatActiveEntrants(entrants);
            List<int> entrantIds = seatedEntrants.Select(seatedEntrant => seatedEntrant.Entrant.EntrantId).ToList();
            var matchTable = new MatchTable(_startingLayout, seatedEntrants.Count);
            LiveMatch liveMatch;
            lock (_coordinatorLock)
            {
                bool isAnyEntrantSeated = (entrantIds.Distinct().Count() != entrantIds.Count) || entrantIds.Any(_matchesByEntrantId.ContainsKey);
                if (isAnyEntrantSeated)
                {
                    throw new InvalidOperationException(SeatedEntrantMessage);
                }

                _lastMatchId++;
                liveMatch = new LiveMatch(_lastMatchId, matchTable, seatedEntrants, _timeProvider, _matchListener, placementTime);
                foreach (int entrantId in entrantIds)
                {
                    _matchesByEntrantId.Add(entrantId, liveMatch);
                }

                liveMatch.StartPlacement();
            }

            Task<int> startedMatchId = Task.FromResult(liveMatch.MatchId);

            return startedMatchId;
        }

        public PlacementResult PlaceCaterpillar(int entrantId, MatchCell destinationCell)
        {
            ThrowIfOffBoard(destinationCell);

            LiveMatch? liveMatch = FindMatchOfEntrant(entrantId);
            PlacementResult placementResult = liveMatch is null ? PlacementResult.NotInMatch : liveMatch.PlaceCaterpillar(entrantId, MatchCellConversion.ToBoardCell(destinationCell));

            return placementResult;
        }

        public PlacementResult PlaceButterfly(int entrantId, MatchCell destinationCell)
        {
            ThrowIfOffBoard(destinationCell);

            LiveMatch? liveMatch = FindMatchOfEntrant(entrantId);
            PlacementResult placementResult = liveMatch is null ? PlacementResult.NotInMatch : liveMatch.PlaceButterfly(entrantId, MatchCellConversion.ToBoardCell(destinationCell));

            return placementResult;
        }

        public EndMatchResult EndMatch(int entrantId)
        {
            EndMatchResult endResult = EndMatchResult.NotInMatch;
            lock (_coordinatorLock)
            {
                LiveMatch? liveMatch = _matchesByEntrantId.GetValueOrDefault(entrantId);
                if (liveMatch is not null)
                {
                    liveMatch.EndMatch();
                    List<int> seatedEntrantIds = _matchesByEntrantId.Where(matchByEntrant => matchByEntrant.Value == liveMatch).Select(matchByEntrant => matchByEntrant.Key).ToList();
                    foreach (int seatedEntrantId in seatedEntrantIds)
                    {
                        _matchesByEntrantId.Remove(seatedEntrantId);
                    }

                    endResult = EndMatchResult.Ended;
                }
            }

            return endResult;
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
