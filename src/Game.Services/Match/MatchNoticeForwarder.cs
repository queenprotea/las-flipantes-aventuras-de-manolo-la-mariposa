using System.Collections.Generic;

using Game.Match;

namespace Game.Services
{
    internal sealed class MatchNoticeForwarder : IMatchListener
    {
        private readonly SessionRegistry _sessionRegistry;

        internal MatchNoticeForwarder(SessionRegistry sessionRegistry)
        {
            _sessionRegistry = sessionRegistry;
        }

        public void ReceiveMatchStarted(IReadOnlyList<MatchRecipientView> recipientViews)
        {
            foreach (MatchRecipientView recipientView in recipientViews)
            {
                _sessionRegistry.Deliver(recipientView.EntrantId, new MatchStartedNotice(recipientView.View));
            }
        }

        public void ReceiveMatchStateChanged(IReadOnlyList<MatchRecipientView> recipientViews)
        {
            foreach (MatchRecipientView recipientView in recipientViews)
            {
                _sessionRegistry.Deliver(recipientView.EntrantId, new MatchStateChangedNotice(recipientView.View));
            }
        }
    }
}
