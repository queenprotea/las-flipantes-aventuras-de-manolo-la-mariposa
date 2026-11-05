using Game.Contracts;

namespace Game.Services
{
    internal sealed class MatchStateChangedNotice : Notice
    {
        private readonly MatchView _matchView;

        internal MatchStateChangedNotice(MatchView matchView)
        {
            _matchView = matchView;
        }

        internal override void SendTo(IGameSessionNoticeReceiver noticeReceiver)
        {
            noticeReceiver.ReceiveMatchStateChanged(_matchView);
        }
    }
}
