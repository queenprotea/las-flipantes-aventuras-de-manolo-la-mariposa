using System;
using System.Collections.Generic;

using Game.Persistence;

namespace Game.Match
{
    internal sealed record FinishedMatchRecord(int MatchId, string EndReason, DateTime FinishedAtUtc, IReadOnlyList<MatchParticipant> PlayerResults);
}
