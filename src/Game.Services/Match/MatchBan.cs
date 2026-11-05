using System;

namespace Game.Services
{
    internal sealed record MatchBan(string DisplayName, bool IsPermanent, TimeSpan TimeLeft);
}
