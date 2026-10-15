using System;
using System.Collections.Generic;

using Torres.ArchitectureSpike.Contracts;

namespace Torres.ArchitectureSpike.Match
{
    public sealed class MatchCoordinator
    {
        private readonly object _lobbyLock = new object();
        private readonly Dictionary<string, LiveMatch> _matchesByPlayer = new Dictionary<string, LiveMatch>(StringComparer.Ordinal);
        private readonly MatchEnvironment _environment;

        private string? _waitingPlayerName;

        public MatchCoordinator(IMatchNotificationReceiver notificationReceiver, IMatchClock matchClock, MatchSettings matchSettings)
        {
            ArgumentNullException.ThrowIfNull(notificationReceiver);
            ArgumentNullException.ThrowIfNull(matchClock);
            ArgumentNullException.ThrowIfNull(matchSettings);

            _environment = new MatchEnvironment(notificationReceiver, matchClock, matchSettings);
        }

        public MatchEntryStatus Join(string playerName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(playerName);

            LiveMatch? startedMatch = null;
            MatchEntryStatus entryStatus;
            lock (_lobbyLock)
            {
                if (_matchesByPlayer.ContainsKey(playerName) || (playerName == _waitingPlayerName))
                {
                    entryStatus = MatchEntryStatus.AlreadyInMatch;
                }
                else if (_waitingPlayerName is string waitingPlayerName)
                {
                    startedMatch = CreateMatch(waitingPlayerName, playerName);
                    _waitingPlayerName = null;
                    entryStatus = MatchEntryStatus.MatchStarted;
                }
                else
                {
                    _waitingPlayerName = playerName;
                    entryStatus = MatchEntryStatus.WaitingForOpponent;
                }
            }

            startedMatch?.Start();

            return entryStatus;
        }

        public ActionResult Advance(string playerName, int steps)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(playerName);

            LiveMatch? playerMatch;
            lock (_lobbyLock)
            {
                _matchesByPlayer.TryGetValue(playerName, out playerMatch);
            }

            ActionResult actionResult = ActionResult.NotInMatch;
            if (playerMatch is not null)
            {
                actionResult = playerMatch.Advance(playerName, steps);
            }

            return actionResult;
        }

        private LiveMatch CreateMatch(string firstPlayerName, string secondPlayerName)
        {
            string[] playerNames = { firstPlayerName, secondPlayerName };
            var liveMatch = new LiveMatch(playerNames, _environment);
            _matchesByPlayer.Add(firstPlayerName, liveMatch);
            _matchesByPlayer.Add(secondPlayerName, liveMatch);

            return liveMatch;
        }
    }
}
