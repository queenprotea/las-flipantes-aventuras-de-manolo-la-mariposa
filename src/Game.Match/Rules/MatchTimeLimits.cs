using System;

namespace Game.Match
{
    /// <summary>
    /// The clocks of a match: how long each placement or turn lasts, how long a player who loses the connection during the turn of another player keeps the seat and, when it is positive, how many of its own steps in a row a seat may let run out before it is taken for absent.
    /// </summary>
    public readonly record struct MatchTimeLimits(TimeSpan StepTime, TimeSpan ReconnectionTime, int AbsentAfterExpiredSteps = 0);
}
