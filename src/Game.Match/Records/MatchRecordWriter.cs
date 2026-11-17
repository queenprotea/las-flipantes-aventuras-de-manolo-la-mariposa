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

        private static readonly ILog _log = LogManager.GetLogger(typeof(MatchRecordWriter));

        private readonly Channel<FinishedMatchRecord> _pendingRecords = Channel.CreateUnbounded<FinishedMatchRecord>(new UnboundedChannelOptions { SingleReader = true });
        private readonly IMatchRecordRepository _matchRecordRepository;
        private readonly IMatchParticipantRepository _matchParticipantRepository;
        private readonly TimeProvider _timeProvider;
        private readonly Task _writerTask;

        internal MatchRecordWriter(IMatchRecordRepository matchRecordRepository, IMatchParticipantRepository matchParticipantRepository, TimeProvider timeProvider)
        {
            _matchRecordRepository = matchRecordRepository;
            _matchParticipantRepository = matchParticipantRepository;
            _timeProvider = timeProvider;
            _writerTask = Task.Run(WritePendingAsync);
        }

        internal void RecordFinish(int matchId, string endReason, IReadOnlyList<MatchParticipant> playerResults)
        {
            var finishedMatch = new FinishedMatchRecord(matchId, endReason, _timeProvider.GetUtcNow().UtcDateTime, playerResults);
            _pendingRecords.Writer.TryWrite(finishedMatch);
        }

        internal Task FinishWritingAsync()
        {
            _pendingRecords.Writer.TryComplete();
            Task pendingWriting = _writerTask;

            return pendingWriting;
        }

        private async Task WritePendingAsync()
        {
            await foreach (FinishedMatchRecord finishedMatch in _pendingRecords.Reader.ReadAllAsync())
            {
                await WriteFinishAsync(finishedMatch);
            }
        }

        private async Task WriteFinishAsync(FinishedMatchRecord finishedMatch)
        {
            try
            {
                await _matchParticipantRepository.WriteResultsAsync(finishedMatch.MatchId, finishedMatch.PlayerResults);
                await _matchRecordRepository.FinishAsync(finishedMatch.MatchId, finishedMatch.EndReason, finishedMatch.FinishedAtUtc);
                _log.InfoFormat(FinishWrittenMessage, finishedMatch.MatchId, finishedMatch.EndReason);
            }
            catch (NpgsqlException exception)
            {
                LogWriteFailure(finishedMatch, exception);
            }
            catch (DbUpdateException exception)
            {
                LogWriteFailure(finishedMatch, exception);
            }
        }

        private static void LogWriteFailure(FinishedMatchRecord finishedMatch, Exception exception)
        {
            _log.Error(string.Format(CultureInfo.InvariantCulture, FinishNotWrittenMessage, finishedMatch.MatchId), exception);
        }
    }
}
