using System.Collections.Generic;

namespace Game.Match
{
    public interface IMatchListener
    {
        void ReceiveMatchStarted(IReadOnlyList<MatchRecipientView> recipientViews);

        void ReceiveMatchStateChanged(IReadOnlyList<MatchRecipientView> recipientViews);

        void ReceiveMatchFinished(IReadOnlyList<int> releasedEntrantIds);
    }
}
