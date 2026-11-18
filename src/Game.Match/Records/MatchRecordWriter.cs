using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Channels;
using System.Threading.Tasks;

using Game.Persistence;

using log4net;

using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace Game.Match
{
    internal sealed class MatchRecordWriter
    {
        private const string FinishWrittenMessage = "Match {0} was recorded as {1}.";
        private const string FinishNotWrittenMessage = "The result of match {0} could not be written; the match stays in progress in the database.";
        private const string CheckpointNotWrittenMessage = "The state of match {0} could not be saved; after a server restart the match would go on from an older step.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(MatchRecordWriter));

        private readonly Channel<Func<Task>> _pendingWrites = Channel.CreateUnbounded<Func<Task>>(new UnboundedChannelOptions { SingleReader = true });
        private readonly IMatchRecordRepository _matchRecordRepository;
        private readonly IMatchParticipantRepository _matchParticipantRepository;
        private readonly IMatchStateRepository _matchStateRepository;
        private readonly TimeProvider _timeProvider;
        private readonly Task _writerTask;

        internal MatchRecordWriter(MatchRepositories matchRepositories, TimeProvider timeProvider)
        {
            _matchRecordRepository = matchRepositories.MatchRecordRepository;
            _matchParticipantRepository = matchRepositories.MatchParticipantRepository;
            _matchStateRepository = matchRepositories.MatchStateRepository;
            _timeProvider = timeProvider;
            _writerTask = Task.Run(WritePendingAsync);
        }

        internal void RecordFinish(int matchId, string endReason, IReadOnlyList<MatchParticipant> playerResults)
        {
            var finishedMatch = new FinishedMatchRecord(matchId, endReason, _timeProvider.GetUtcNow().UtcDateTime, playerResults);
            _pendingWrites.Writer.TryWrite(() => WriteFinishAsync(finishedMatch));
        }

        internal void RecordCheckpoint(int matchId, MatchCheckpoint checkpoint)
        {
            var savedState = new MatchState
            {
                MatchId = matchId,
                RoundNumber = (short)checkpoint.RoundNumber,
                TurnNumber = (short)checkpoint.TurnRoundNumber,
                ActiveSeatNumber = (short)checkpoint.ActiveSeatNumber,
                StateDocument = MatchCheckpointDocument.Write(checkpoint),
                SavedAt = _timeProvider.GetUtcNow().UtcDateTime,
            };
            _pendingWrites.Writer.TryWrite(() => WriteCheckpointAsync(savedState));
        }

        internal Task FinishWritingAsync()
        {
            _pendingWrites.Writer.TryComplete();
            Task pendingWriting = _writerTask;

            return pendingWriting;
        }

        private async Task WritePendingAsync()
        {
            await foreach (Func<Task> pendingWrite in _pendingWrites.Reader.ReadAllAsync())
            {
                await pendingWrite();
            }
        }

        private async Task WriteFinishAsync(FinishedMatchRecord finishedMatch)
        {
            try
            {
                await _matchParticipantRepository.WriteResultsAsync(finishedMatch.MatchId, finishedMatch.PlayerResults);
                await _matchRecordRepository.FinishAsync(finishedMatch.MatchId, finishedMatch.EndReason, finishedMatch.FinishedAtUtc);
                await _matchStateRepository.DeleteAsync(finishedMatch.MatchId);
                _log.InfoFormat(FinishWrittenMessage, finishedMatch.MatchId, finishedMatch.EndReason);
            }
            catch (NpgsqlException exception)
            {
                LogWriteFailure(FinishNotWrittenMessage, finishedMatch.MatchId, exception);
            }
            catch (DbUpdateException exception)
            {
                LogWriteFailure(FinishNotWrittenMessage, finishedMatch.MatchId, exception);
            }
        }

        private async Task WriteCheckpointAsync(MatchState savedState)
        {
            try
            {
                await _matchStateRepository.SaveAsync(savedState);
            }
            catch (NpgsqlException exception)
            {
                LogWriteFailure(CheckpointNotWrittenMessage, savedState.MatchId, exception);
            }
            catch (DbUpdateException exception)
            {
                LogWriteFailure(CheckpointNotWrittenMessage, savedState.MatchId, exception);
            }
        }

        private static void LogWriteFailure(string failureMessage, int matchId, Exception exception)
        {
            _log.Error(string.Format(CultureInfo.InvariantCulture, failureMessage, matchId), exception);
        }
    }
}
