using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using CoreWCF;

using Game.Contracts;
using Game.Persistence;

using log4net;

using Npgsql;

namespace Game.Services
{
    public sealed class HistoryService : IHistoryService
    {
        private const string HistoryFeatureName = "the match history";
        private const string HistoryUnavailableMessage = "The match history is not available right now.";
        private const string MatchDetailUnavailableMessage = "The match is not available right now.";
        private const string HistoryReadFailedMessage = "The database could not be read while listing the match history of player {0}.";
        private const string MatchDetailReadFailedMessage = "The database could not be read while player {0} asked for match {1}.";
        private const string HistoryPageServedMessage = "Player {0} read page {1} of {2} of the match history, with {3} matches.";
        private const string MatchNotShownMessage = "Player {0} asked for match {1}, which was not shown: {2}.";

        private const int FirstPageNumber = 1;
        private const int NoGuests = 0;
        private const int UnplacedSortKey = int.MaxValue;

        private static readonly ILog _log = LogManager.GetLogger(typeof(HistoryService));

        private readonly SessionRegistry _sessionRegistry;
        private readonly MatchRecordRepository _matchRecordRepository;
        private readonly MatchParticipantRepository _matchParticipantRepository;
        private readonly PlayerRepository _playerRepository;
        private readonly HistoryPaging _historyPaging;

        public HistoryService(SessionRegistry sessionRegistry, MatchRecordRepository matchRecordRepository, MatchParticipantRepository matchParticipantRepository, PlayerRepository playerRepository, HistoryPaging historyPaging)
        {
            ArgumentNullException.ThrowIfNull(sessionRegistry);
            ArgumentNullException.ThrowIfNull(matchRecordRepository);
            ArgumentNullException.ThrowIfNull(matchParticipantRepository);
            ArgumentNullException.ThrowIfNull(playerRepository);
            ArgumentNullException.ThrowIfNull(historyPaging);

            _sessionRegistry = sessionRegistry;
            _matchRecordRepository = matchRecordRepository;
            _matchParticipantRepository = matchParticipantRepository;
            _playerRepository = playerRepository;
            _historyPaging = historyPaging;
        }

        public async Task<MatchHistoryResponse> GetMatchHistoryAsync(int pageNumber)
        {
            int callerPlayerId = AccountOwnerGuard.RequireAccountOwner(_sessionRegistry, _log, HistoryFeatureName).OwnerId;
            MatchHistoryResponse historyResponse;
            try
            {
                historyResponse = await ReadHistoryPageAsync(callerPlayerId, pageNumber);
            }
            catch (NpgsqlException exception)
            {
                string failureMessage = string.Format(CultureInfo.InvariantCulture, HistoryReadFailedMessage, callerPlayerId);
                _log.Error(failureMessage, exception);
                throw new FaultException(HistoryUnavailableMessage);
            }

            _log.DebugFormat(CultureInfo.InvariantCulture, HistoryPageServedMessage, callerPlayerId, historyResponse.PageNumber, historyResponse.PageCount, historyResponse.Entries.Count);

            return historyResponse;
        }

        public async Task<MatchDetailResponse> GetMatchDetailAsync(int matchId)
        {
            int callerPlayerId = AccountOwnerGuard.RequireAccountOwner(_sessionRegistry, _log, HistoryFeatureName).OwnerId;
            MatchDetailResponse detailResponse;
            try
            {
                detailResponse = await ReadMatchDetailAsync(matchId, callerPlayerId);
            }
            catch (NpgsqlException exception)
            {
                string failureMessage = string.Format(CultureInfo.InvariantCulture, MatchDetailReadFailedMessage, callerPlayerId, matchId);
                _log.Error(failureMessage, exception);
                throw new FaultException(MatchDetailUnavailableMessage);
            }

            if (detailResponse.Status != MatchDetailStatus.Found)
            {
                _log.WarnFormat(CultureInfo.InvariantCulture, MatchNotShownMessage, callerPlayerId, matchId, detailResponse.Status);
            }

            return detailResponse;
        }

