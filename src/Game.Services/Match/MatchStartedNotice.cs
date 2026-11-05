using Game.Contracts;

namespace Game.Services
{
    internal sealed class MatchStartedNotice : Notice
    {
        private readonly MatchView _matchView;

        internal MatchStartedNotice(MatchView matchView)
        {
            _matchView = matchView;
        }

        internal override void SendTo(IGameSessionNoticeReceiver noticeReceiver)
        {
            noticeReceiver.ReceiveMatchStarted(_matchView);
        }
    }
}
