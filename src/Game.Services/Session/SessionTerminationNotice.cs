using Game.Contracts;

namespace Game.Services
{
    internal sealed class SessionTerminationNotice : Notice
    {
        private readonly SessionTerminationReason _reason;

        internal SessionTerminationNotice(SessionTerminationReason reason)
        {
            _reason = reason;
        }

        internal override void SendTo(IGameSessionNoticeReceiver noticeReceiver)
        {
            noticeReceiver.ReceiveSessionTerminated(_reason);
        }
    }
}
