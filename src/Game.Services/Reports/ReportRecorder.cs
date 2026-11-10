using System;
using System.Globalization;
using System.Threading.Tasks;

using Game.Persistence;

using log4net;

using Npgsql;

namespace Game.Services
{
    public sealed class ReportRecorder
    {
        private const string NotStartedMessage = "The sanction policy was not set; Start must run before reports are recorded.";
        private const string ReportRecordedMessage = "Player {0} reported a message of player {1} in room {2}.";
        private const string TemporaryBanAppliedMessage = "Player {0} received a ban of level {1} until {2:O}.";
        private const string PermanentBanAppliedMessage = "Player {0} received a permanent ban.";
        private const string SanctionFailedMessage = "The database could not be used while judging the sanction of player {0}; the next report judges it again.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(ReportRecorder));

        private readonly ReportRepository _reportRepository;
        private readonly SanctionRepository _sanctionRepository;
        private readonly TimeProvider _timeProvider;

        private SanctionPolicy? _sanctionPolicy;

        public ReportRecorder(ReportRepository reportRepository, SanctionRepository sanctionRepository, TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(reportRepository);
            ArgumentNullException.ThrowIfNull(sanctionRepository);
            ArgumentNullException.ThrowIfNull(timeProvider);

            _reportRepository = reportRepository;
            _sanctionRepository = sanctionRepository;
            _timeProvider = timeProvider;
        }

        public void Start(SanctionPolicy sanctionPolicy)
        {
            ArgumentNullException.ThrowIfNull(sanctionPolicy);

            _sanctionPolicy = sanctionPolicy;
        }

        internal async Task<bool> RecordReportAsync(int reporterOwnerId, ReportTarget reportTarget)
        {
            SanctionPolicy sanctionPolicy = RequirePolicy();
            var newReport = new Report
            {
                ReporterPlayerId = reporterOwnerId,
                ReportedPlayerId = reportTarget.AuthorOwnerId,
                RoomId = reportTarget.RoomId,
                ReportedText = reportTarget.MessageText,
                CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            };
            ReportCreationOutcome creationOutcome = await _reportRepository.CreateAsync(newReport);
            bool wasRecorded = creationOutcome == ReportCreationOutcome.Created;
            if (wasRecorded)
            {
                _log.InfoFormat(ReportRecordedMessage, reporterOwnerId, reportTarget.AuthorOwnerId, reportTarget.RoomId);
                await JudgeSanctionAfterReportAsync(newReport, sanctionPolicy);
            }

            return wasRecorded;
        }

        private async Task JudgeSanctionAfterReportAsync(Report recordedReport, SanctionPolicy sanctionPolicy)
        {
            try
            {
                await ApplySanctionIfThresholdReachedAsync(recordedReport, sanctionPolicy);
            }
            catch (NpgsqlException exception)
            {
                _log.Error(string.Format(CultureInfo.InvariantCulture, SanctionFailedMessage, recordedReport.ReportedPlayerId), exception);
            }
        }

        private async Task ApplySanctionIfThresholdReachedAsync(Report recordedReport, SanctionPolicy sanctionPolicy)
        {
            int reportedPlayerId = recordedReport.ReportedPlayerId;
            DateTime? latestThresholdAtUtc = await _sanctionRepository.FindLatestThresholdAsync(reportedPlayerId);
            int occasionCount = await _reportRepository.CountOccasionsAsync(reportedPlayerId, latestThresholdAtUtc);
            if (occasionCount < sanctionPolicy.ThresholdOccasions)
            {
                return;
            }

            int previousSanctionCount = await _sanctionRepository.CountAsync(reportedPlayerId);
            Sanction newSanction = sanctionPolicy.BuildSanction(reportedPlayerId, previousSanctionCount, recordedReport.CreatedAt);
            await _sanctionRepository.CreateAsync(newSanction);
            LogAppliedSanction(newSanction);
        }

        private static void LogAppliedSanction(Sanction appliedSanction)
        {
            if (appliedSanction.IsPermanent)
            {
                _log.InfoFormat(PermanentBanAppliedMessage, appliedSanction.PlayerId);
            }
            else
            {
                _log.InfoFormat(TemporaryBanAppliedMessage, appliedSanction.PlayerId, appliedSanction.Level, appliedSanction.EffectiveUntil);
            }
        }

        private SanctionPolicy RequirePolicy()
        {
            SanctionPolicy sanctionPolicy = _sanctionPolicy ?? throw new InvalidOperationException(NotStartedMessage);

            return sanctionPolicy;
        }
    }
}
