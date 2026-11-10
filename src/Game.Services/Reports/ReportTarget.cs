namespace Game.Services
{
    internal sealed record ReportTarget(int RoomId, int MessageId, int AuthorOwnerId, bool IsAuthorGuest, string MessageText, bool WasReportedByReporter);
}