        private async Task<MatchHistoryResponse> ReadHistoryPageAsync(int callerPlayerId, int pageNumber)
        {
            int matchCount = await _matchRecordRepository.CountFinishedByPlayerAsync(callerPlayerId);
            int fullPageCount = matchCount / _historyPaging.MatchesPerPage;
            int leftoverCount = matchCount % _historyPaging.MatchesPerPage;
            int pageCount = leftoverCount == 0 ? fullPageCount : fullPageCount + 1;
            var historyResponse = new MatchHistoryResponse
            {
                PageNumber = FirstPageNumber,
                PageCount = pageCount,
            };
            if (matchCount == 0)
            {
                return historyResponse;
            }

            historyResponse.PageNumber = Math.Clamp(pageNumber, FirstPageNumber, pageCount);
            int skippedCount = (historyResponse.PageNumber - FirstPageNumber) * _historyPaging.MatchesPerPage;
            List<MatchRecord> pageMatches = await _matchRecordRepository.ListFinishedPageByPlayerAsync(callerPlayerId, skippedCount, _historyPaging.MatchesPerPage);
            List<PlayedMatch> playedMatches = await AttachParticipantsAsync(pageMatches);
            historyResponse.Entries = playedMatches
                .Select(playedMatch => ToHistoryEntry(playedMatch, callerPlayerId))
                .ToList();

            return historyResponse;
        }

        private async Task<MatchDetailResponse> ReadMatchDetailAsync(int matchId, int callerPlayerId)
        {
            MatchRecord? finishedMatch = await FindFinishedMatchAsync(matchId);
            var detailResponse = new MatchDetailResponse { Status = MatchDetailStatus.NotFound };
            if (finishedMatch is null)
            {
                return detailResponse;
            }

            List<PlayedMatch> playedMatches = await AttachParticipantsAsync(new List<MatchRecord> { finishedMatch });
            detailResponse = ToDetailResponse(playedMatches[0], callerPlayerId);

            return detailResponse;
        }

        private Task<MatchRecord?> FindFinishedMatchAsync(int matchId)
        {
            Task<MatchRecord?> finding = Task.FromResult<MatchRecord?>(null);
            if (matchId > 0)
            {
                finding = _matchRecordRepository.FindFinishedAsync(matchId);
            }

            return finding;
        }

        private async Task<List<PlayedMatch>> AttachParticipantsAsync(List<MatchRecord> matches)
        {
            List<int> matchIds = matches.Select(match => match.MatchId).ToList();
            List<MatchParticipant> participations = await _matchParticipantRepository.ListByMatchesAsync(matchIds);
            List<int> playerIds = participations.Select(participation => participation.PlayerId).Distinct().ToList();
            List<Player> players = await _playerRepository.ListByIdsAsync(playerIds);

            Dictionary<int, string> usernamesByPlayerId = players.ToDictionary(player => player.PlayerId, player => player.Username);
            ILookup<int, NamedParticipant> participantsByMatchId = participations
                .Where(participation => usernamesByPlayerId.ContainsKey(participation.PlayerId))
                .Select(participation => new NamedParticipant(participation, usernamesByPlayerId[participation.PlayerId]))
                .ToLookup(namedParticipant => namedParticipant.Participation.MatchId);
            List<PlayedMatch> playedMatches = matches
                .Select(match => new PlayedMatch(match, OrderByPlacement(participantsByMatchId[match.MatchId])))
                .ToList();

            return playedMatches;
        }

        private static List<NamedParticipant> OrderByPlacement(IEnumerable<NamedParticipant> namedParticipants)
        {
            List<NamedParticipant> orderedParticipants = namedParticipants
                .OrderBy(namedParticipant => namedParticipant.Participation.FinalPosition ?? UnplacedSortKey)
                .ThenBy(namedParticipant => namedParticipant.Participation.SeatNumber)
                .ToList();

            return orderedParticipants;
        }

