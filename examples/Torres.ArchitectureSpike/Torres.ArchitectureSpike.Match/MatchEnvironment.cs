using System;

using Torres.ArchitectureSpike.Domain;

namespace Torres.ArchitectureSpike.Match
{
    internal sealed class MatchEnvironment
    {
        internal MatchEnvironment(IMatchNotificationReceiver notificationReceiver, IMatchClock matchClock, MatchSettings matchSettings)
        {
            NotificationReceiver = notificationReceiver;
            Clock = matchClock;
            TurnDuration = TimeSpan.FromSeconds(matchSettings.TurnSeconds);
            Rules = new TokenRaceRules(matchSettings.GoalPosition, matchSettings.MaximumStepsPerTurn);
        }

        internal IMatchNotificationReceiver NotificationReceiver { get; }

        internal IMatchClock Clock { get; }

        internal TimeSpan TurnDuration { get; }

        internal TokenRaceRules Rules { get; }
    }
}
