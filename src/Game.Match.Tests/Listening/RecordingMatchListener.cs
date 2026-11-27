using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

namespace Game.Match.Tests
{
    internal sealed class RecordingMatchListener : IMatchListener
    {
        private readonly List<RecordedNotice> _recordedNotices = new List<RecordedNotice>();
        private readonly List<int> _releasedEntrantIds = new List<int>();
        private readonly List<int> _absentEntrantIds = new List<int>();

        internal int NoticeCount => _recordedNotices.Count;

        internal IReadOnlyList<RecordedNoticeKind> NoticeKinds => _recordedNotices.Select(notice => notice.Kind).ToList();

        internal IReadOnlyList<int> ReleasedEntrantIds => _releasedEntrantIds;

        internal int FinishCount { get; private set; }

        internal IReadOnlyList<int> AbsentEntrantIds => _absentEntrantIds;

        public void ReceiveMatchStarted(IReadOnlyList<MatchRecipientView> recipientViews)
        {
            _recordedNotices.Add(new RecordedNotice(RecordedNoticeKind.MatchStarted, recipientViews));
        }

        public void ReceiveMatchStateChanged(IReadOnlyList<MatchRecipientView> recipientViews)
        {
            _recordedNotices.Add(new RecordedNotice(RecordedNoticeKind.MatchStateChanged, recipientViews));
        }

        public void ReceiveMatchFinished(IReadOnlyList<int> releasedEntrantIds, IReadOnlyList<EntrantStanding> entrantStandings)
        {
            FinishCount++;
            _releasedEntrantIds.AddRange(releasedEntrantIds);
        }

        public void ReceiveSeatAbsent(int entrantId)
        {
            _absentEntrantIds.Add(entrantId);
        }

        internal bool HasNotified(int entrantId)
        {
            bool hasNotified = _recordedNotices.SelectMany(notice => notice.RecipientViews).Any(recipientView => recipientView.EntrantId == entrantId);

            return hasNotified;
        }

        internal MatchView FindLatestViewFor(int entrantId)
        {
            MatchView latestView = _recordedNotices.SelectMany(notice => notice.RecipientViews).Last(recipientView => recipientView.EntrantId == entrantId).MatchView;

            return latestView;
        }
    }
}
