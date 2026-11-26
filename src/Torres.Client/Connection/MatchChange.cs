using System;

using Game.Contracts;

namespace Torres.Client.Connection
{
    internal sealed record MatchChange(MatchChangeKind Kind, MatchView MatchView, DateTime ReceivedAtUtc);
}
