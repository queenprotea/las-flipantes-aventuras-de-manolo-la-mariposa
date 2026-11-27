using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace Game.Persistence
{
    public sealed class ReportRepository : IReportRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public ReportRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public Task<ReportCreationOutcome> CreateAsync(Report newReport)
        {
            Task<ReportCreationOutcome> creation = StoreAsync(newReport);

            return creation;
        }

        public Task<int> CountOccasionsAsync(int reportedPlayerId, DateTime? thresholdAt)
        {
            Task<int> occasionCounting = CountDistinctOccasionsAsync(reportedPlayerId, thresholdAt);

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

        private async Task<int> CountDistinctOccasionsAsync(int reportedPlayerId, DateTime? thresholdAt)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
            IQueryable<Report> receivedReports = databaseContext.Reports
                .Where(storedReport => storedReport.ReportedPlayerId == reportedPlayerId);
            if (thresholdAt is DateTime foundThresholdAt)
            {
                receivedReports = receivedReports.Where(storedReport => storedReport.CreatedAt > foundThresholdAt);
            }

            int occasionCount = await receivedReports
                .Select(storedReport => new { storedReport.RoomId, storedReport.MatchId })
                .Distinct()
                .CountAsync();

            return occasionCount;
        }
    }
}
