using System;

namespace Game.Match
{
    /// <summary>
    /// The clocks of a match: how long each placement or turn lasts, and how long a player who loses the connection during the turn of another player keeps the seat.
    /// </summary>
    public readonly record struct MatchTimeLimits(TimeSpan StepTime, TimeSpan ReconnectionTime);
}
