using System.Collections.Generic;

using Game.Contracts;

namespace Game.Services
{
    internal sealed class RoomChatEntry
    {
        private readonly HashSet<int> _reporterOwnerIds = new HashSet<int>();

        internal RoomChatEntry(ChatMessage message, int authorOwnerId)
        {
            Message = message;
            AuthorOwnerId = authorOwnerId;
        }

        internal ChatMessage Message { get; }

        internal int AuthorOwnerId { get; }

        internal bool WasReportedBy(int reporterOwnerId)
        {
            bool wasReported = _reporterOwnerIds.Contains(reporterOwnerId);

            return wasReported;
        }

        internal void RecordReporter(int reporterOwnerId)
        {
            _reporterOwnerIds.Add(reporterOwnerId);
        }
    }
}
