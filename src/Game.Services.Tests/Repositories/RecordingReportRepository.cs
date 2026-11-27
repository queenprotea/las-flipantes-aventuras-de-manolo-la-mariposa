using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Services.Tests
{
    internal sealed class RecordingReportRepository : IReportRepository
    {
        internal List<Report> Reports { get; } = new List<Report>();

        public Task<ReportCreationOutcome> CreateAsync(Report newReport)
        {
            Reports.Add(newReport);
            Task<ReportCreationOutcome> creation = Task.FromResult(ReportCreationOutcome.Created);

            return creation;
        }

        public Task<int> CountOccasionsAsync(int reportedPlayerId, DateTime? reportedAfterUtc)
        {
            int occasionCount = Reports.Count(report => (report.ReportedPlayerId == reportedPlayerId) && ((reportedAfterUtc is null) || (report.CreatedAt > reportedAfterUtc)));
            Task<int> counting = Task.FromResult(occasionCount);

            return counting;
        }
    }
}
