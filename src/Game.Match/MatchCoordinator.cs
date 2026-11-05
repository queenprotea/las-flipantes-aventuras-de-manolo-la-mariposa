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
        private readonly IMatchListener _listener;
        private readonly StartingLayout _startingLayout;

        private int _lastMatchId;

        public MatchCoordinator(TimeProvider timeProvider, IMatchListener listener, IReadOnlyList<MatchCell> startingFlowerCells)
        {
            ArgumentNullException.ThrowIfNull(timeProvider);
            ArgumentNullException.ThrowIfNull(listener);
            ArgumentNullException.ThrowIfNull(startingFlowerCells);

            _timeProvider = timeProvider;
            _listener = listener;
            _startingLayout = new StartingLayout(startingFlowerCells.Select(ToStartingFlower).ToList());
        }

        public static int MinimumEntrantCount => MatchTable.MinimumSeatCount;

        public static int MaximumEntrantCount => MatchTable.MaximumSeatCount;

        public static bool BoardContains(MatchCell cell)
        {
            ArgumentNullException.ThrowIfNull(cell);

            bool isOnBoard = Board.Contains(MatchCellConversion.ToBoardCell(cell));

            return isOnBoard;
        }

        public Task<int> StartMatchAsync(IReadOnlyList<MatchEntrant> entrants, TimeSpan placementTime)
        {
            ArgumentNullException.ThrowIfNull(entrants);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(placementTime, TimeSpan.Zero);

            List<SeatedEntrant> seatedEntrants = SeatActiveEntrants(entrants);
            List<int> entrantIds = seatedEntrants.Select(seatedEntrant => seatedEntrant.Entrant.EntrantId).ToList();
            var table = new MatchTable(_startingLayout, seatedEntrants.Count);
            LiveMatch match;
            lock (_coordinatorLock)
            {
                bool isAnyEntrantSeated = (entrantIds.Distinct().Count() != entrantIds.Count) || entrantIds.Any(_matchesByEntrantId.ContainsKey);
                if (isAnyEntrantSeated)
                {
                    throw new InvalidOperationException(SeatedEntrantMessage);
                }

                _lastMatchId++;
                match = new LiveMatch(_lastMatchId, table, seatedEntrants, _timeProvider, _listener, placementTime);
                foreach (int entrantId in entrantIds)
                {
                    _matchesByEntrantId.Add(entrantId, match);
                }

                match.Start();
            }

            Task<int> matchIdTask = Task.FromResult(match.MatchId);

            return matchIdTask;
        }

        public PlacementResult PlaceCaterpillar(int entrantId, MatchCell cell)
        {
            ThrowIfOffBoard(cell);

            LiveMatch? match = FindMatchOf(entrantId);
            PlacementResult placementResult = match is null ? PlacementResult.NotInMatch : match.PlaceCaterpillar(entrantId, MatchCellConversion.ToBoardCell(cell));

            return placementResult;
        }

        public PlacementResult PlaceButterfly(int entrantId, MatchCell cell)
        {
            ThrowIfOffBoard(cell);

            LiveMatch? match = FindMatchOf(entrantId);
            PlacementResult placementResult = match is null ? PlacementResult.NotInMatch : match.PlaceButterfly(entrantId, MatchCellConversion.ToBoardCell(cell));

            return placementResult;
        }

        public EndMatchResult EndMatch(int entrantId)
        {
            EndMatchResult endResult = EndMatchResult.NotInMatch;
            lock (_coordinatorLock)
            {
                if (_matchesByEntrantId.TryGetValue(entrantId, out LiveMatch? match))
                {
                    match.End();
                    List<int> seatedEntrantIds = _matchesByEntrantId.Where(seat => seat.Value == match).Select(seat => seat.Key).ToList();
                    foreach (int seatedEntrantId in seatedEntrantIds)
                    {
                        _matchesByEntrantId.Remove(seatedEntrantId);
                    }

                    endResult = EndMatchResult.Ended;
                }
            }

            return endResult;
        }

        private static void ThrowIfOffBoard(MatchCell cell)
        {
            ArgumentNullException.ThrowIfNull(cell);
            ArgumentOutOfRangeException.ThrowIfNegative(cell.Row, nameof(cell));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(cell.Row, Board.Size, nameof(cell));
            ArgumentOutOfRangeException.ThrowIfNegative(cell.Column, nameof(cell));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(cell.Column, Board.Size, nameof(cell));
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

        private static Flower ToStartingFlower(MatchCell cell)
        {
            var flower = new Flower(MatchCellConversion.ToBoardCell(cell), StartingLayout.StartingFlowerHeight);

            return flower;
        }

        private LiveMatch? FindMatchOf(int entrantId)
        {
            LiveMatch? match;
            lock (_coordinatorLock)
            {
                _matchesByEntrantId.TryGetValue(entrantId, out match);
            }

            return match;
        }
    }
}
