using System;

using Game.Contracts;

namespace Torres.Client.Session
{
    internal sealed record MatchChange(MatchChangeKind Kind, MatchView MatchView, DateTime ReceivedAtUtc);
}
