using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Domain;
using Game.Persistence;

namespace Game.Match
{
    public sealed class MatchCoordinator
    {
        private const string SeatedEntrantMessage = "A player cannot take two seats at once.";

        private readonly Lock _coordinatorLock = new Lock();
        private readonly Dictionary<int, LiveMatch> _matchesByEntrantId = new Dictionary<int, LiveMatch>();
        private readonly HashSet<int> _reservedEntrantIds = new HashSet<int>();
        private readonly TimeProvider _timeProvider;
        private readonly IMatchListener _matchListener;
        private readonly IMatchRecordRepository _matchRecordRepository;
        private readonly IMatchParticipantRepository _matchParticipantRepository;
        private readonly MatchRecordWriter _matchRecordWriter;
        private readonly StartingLayout _startingLayout;
        private readonly Random _chanceSource;

        public MatchCoordinator(TimeProvider timeProvider, IMatchListener matchListener, IMatchRecordRepository matchRecordRepository, IMatchParticipantRepository matchParticipantRepository, IReadOnlyList<MatchCell> startingFlowerCells, Random chanceSource)
        {
            ArgumentNullException.ThrowIfNull(timeProvider);
            ArgumentNullException.ThrowIfNull(matchListener);
            ArgumentNullException.ThrowIfNull(matchRecordRepository);
            ArgumentNullException.ThrowIfNull(matchParticipantRepository);
            ArgumentNullException.ThrowIfNull(startingFlowerCells);
            ArgumentNullException.ThrowIfNull(chanceSource);

            _timeProvider = timeProvider;
            _matchListener = matchListener;
            _matchRecordRepository = matchRecordRepository;
            _matchParticipantRepository = matchParticipantRepository;
            _matchRecordWriter = new MatchRecordWriter(matchRecordRepository, matchParticipantRepository, timeProvider);
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

        public Task<int> StartMatchAsync(MatchOpening matchOpening, TimeSpan stepTime, MatchRuleSet ruleSet)
        {
            ArgumentNullException.ThrowIfNull(matchOpening);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(matchOpening.RoomId, nameof(matchOpening));
            ArgumentNullException.ThrowIfNull(matchOpening.Entrants, nameof(matchOpening));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(stepTime, TimeSpan.Zero);
            ArgumentNullException.ThrowIfNull(ruleSet);

            Task<int> matchStart = StartRecordedMatchAsync(matchOpening, stepTime, ruleSet);

            return matchStart;
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

        public EndMatchResult EndMatch(int entrantId)
        {
            EndMatchResult endResult = EndMatchResult.NotInMatch;
            lock (_coordinatorLock)
            {
                LiveMatch? liveMatch = _matchesByEntrantId.GetValueOrDefault(entrantId);
                if (liveMatch is not null)
                {
                    liveMatch.Finished -= LiveMatchOnFinished;
                    liveMatch.EndMatch();
                    RemoveEntrantsOf(liveMatch);
                    _matchRecordWriter.RecordFinish(liveMatch.MatchId, liveMatch.ReadEndReason(), liveMatch.ReadPlayerResults());
                    endResult = EndMatchResult.Ended;
                }
            }

            return endResult;
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

        private async Task<int> StartRecordedMatchAsync(MatchOpening matchOpening, TimeSpan stepTime, MatchRuleSet ruleSet)
        {
            MatchRules matchRules = MatchRuleConversion.ToMatchRules(ruleSet);
            List<SeatedEntrant> seatedEntrants = SeatActiveEntrants(matchOpening.Entrants);
            List<int> entrantIds = seatedEntrants.Select(seatedEntrant => seatedEntrant.Entrant.EntrantId).ToList();
            var matchTable = new MatchTable(_startingLayout, seatedEntrants.Count, matchRules, _chanceSource);
            ReserveSeats(entrantIds);
            int matchId;
            try
            {
                matchId = await _matchRecordRepository.CreateAsync(matchOpening.RoomId, seatedEntrants.Count);
                await _matchParticipantRepository.AddAsync(matchId, MatchRecordConversion.ToSeatedPlayers(seatedEntrants));
                var liveMatch = new LiveMatch(matchId, matchTable, seatedEntrants, _timeProvider, _matchListener, stepTime);
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
                bool isAnyEntrantSeated = (entrantIds.Distinct().Count() != entrantIds.Count) || entrantIds.Any(_matchesByEntrantId.ContainsKey) || entrantIds.Any(_reservedEntrantIds.Contains);
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

                _matchListener.ReceiveMatchFinished(releasedEntrantIds);
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
