using System;
using System.Collections.Generic;
using System.Threading;

using CoreWCF;

using Game.Contracts;

using log4net;

namespace Game.Services
{
    public sealed class SessionRegistry
    {
        private const string SessionOpenedMessage = "Session of player {0} opened.";
        private const string SessionReplacedMessage = "Session of player {0} replaced by a newer one.";
        private const string SessionClosedMessage = "Session of player {0} closed by the player.";
        private const string SessionLostMessage = "Session of player {0} lost.";
        private const string SeatAbsentMessage = "Session of player {0} declared lost: the seat let its match steps run out.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(SessionRegistry));

        private readonly Lock _registryLock = new Lock();
        private readonly Dictionary<string, PlayerSession> _sessionsById = new Dictionary<string, PlayerSession>(StringComparer.Ordinal);
        private readonly Dictionary<int, PlayerSession> _sessionsByOwnerId = new Dictionary<int, PlayerSession>();

        public event EventHandler<SessionOwnerEventArgs>? SessionOpened;

        public event EventHandler<SessionOwnerEventArgs>? SessionClosed;

        public event EventHandler<SessionOwnerEventArgs>? SessionLost;

        internal PlayerSession Open(SessionOwner owner)
        {
            OperationContext callContext = OperationContext.Current;
            IGameSessionNoticeReceiver noticeReceiver = callContext.GetCallbackChannel<IGameSessionNoticeReceiver>();
            var openedSession = new PlayerSession(owner, callContext.SessionId, noticeReceiver, this);
            PlayerSession? replacedSession;
            lock (_registryLock)
            {
                _sessionsByOwnerId.TryGetValue(owner.OwnerId, out replacedSession);
                if (replacedSession is not null)
                {
                    _sessionsById.Remove(replacedSession.SessionId);
                }

                _sessionsByOwnerId[owner.OwnerId] = openedSession;
                _sessionsById[openedSession.SessionId] = openedSession;
            }

            EndReplacedSession(replacedSession, openedSession);
            openedSession.Watch();
            _log.InfoFormat(SessionOpenedMessage, owner.OwnerId);
            OnSessionOpened(new SessionOwnerEventArgs(owner.OwnerId));

            return openedSession;
        }

        internal PlayerSession? FindCaller()
        {
            string? callerSessionId = OperationContext.Current?.SessionId;
            PlayerSession? callerSession = null;
            lock (_registryLock)
            {
                if (callerSessionId is not null)
                {
                    _sessionsById.TryGetValue(callerSessionId, out callerSession);
                }
            }

            return callerSession;
        }

        internal bool HasOpenSession(int ownerId)
        {
            bool hasOpenSession;
            lock (_registryLock)
            {
                hasOpenSession = _sessionsByOwnerId.ContainsKey(ownerId);
            }

            return hasOpenSession;
        }

        internal SessionOwner? FindOwner(int ownerId)
        {
            PlayerSession? ownerSession;
            lock (_registryLock)
            {
                _sessionsByOwnerId.TryGetValue(ownerId, out ownerSession);
            }

            SessionOwner? owner = ownerSession?.Owner;

            return owner;
        }

        internal void Close(PlayerSession closedSession)
        {
            bool wasRemoved = Remove(closedSession);
            closedSession.Abandon();
            if (wasRemoved)
            {
                _log.InfoFormat(SessionClosedMessage, closedSession.Owner.OwnerId);
                OnSessionClosed(new SessionOwnerEventArgs(closedSession.Owner.OwnerId));
            }
        }

        internal void DeclareLost(PlayerSession lostSession)
        {
            if (!Remove(lostSession))
            {
                return;
            }

            lostSession.Drop();
            _log.InfoFormat(SessionLostMessage, lostSession.Owner.OwnerId);
            OnSessionLost(new SessionOwnerEventArgs(lostSession.Owner.OwnerId));
        }

        internal void DeclareAbsent(int ownerId)
        {
            PlayerSession? absentSession;
            lock (_registryLock)
            {
                _sessionsByOwnerId.TryGetValue(ownerId, out absentSession);
            }

            if (absentSession is not null)
            {
                _log.InfoFormat(SeatAbsentMessage, ownerId);
                DeclareLost(absentSession);
            }
        }

        internal void Deliver(int ownerId, Notice notice)
        {
            PlayerSession? recipientSession;
            lock (_registryLock)
            {
                _sessionsByOwnerId.TryGetValue(ownerId, out recipientSession);
            }

            recipientSession?.Enqueue(notice);
        }

        private static void EndReplacedSession(PlayerSession? replacedSession, PlayerSession openedSession)
        {
            if (replacedSession is null)
            {
                return;
            }

            if (replacedSession.SessionId == openedSession.SessionId)
            {
                replacedSession.Abandon();
            }
            else
            {
                replacedSession.Terminate(SessionTerminationReason.AccountSignedInElsewhere);
                _log.InfoFormat(SessionReplacedMessage, replacedSession.Owner.OwnerId);
            }
        }

        private bool Remove(PlayerSession session)
        {
            bool wasRemoved = false;
            lock (_registryLock)
            {
                bool isCurrent = _sessionsByOwnerId.TryGetValue(session.Owner.OwnerId, out PlayerSession? currentSession)
                    && ReferenceEquals(currentSession, session);
                if (isCurrent)
                {
                    _sessionsByOwnerId.Remove(session.Owner.OwnerId);
                    _sessionsById.Remove(session.SessionId);
                    wasRemoved = true;
                }
            }

            return wasRemoved;
        }

        private void OnSessionOpened(SessionOwnerEventArgs e)
        {
            SessionOpened?.Invoke(this, e);
        }

        private void OnSessionClosed(SessionOwnerEventArgs e)
        {
            SessionClosed?.Invoke(this, e);
        }

        private void OnSessionLost(SessionOwnerEventArgs e)
        {
            SessionLost?.Invoke(this, e);
        }
    }
}
