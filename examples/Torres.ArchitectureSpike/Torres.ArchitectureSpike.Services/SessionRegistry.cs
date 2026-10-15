using System;
using System.Collections.Generic;
using System.Linq;

using log4net;

using Torres.ArchitectureSpike.Contracts;
using Torres.ArchitectureSpike.Match;

namespace Torres.ArchitectureSpike.Services
{
    public sealed class SessionRegistry : IMatchNotificationReceiver
    {
        private const string SessionOpenedMessage = "Session of {0} opened.";
        private const string SessionLostMessage = "Session of {0} declared lost: {1}.";
        private const string DeliveryDiscardedMessage = "Match update for {0} discarded: no live session.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(SessionRegistry));

        private readonly object _registryLock = new object();
        private readonly Dictionary<string, PlayerSession> _sessionsByPlayer = new Dictionary<string, PlayerSession>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _playersBySessionId = new Dictionary<string, string>(StringComparer.Ordinal);

        public ConnectStatus Register(string playerName, IGameSessionCallback callbackChannel, string sessionId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(playerName);
            ArgumentNullException.ThrowIfNull(callbackChannel);
            ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

            PlayerSession? openedSession = null;
            lock (_registryLock)
            {
                if (!_sessionsByPlayer.ContainsKey(playerName))
                {
                    openedSession = new PlayerSession(playerName, callbackChannel, this);
                    _sessionsByPlayer.Add(playerName, openedSession);
                    _playersBySessionId.Add(sessionId, playerName);
                }
            }

            ConnectStatus connectStatus = ConnectStatus.NameInUse;
            if (openedSession is not null)
            {
                openedSession.Watch();
                _log.InfoFormat(SessionOpenedMessage, playerName);
                BroadcastConnectionChange(playerName, PlayerConnectionChange.Connected);
                connectStatus = ConnectStatus.Connected;
            }

            return connectStatus;
        }

        public string? FindPlayerName(string sessionId)
        {
            string? playerName;
            lock (_registryLock)
            {
                _playersBySessionId.TryGetValue(sessionId, out playerName);
            }

            return playerName;
        }

        public void Deliver(string playerName, MatchUpdate update)
        {
            PlayerSession? playerSession;
            lock (_registryLock)
            {
                _sessionsByPlayer.TryGetValue(playerName, out playerSession);
            }

            if (playerSession is null)
            {
                _log.InfoFormat(DeliveryDiscardedMessage, playerName);
                return;
            }

            playerSession.Enqueue(SessionDelivery.ForMatchUpdate(update));
        }

        internal void DeclareLost(PlayerSession lostSession, PlayerConnectionChange change)
        {
            lock (_registryLock)
            {
                _sessionsByPlayer.Remove(lostSession.PlayerName);
                string[] lostSessionIds = _playersBySessionId
                    .Where(sessionEntry => sessionEntry.Value == lostSession.PlayerName)
                    .Select(sessionEntry => sessionEntry.Key)
                    .ToArray();
                foreach (string lostSessionId in lostSessionIds)
                {
                    _playersBySessionId.Remove(lostSessionId);
                }
            }

            lostSession.StopSending();
            _log.InfoFormat(SessionLostMessage, lostSession.PlayerName, change);
            BroadcastConnectionChange(lostSession.PlayerName, change);
        }

        private void BroadcastConnectionChange(string playerName, PlayerConnectionChange change)
        {
            var connectionNotice = new PlayerConnectionNotice
            {
                PlayerName = playerName,
                Change = change,
            };
            List<PlayerSession> otherSessions;
            lock (_registryLock)
            {
                otherSessions = _sessionsByPlayer.Values.Where(session => session.PlayerName != playerName).ToList();
            }

            foreach (PlayerSession otherSession in otherSessions)
            {
                otherSession.Enqueue(SessionDelivery.ForConnectionNotice(connectionNotice));
            }
        }
    }
}
