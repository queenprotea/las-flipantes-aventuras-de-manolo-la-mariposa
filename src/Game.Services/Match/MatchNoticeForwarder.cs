using System.Collections.Generic;

using Game.Contracts;
using Game.Match;

using log4net;

namespace Game.Services
{
    internal sealed class MatchNoticeForwarder : IMatchListener
    {
        private const string MatchFinishedMessage = "A match finished in room {0}.";

        private static readonly ILog _matchLog = LogManager.GetLogger(typeof(MatchNoticeForwarder));

        private readonly SessionRegistry _sessionRegistry;
        private readonly MatchRoomReturn _matchRoomReturn;

        internal MatchNoticeForwarder(SessionRegistry sessionRegistry, MatchRoomReturn matchRoomReturn)
        {
            _sessionRegistry = sessionRegistry;
            _matchRoomReturn = matchRoomReturn;
        }

        public void ReceiveMatchStarted(IReadOnlyList<MatchRecipientView> recipientViews)
        {
            foreach (MatchRecipientView recipientView in recipientViews)
            {
                _sessionRegistry.Deliver(recipientView.EntrantId, new MatchStartedNotice(recipientView.MatchView));
            }
        }

        public void ReceiveMatchStateChanged(IReadOnlyList<MatchRecipientView> recipientViews)
        {
            foreach (MatchRecipientView recipientView in recipientViews)
            {
                _sessionRegistry.Deliver(recipientView.EntrantId, new MatchStateChangedNotice(recipientView.MatchView));
            }
        }

        public void ReceiveMatchFinished(IReadOnlyList<int> releasedEntrantIds)
        {
            LiveRoom? returnedRoom = null;
            foreach (int releasedEntrantId in releasedEntrantIds)
            {
                returnedRoom ??= _matchRoomReturn.ReturnRoomOf(releasedEntrantId, RoomChangeReason.MatchFinished);
            }

            if (returnedRoom is not null)
            {
                _matchLog.InfoFormat(MatchFinishedMessage, returnedRoom.RoomId);
            }
        }
    }
}
