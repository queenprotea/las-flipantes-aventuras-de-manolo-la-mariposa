using System;

using Torres.ArchitectureSpike.Contracts;

namespace Torres.ArchitectureSpike.Client
{
    internal sealed class ReceivedNotice
    {
        internal ReceivedNotice(MatchUpdate? matchUpdate, PlayerConnectionNotice? connectionNotice)
        {
            MatchUpdate = matchUpdate;
            ConnectionNotice = connectionNotice;
            ArrivalThreadId = Environment.CurrentManagedThreadId;
        }

        internal MatchUpdate? MatchUpdate { get; }

        internal PlayerConnectionNotice? ConnectionNotice { get; }

        internal int ArrivalThreadId { get; }

        internal int ProcessedThreadId { get; private set; }

        internal void MarkProcessed(int loopThreadId)
        {
            ProcessedThreadId = loopThreadId;
        }
    }
}
