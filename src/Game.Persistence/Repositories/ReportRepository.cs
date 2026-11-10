using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace Game.Persistence
{
    public sealed class ReportRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public ReportRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            ArgumentNullException.ThrowIfNull(contextFactory);

            _contextFactory = contextFactory;
        }

        public Task<ReportCreationOutcome> CreateAsync(Report newReport)
        {
            ArgumentNullException.ThrowIfNull(newReport);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newReport.ReporterPlayerId);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newReport.ReportedPlayerId);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newReport.RoomId);
            ArgumentException.ThrowIfNullOrWhiteSpace(newReport.ReportedText);

            Task<ReportCreationOutcome> creation = StoreAsync(newReport);

            return creation;
        }

        public Task<int> CountOccasionsAsync(int reportedPlayerId, DateTime? reportedAfterUtc)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(reportedPlayerId);

            Task<int> occasionCounting = CountDistinctOccasionsAsync(reportedPlayerId, reportedAfterUtc);

            return occasionCounting;
        }

        private async Task<ReportCreationOutcome> StoreAsync(Report newReport)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
            databaseContext.Reports.Add(newReport);
            ReportCreationOutcome creationOutcome = ReportCreationOutcome.Created;
            try
            {
                await databaseContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
            {
                creationOutcome = ReportCreationOutcome.PlayerMissing;
            }

            return creationOutcome;
        }

        private async Task<int> CountDistinctOccasionsAsync(int reportedPlayerId, DateTime? reportedAfterUtc)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
            IQueryable<Report> receivedReports = databaseContext.Reports
                .Where(storedReport => storedReport.ReportedPlayerId == reportedPlayerId);
            if (reportedAfterUtc is DateTime reportedAfterInstantUtc)
            {
                receivedReports = receivedReports.Where(storedReport => storedReport.CreatedAt > reportedAfterInstantUtc);
            }

            int occasionCount = await receivedReports
                .Select(storedReport => new { storedReport.RoomId, storedReport.MatchId })
                .Distinct()
                .CountAsync();

            return occasionCount;
        }
    }
}
