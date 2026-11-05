using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

namespace Game.Match.Tests
{
    internal sealed class RecordingMatchListener : IMatchListener
    {
        private readonly List<RecordedNotice> _notices = new List<RecordedNotice>();

        internal int NoticeCount => _notices.Count;

        internal IReadOnlyList<RecordedNoticeKind> NoticeKinds => _notices.Select(notice => notice.Kind).ToList();

        public void ReceiveMatchStarted(IReadOnlyList<MatchRecipientView> recipientViews)
        {
            _notices.Add(new RecordedNotice(RecordedNoticeKind.MatchStarted, recipientViews));
        }

        public void ReceiveMatchStateChanged(IReadOnlyList<MatchRecipientView> recipientViews)
        {
            _notices.Add(new RecordedNotice(RecordedNoticeKind.MatchStateChanged, recipientViews));
        }

        internal bool HasNotified(int entrantId)
        {
            bool hasNotified = _notices.SelectMany(notice => notice.RecipientViews).Any(recipientView => recipientView.EntrantId == entrantId);

            return hasNotified;
        }

        internal MatchView FindLatestViewFor(int entrantId)
        {
            MatchView latestView = _notices.SelectMany(notice => notice.RecipientViews).Last(recipientView => recipientView.EntrantId == entrantId).View;

            return latestView;
        }
    }
}
