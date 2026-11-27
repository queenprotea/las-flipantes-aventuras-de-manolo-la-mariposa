using System;
using System.Threading.Tasks;

namespace Game.Persistence
{
    public interface IReportRepository
    {
        Task<ReportCreationOutcome> CreateAsync(Report newReport);

        Task<int> CountOccasionsAsync(int reportedPlayerId, DateTime? reportedAfterUtc);
    }
}
