using System;

namespace Game.Persistence
{
    public sealed class Report
    {
        public int ReportId { get; set; }

        public int ReporterPlayerId { get; set; }

        public int ReportedPlayerId { get; set; }

        public int RoomId { get; set; }

        public int? MatchId { get; set; }

        public string ReportedText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