        private static MatchHistoryEntry ToHistoryEntry(PlayedMatch playedMatch, int callerPlayerId)
        {
            MatchParticipant ownParticipation = playedMatch.Participants
                .Single(participant => participant.Participation.PlayerId == callerPlayerId)
                .Participation;
            List<string> rivalUsernames = playedMatch.Participants
                .Where(participant => participant.Participation.PlayerId != callerPlayerId)
                .Select(participant => participant.Username)
                .ToList();

            var historyEntry = new MatchHistoryEntry
            {
                MatchId = playedMatch.Match.MatchId,
                PlayedAt = ReadFinishedAt(playedMatch.Match),
                PlayerCount = playedMatch.Match.PlayerCount,
                RivalUsernames = rivalUsernames,
                Outcome = ToOutcome(playedMatch.Match.EndReason),
                Placement = ownParticipation.FinalPosition,
                Points = ownParticipation.FinalScore,
                WasWithdrawn = ownParticipation.WasWithdrawn,
            };

            return historyEntry;
        }

        private static MatchDetailResponse ToDetailResponse(PlayedMatch playedMatch, int callerPlayerId)
        {
            bool isCallerParticipant = playedMatch.Participants.Any(participant => participant.Participation.PlayerId == callerPlayerId);
            var detailResponse = new MatchDetailResponse { Status = MatchDetailStatus.NotYours };
            if (!isCallerParticipant)
            {
                return detailResponse;
            }

            MatchRecord finishedMatch = playedMatch.Match;
            DateTime finishedAt = ReadFinishedAt(finishedMatch);
            MatchOutcome outcome = ToOutcome(finishedMatch.EndReason);
            int? durationSeconds = null;
            if (outcome != MatchOutcome.Interrupted)
            {
                durationSeconds = (int)(finishedAt - finishedMatch.StartedAt).TotalSeconds;
            }

            detailResponse.Status = MatchDetailStatus.Found;
            detailResponse.PlayedAt = finishedAt;
            detailResponse.PlayerCount = finishedMatch.PlayerCount;
            detailResponse.DurationSeconds = durationSeconds;
            detailResponse.Outcome = outcome;
            detailResponse.Results = playedMatch.Participants
                .Where(participant => participant.Participation.FinalPosition.HasValue && participant.Participation.FinalScore.HasValue)
                .Select(participant => ToResultEntry(participant, callerPlayerId))
                .ToList();
            detailResponse.GuestCount = Math.Max(NoGuests, finishedMatch.PlayerCount - playedMatch.Participants.Count);

            return detailResponse;
        }

        private static MatchResultEntry ToResultEntry(NamedParticipant namedParticipant, int callerPlayerId)
        {
            MatchParticipant participation = namedParticipant.Participation;
            var resultEntry = new MatchResultEntry
            {
                Placement = participation.FinalPosition.GetValueOrDefault(),
                Username = namedParticipant.Username,
                Points = participation.FinalScore.GetValueOrDefault(),
                IsCurrentPlayer = participation.PlayerId == callerPlayerId,
                WasWithdrawn = participation.WasWithdrawn,
            };

            return resultEntry;
        }

        private static DateTime ReadFinishedAt(MatchRecord finishedMatch)
        {
            DateTime finishedAt = finishedMatch.FinishedAt.GetValueOrDefault(finishedMatch.StartedAt);

            return finishedAt;
        }

        private static MatchOutcome ToOutcome(string? endReason)
        {
            MatchOutcome outcome = endReason switch
            {
                MatchEndReasonCodes.Completed => MatchOutcome.Finished,
                MatchEndReasonCodes.Abandoned => MatchOutcome.Abandoned,
                MatchEndReasonCodes.Interrupted => MatchOutcome.Interrupted,
                _ => throw new ArgumentOutOfRangeException(nameof(endReason)),
            };

            return outcome;
        }
    }
}
